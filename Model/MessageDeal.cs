using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace SocketAsyncEventArgsOfficeDemo
{
    class MessageDeal
    {

        public enum messageType
        {
            landMessage = 1,
            registMessage = 2
        }

        public static void ReceiveDeal(SocketAsyncEventArgs e)
        {
            //粘包处理后，就可以分类处理
            if (StickyDeal(e))
            {
                //如果粘包处理成功，那么就可以开始分类处理
                ClassifyDeal(e);
            }
        }

        //粘包处理
        public static bool StickyDeal(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;

            if (!token.isCopy)
            {
                //复制数据
                byte[] data = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
                //将数据放入receiveBuffer
                //receiveBuffer是list<byte>类型的
                lock (token.receiveBuffer)
                {
                    token.receiveBuffer.AddRange(data);
                }
                token.isCopy = true;
            }
                

            //粘包处理

            //接收到的数据长度还不足以分析包的类型和长度，跳出，让程序继续接收          TODO  break->return?
            if (token.receiveBuffer.Count < 8)
            {
                return false;
            }
            //如果packageLen长度为0，就得到包长
            else if (token.packageLen == 0)
            {
                //得到包的类型
                byte[] typeByte = token.receiveBuffer.GetRange(0, 4).ToArray();
                token.packageType = BitConverter.ToInt32(typeByte, 0);
                //得到包的长度
                byte[] lenBytes = token.receiveBuffer.GetRange(4, 8).ToArray();
                token.packageLen = BitConverter.ToInt32(lenBytes, 0);
            }
            //接收到的数据长度不够，跳出，让程序继续接收     TODO    先分析包的类型再决定如何做？
            if (token.receiveBuffer.Count() < token.packageLen)
            {
                return false;
            }
            //接收到的数据包最少有一个完整的数据，可以交给下面的函数处理
            else
            {
                return true;
            } 
        }

        //对完整的数据进行分类处理
        public static void ClassifyDeal(SocketAsyncEventArgs e)
        {
            Console.WriteLine("开始分类处理");
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            messageType type = (messageType)token.packageType;

            //根据数据类型处理数据
            switch (type)
            {
                case messageType.landMessage:
                    Console.WriteLine("登陆信息处理");
                    LandMessDeal(e);
                    break;

                case messageType.registMessage:
                    RegisMessDeal(e);
                    break;

                default:
                    //可能的处理
                    break;
            }
            //将这两个标志归零
            token.packageLen = 0;
            token.packageType = 0;
        }

        //登陆数据处理函数，未完成，这里直接先发一次数据
        public static void LandMessDeal(SocketAsyncEventArgs e)
        {
            MServer mServer = MServer.CreateInstance();
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            //得到一个完整的包的数据，放入新list,第二个参数是数据长度，所以要减去8  
            List<byte> onePackage = token.receiveBuffer.GetRange(8, token.packageLen - 8);
            //将复制出来的数据从receiveBuffer旧list中删除
            token.receiveBuffer.RemoveRange(0, token.packageLen);
            //list要先转换成数组，再转换成字符串
            String jsonStr = Encoding.Default.GetString(onePackage.ToArray());
            //得到用户名和密码
            JObject obj = JObject.Parse(jsonStr);
            String userName = obj["UserName"].ToString();
            String passWord = obj["PassWord"].ToString();

            //查询数据库
            bool isLand = mServer.dataBaseQuery.LoginQuery(obj["UserName"].ToString(), obj["PassWord"].ToString());
            JObject json = new JObject();
            //根据结果构建返回消息
            if (isLand)
            {
                json["isLand"] = "True";
            }
            else
            {
                json["isLand"] = "False";
            }
            String sendStr = json.ToString();
            mServer.SendMessage(1, sendStr, e);

            if (isLand)
            {
                //然后查询该用户的详细信息，并发送给该用户，消息类型为3
                sendStr = mServer.dataBaseQuery.UserInfoQuery(userName);
                mServer.SendMessage(3, sendStr, e);

                //并保存该用户的ID到userToken中
                JObject user = JObject.Parse(sendStr);
                token.UserId = user["id"].ToString();

                //查询用户的好友信息，发送给该用户，信息类型为4
                sendStr = mServer.dataBaseQuery.FriendInfoQuery(token.UserId);
                mServer.SendMessage(4, sendStr, e);


                //查询该用户的信息表是否有信息，并发送给该用户,消息类型为5
                sendStr = mServer.dataBaseQuery.UserMessageQuery(token.UserId);
                mServer.SendMessage(5, sendStr, e);
            
            }      
        }

        public static void RegisMessDeal(SocketAsyncEventArgs e)
        {
            MServer mServer = MServer.CreateInstance();
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            //得到一个完整的包的数据，放入新list,第二个参数是数据长度，所以要减去8  
            List<byte> onePackage = token.receiveBuffer.GetRange(8, token.packageLen - 8);
            //将复制出来的数据从receiveBuffer旧list中删除
            token.receiveBuffer.RemoveRange(0, token.packageLen);
            //list要先转换成数组，再转换成字符串
            String jsonStr = Encoding.Default.GetString(onePackage.ToArray());
            //得到用户名和密码
            JObject obj = JObject.Parse(jsonStr);
            //MessageBox.Show(jsonStr);
            //MessageBox.Show(obj["UserName"].ToString()+obj["PassWord"].ToString()+ obj["RealName"].ToString() +
                                            //obj["Sex"].ToString() + obj["BirthDay"].ToString() + obj["Address"].ToString() +
                                            //obj["Email"].ToString() + obj["PhoneNumber"].ToString() + obj["Remark"].ToString());

            bool isRegist = mServer.dataBaseQuery.Register(obj["UserName"].ToString(), obj["PassWord"].ToString(), obj["RealName"].ToString(),
                                            obj["Sex"].ToString(), obj["BirthDay"].ToString(), obj["Address"].ToString(),
                                            obj["Email"].ToString(), obj["PhoneNumber"].ToString(), obj["Remark"].ToString());
            JObject json = new JObject();
            if (isRegist)
            {
                json["isRegist"] = "True";
            }
            else
            {
                json["isRegist"] = "False";
            }
            String sendStr = json.ToString();
            //发送失败或者成功的消息
            mServer.SendMessage(2, sendStr, e);
        }
    }
}
