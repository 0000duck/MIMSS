using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

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
                command.Parameters.AddWithValue("@password", password);
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
                command.CommandText = "create table " + id + "message" + "( friendid INT NOT NULL," + " message varchar(255) NOT NULL ," + "messagedate datetime NOT NULL" + ")";
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
    }
}
