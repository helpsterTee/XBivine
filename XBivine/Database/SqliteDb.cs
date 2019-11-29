using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace XBivine.Database
{
    class SqliteDb
    {
        SQLiteConnection _sqliteConn;

        public SqliteDb()
        {
            this._sqliteConn = new SQLiteConnection("Data Source=database.sqlite;Version=3;");
            this._sqliteConn.Open();

            //init
            CreateSchema();
        }

        private void CreateSchema()
        {
            SQLiteCommand cm = _sqliteConn.CreateCommand();
            cm.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='projects';";
            string name = (string)cm.ExecuteScalar();
            
            if (!String.IsNullOrEmpty(name))
            {
                Console.WriteLine("[SQLite]: Schema exists, skipping");
                return;
            }

            Console.WriteLine("[SQLite]: Creating Schema");
            ExecuteNonQuery("CREATE TABLE settings (id integer primary key autoincrement, key string, value string);");
            ExecuteNonQuery("CREATE TABLE projects (id integer primary key autoincrement, version integer, projectName string, author string, created date, lastChanged date);");
            ExecuteNonQuery("CREATE TABLE ifcEntity (id integer primary key autoincrement, ifcClass string, guid string, name string, description string, parentId integer, projectId integer, FOREIGN KEY (projectId) REFERENCES projects(id));");
        }

        private SQLiteCommand ExecuteNonQuery(string commandText)
        {
            SQLiteCommand comm = _sqliteConn.CreateCommand();
            comm.CommandText = commandText;
            comm.ExecuteNonQuery();
            return comm;
        }

        private object ExecuteScalar(string commandText)
        {
            SQLiteCommand comm = _sqliteConn.CreateCommand();
            comm.CommandText = commandText;
            return comm.ExecuteScalar();
        }

        private void InitSettings()
        {
            ExecuteNonQuery("INSERT INTO settings (key, value) VALUES ('version', '1')");
        }

    }
}
