using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Newtonsoft.Json;

namespace P2Pchatt
{
    public class Database
    {
        SQLiteConnection m_dbConnection;

        public Database()
        {
            m_dbConnection = new SQLiteConnection("Data Source=ChatDatabase.db;Version=3;");
            m_dbConnection.Open();
        }
        public Database(string username)
        {
            //open/ init database 
            m_dbConnection = new SQLiteConnection("Data Source=ChatDatabase.db;Version=3;");
            m_dbConnection.Open();

            string sql = "create table if not exists chats (name varchar(20) unique)";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        
            sql = "insert or replace into chats (name) values ('" + username + "') ";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        public void AddToChatHistory(DataPacket packet, string username)
        {
            string sql = "create table if not exists " + username + " (datapacket text)";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            string stringpacket = JsonConvert.SerializeObject(packet);
            sql = "insert into " + username + " (datapacket) values ( '" + stringpacket +  "')";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();   
        }

        public List<string> RetrieveFromChatHistory(string tablename)
        {
            List<string> list = new List<string>();
            string sql = "select * from " +tablename;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = command.ExecuteReader();
            while(reader.Read())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }

    }
}
