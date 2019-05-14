using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace MIMSS.Model
{
    class DataBaseQuery
    {
        static String str;

        //数据库初始化
        public DataBaseQuery(string database, string source, string id, string password, string sslmode)
        {
            //数据库连接
            str = "Database="+database+";Data Source="+source+";User id="+id+";Password="+password+";SslMode="+sslmode+";";
        }
        //拿到一个数据库连接
        public static MySqlConnection GetDataConn()
        {
            MySqlConnection mySql = new MySqlConnection(DataBaseQuery.str);
            return mySql;
        }


        //打开数据库
        public void  DataBaseOpen(MySqlConnection mySql)
        {
            mySql.Open();
        }
        //关闭数据库
        public void DataBaseClose(MySqlConnection mySql)
        {
            mySql.Close();
        }
        //查询某个用户是否存在
        public bool UserQuery(string username)
        {
            bool isexsit;
            MySqlConnection mySql = DataBaseQuery.GetDataConn();
            mySql.Open();
            try
            {    
                MySqlCommand command = new MySqlCommand("select * from userid where username = @name", mySql);
                command.Parameters.AddWithValue("@name", username);
                MySqlDataReader mysqldr = command.ExecuteReader();
                //Read()会返回bool类型，是否有下一条数据
                isexsit = mysqldr.Read();
                
            }
            catch (Exception ex)
            {
                Console.Write("UserQuery Error : " + ex);
                isexsit = false;
            }
            mySql.Close();
            return isexsit;
            
        }
        //用户登陆
        public bool LoginQuery(string username, string password)
        {
            bool isLogin;
            MySqlConnection mySql = DataBaseQuery.GetDataConn();
            mySql.Open();
            try
            {
                //利用mySql进行查询操作
                MySqlCommand command = new MySqlCommand("select * from userid where username = @name and password = @pass", mySql);
                command.Parameters.AddWithValue("@name", username);
                command.Parameters.AddWithValue("@pass", password);
                //执行查询操作并返回查询结果
                MySqlDataReader mysqldr = command.ExecuteReader();
                //Read()会返回bool类型，是否有下一条数据
                isLogin = mysqldr.Read();
                mysqldr.Close();
                return isLogin;
            }
            catch (Exception ex)
            {
                Console.Write("LoginQuery Error : " + ex);
                isLogin = false;
            }
            mySql.Close();
            return isLogin;
        }
        
        //查询用户信息
        public String UserInfoQuery(string username)
        {
            MySqlConnection mySql = DataBaseQuery.GetDataConn();
            mySql.Open();
            JObject obj = new JObject();
            try
            {
                //利用mySql进行查询操作
                MySqlCommand command = new MySqlCommand("select * from userid where username = @name", mySql);
                command.Parameters.AddWithValue("@name", username);
                //执行查询操作并返回查询结果
                MySqlDataReader mysqldr = command.ExecuteReader();
                //取得id和username
                mysqldr.Read();
                obj["id"] = mysqldr[0].ToString();
                obj["UserName"] = mysqldr[1].ToString();
                mysqldr.Close();
                //取得userinformation中的信息
                command.CommandText = "select * from userinformation where id = @id";
                command.Parameters.AddWithValue("@id", obj["id"].ToString());
                mysqldr = command.ExecuteReader();
                mysqldr.Read();
                obj["RealName"] = mysqldr[1].ToString();
                obj["Sex"] = mysqldr[2].ToString();
                obj["BirthDay"] = mysqldr[3].ToString();
                obj["Address"] = mysqldr[4].ToString();
                obj["Email"] = mysqldr[5].ToString();
                obj["PhoneNumber"] = mysqldr[6].ToString();
                obj["Remark"] = mysqldr[7].ToString();
                obj["isOk"] = "True";

                mysqldr.Close();
            }
            catch (Exception ex)
            {
                Console.Write("LoginQuery Error : " + ex);
                obj["isOk"] = "False";
            }
            String str = obj.ToString();
            mySql.Close();
            return str;
        }

        //查询好友信息，保存在json数组中
        public String FriendInfoQuery(string id)
        {
            MySqlConnection mySql = DataBaseQuery.GetDataConn();
            mySql.Open();
            JArray obj = new JArray();
            try
            {
                String tablename = id + "friend";
                //利用mySql进行查询操作
                MySqlCommand command = mySql.CreateCommand();
                command.CommandText = "select * from " + tablename;
                //执行查询操作并返回查询结果
                MySqlDataReader mysqldr = command.ExecuteReader();
                //新建一个String List用来保存好友的id 
                List<String> friendidList = new List<String>();
                //取得好友id和分组
                int i = 0;
                while (mysqldr.Read())
                {
                    JObject temp = new JObject();
                    obj.Add(temp);
                    //将好友id加入friendidList
                    friendidList.Add(mysqldr[0].ToString());
                    //将好友分组加入JSON ARRAY
                    obj[i]["id"] = mysqldr[0].ToString();
                    obj[i]["Group"] = mysqldr[1].ToString();
                    i++;
                }
                mysqldr.Close();

                i = 0;
                //现在开始循环查询好友的用户名
                
                foreach (var friendid in friendidList)
                {
                    command.CommandText = "select * from userid where id =" + friendid;
                    //command.Parameters.AddWithValue("@id", friendid);
                    mysqldr = command.ExecuteReader();
                    mysqldr.Read();

                    //将好友的用户名添加进JSON数组
                    obj[i]["UserName"] = mysqldr[1].ToString();
                    i++;
                    mysqldr.Close();
                }
                
                i = 0;
                //现在开始循环查询好友的详细信息
                foreach (var friendid in friendidList)
                {
                    command.CommandText = "select * from userinformation where id =" + friendid;
                    //command.Parameters.AddWithValue("@friendid", friendid);
                    mysqldr = command.ExecuteReader();
                    mysqldr.Read();

                    //将好友的详细数据添加进JSON数组
                    obj[i]["RealName"] = mysqldr[0].ToString();
                    obj[i]["Sex"] = mysqldr[1].ToString();
                    obj[i]["BirthDay"] = mysqldr[2].ToString();
                    obj[i]["Address"] = mysqldr[3].ToString();
                    obj[i]["Email"] = mysqldr[4].ToString();
                    obj[i]["PhoneNumber"] = mysqldr[5].ToString();
                    obj[i]["Remarks"] = mysqldr[6].ToString();
                    i++;
                    mysqldr.Close();
                }

                obj[0]["isOk"] = "True";
                
            }
            catch (Exception ex)
            {
                Console.Write("FriendInfoQuery Error : " + ex);
                obj[0]["isOk"] = "False";
            }
            String str = obj.ToString();
            mySql.Close();
            return str;
        }

        //查询用户消息表，保存在json数组中
        public String UserMessageQuery(string id)
        {
            MySqlConnection mySql = DataBaseQuery.GetDataConn();
            mySql.Open();
            JArray obj = new JArray();
            try
            {
                String tablename = id + "message";
                //利用mySql进行查询操作
                MySqlCommand command = mySql.CreateCommand();
                //只用查询还没有发送的
                command.CommandText = "select * from " + tablename + " where issend=0";
                //执行查询操作并返回查询结果
                MySqlDataReader mysqldr = command.ExecuteReader();
                //将数据放图
                int i = 0;
                while (mysqldr.Read())
                {
                    JObject temp = new JObject();
                    obj.Add(temp);
                    obj[i]["FriendId"] = mysqldr[0].ToString();
                    obj[i]["Message"] = mysqldr[1].ToString();
                    obj[i]["MessageDate"] = mysqldr[2].ToString();
                    i++;
                }
                mysqldr.Close();

                //将读取出来的信息更新为已读
                command.CommandText = "update " + tablename + " set issend = 1";
                command.ExecuteNonQuery(); 

                if (obj.Count == 0)
                {
                    mysqldr.Close();
                    mySql.Close();
                    return "null";
                }

                obj[0]["isOk"] = "True";

                mysqldr.Close();
            }
            catch (Exception ex)
            {
                Console.Write("UserMessageQuery Error : " + ex);
                obj[0]["isOk"] = "False";
            }
            String str = obj.ToString();
            mySql.Close();
            return str;
        }

        //用户注册
        public bool Register(string username, string password, string realname, string sex, string birthday, string address, string email, string phonenumber, string remarks)
        {
            bool isRegist;
            MySqlConnection mySql = DataBaseQuery.GetDataConn();
            mySql.Open();

            //如果用户名已经被注册，那么注册失败
            if (this.UserQuery(username))
            {
                return false;
            }

            //事务开始
            MySqlTransaction transaction = mySql.BeginTransaction();
            MySqlCommand command = mySql.CreateCommand();
            command.Transaction = transaction;

            try
            {
                //插入userid表
                command.CommandText = "insert into  userid (username, password) values(@name, @password)";
                command.Parameters.AddWithValue("@name", username);
                command.Parameters.AddWithValue("@password", password);
                command.ExecuteNonQuery();
                //插入userinformation表
                command.CommandText = "insert into userinformation (id,realname,sex,birthday,address,email,phonenumber,remarks) " +
                                      "values(@@IDENTITY,@realname,@sex,@birthday,@address,@email,@phonenumber,@remarks)";
                command.Parameters.AddWithValue("@realname", realname);
                command.Parameters.AddWithValue("@sex", sex);
                command.Parameters.AddWithValue("@birthday", birthday);
                command.Parameters.AddWithValue("@address", address);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@phonenumber", phonenumber);
                command.Parameters.AddWithValue("@remarks", remarks);
                command.ExecuteNonQuery();
                //插入userstatus表
                command.CommandText = "insert into userstatus (id) values(@@IDENTITY)";
                command.ExecuteNonQuery();
                //得到刚才自增的id
                command.CommandText = "select @@IDENTITY";
                //得到查询的结果集,即自增id
                MySqlDataReader reader = command.ExecuteReader();
                reader.Read();
                string id = reader[0].ToString();
                reader.Close();
                //建立好友表
                command.CommandText = "create table " + id + "friend" + "( friendid INT NOT NULL,"+" friendgroup varchar(20) NOT NULL, "+"PRIMARY KEY ( friendid ))";
                command.ExecuteNonQuery();
                //建立消息表
                command.CommandText = "create table " + id + "message" + "( friendid INT NOT NULL," + " message varchar(255) NOT NULL ," + "messagedate datetime NOT NULL," + "issend int(2) NOT NULL default 0" + ")";
                command.ExecuteNonQuery();
                //如果提交了，那么就返回true
                transaction.Commit();
                isRegist = true;
            }
            catch (Exception ex)
            {
                //出错了，回滚，返回false
                transaction.Rollback();
                Console.Write("Register Error : " + ex);
                isRegist = false;
            }
            mySql.Close();
            return isRegist;
        }

        //保存聊天信息
        public void SaveChat(String SendId, String Message, String MessageDate, String ReceiveId)
        {
            //拿到数据库连接
            MySqlConnection mySql = DataBaseQuery.GetDataConn();
            mySql.Open();

            try
            {
                MySqlCommand command = mySql.CreateCommand();
                command.CommandText = "insert into " + ReceiveId + "message" + " (friendid, message, messagedate) values (@friendid, @message, @messagedate)";
                command.Parameters.AddWithValue("@friendid", SendId);
                command.Parameters.AddWithValue("@message", Message);
                command.Parameters.AddWithValue("@messagedate", MessageDate);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.Write("UserMessageQuery Error : " + ex);
            }
            mySql.Close();
        }

        //用来模糊查询用户
        public String QueryAddFriend(String searchString)
        {
            //拿到数据库连接
            MySqlConnection mySql = DataBaseQuery.GetDataConn();
            mySql.Open();
            JArray jArray = new JArray();
            try
            {
                MySqlCommand command = mySql.CreateCommand();
                command.CommandText = "select * from userid where username like @searchString";
                command.Parameters.AddWithValue("@searchString", "%"+ searchString +"%");
                MySqlDataReader mysqldr = command.ExecuteReader();
                while (mysqldr.Read())
                {
                    JObject obj = new JObject();
                    jArray.Add(obj);
                    obj["Id"] = mysqldr[0].ToString();
                    obj["UserName"] = mysqldr[1].ToString();
                }

                if (jArray.Count == 0)
                {
                    JObject obj = new JObject();
                    jArray.Add(obj);
                    obj["isOk"] = "False";
                    String fastr = jArray.ToString();
                    return fastr;
                }

                jArray[0]["isOk"] = "True";
                
            }
            catch (Exception ex)
            {
                jArray[0]["isOk"] = "False";
                Console.Write("QueryAddFriend Error : " + ex);
            }
            mySql.Close();
            String str = jArray.ToString();
            return str;    
        }
    }

    
}
