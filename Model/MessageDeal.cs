using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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
                Console.WriteLine("开始分类处理");
                ClassifyDeal(e);
            }
        }

        //粘包处理
        public static bool StickyDeal(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;

            //复制数据
            byte[] data = new byte[e.BytesTransferred];
            Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
            //将数据放入receiveBuffer
            //receiveBuffer使list<byte>类型的
            lock (token.receiveBuffer)
            {
                token.receiveBuffer.AddRange(data);
            }

            //粘包处理

            //接收到的数据长度还不足以分析包的类型和长度，跳出，让程序继续接收          TODO  break->return?
            if (token.receiveBuffer.Count < 8)
            {
                return false;
            }
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
                    //可能的处理
                    break;

                default:
                    //可能的处理
                    break;
            }
        }

        //登陆数据处理函数，未完成，这里直接先发一次数据
        public static void LandMessDeal(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            //得到一个完整的包的数据，放入新list
            List<byte> onePackage = token.receiveBuffer.GetRange(8, token.packageLen);
            //将复制出来的数据从receiveBuffer旧list中删除
            token.receiveBuffer.RemoveRange(0, token.packageLen);
            //list要先转换成数组，再转换成字符串
            String jsonStr = Encoding.Default.GetString(onePackage.ToArray());
            //得到用户名和密码
            JObject obj = JObject.Parse(jsonStr);
            String userName = obj["UserName"].ToString();
            String passWord = obj["PassWord"].ToString();
            MessageBox.Show(userName + passWord);

            //查询数据库


            int packageType = 1;
            String str = "asfafs";
            int packageLen = str.Length + 8;
            Console.WriteLine("包的大小为" + packageLen);
            byte[] bType = System.BitConverter.GetBytes(packageType);
            byte[] bLen = System.BitConverter.GetBytes(packageLen);
            //将数据放入发送buffer
            token.sendBuffer.AddRange(bType);
            token.sendBuffer.AddRange(bLen);
            token.sendBuffer.AddRange(System.Text.Encoding.Default.GetBytes(str));
            //接下来可以调用发送函数的回调函数了
            //下一次要发送多少数据
            token.sendPacketNum.Add(packageLen);
            Console.WriteLine("将信息保存进了sendBuffer");
        }
    }
}
