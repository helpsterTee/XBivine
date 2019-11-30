using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using XBivine.Model;

namespace XBivine.Database
{
    class SqliteDb
    {
        static SQLiteConnection _sqliteConn;

        public SqliteDb()
        {
            _sqliteConn = new SQLiteConnection("Data Source=database.sqlite;Version=3;");
            _sqliteConn.Open();

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
            ExecuteNonQuery("CREATE TABLE projects (id integer primary key autoincrement, version integer, projectName string, author string, filename string, created date, lastChanged date);");
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

        //TODO: We need to refactor this later, this is just a quick and dirty hack for sqlite support. Create an interface and generalize db usage to multiple backend providers (MariaDB)
        public static long InsertProject(MProject p)
        {
            SQLiteCommand cm = _sqliteConn.CreateCommand();
            cm.CommandText = "INSERT INTO projects (version, projectName, author, filename, created, lastChanged) VALUES (@version, @projectName, @author, @filename, @created, @lastChanged);";
            cm.Prepare();

            cm.Parameters.AddWithValue("@version", p.ProjectVersion);
            cm.Parameters.AddWithValue("@projectName", p.ProjectName);
            cm.Parameters.AddWithValue("@author", p.Author);
            cm.Parameters.AddWithValue("@filename", p.FileName);
            cm.Parameters.AddWithValue("@created", p.Created);
            cm.Parameters.AddWithValue("@lastChanged", p.LastChanged);

            cm.ExecuteNonQuery();

            return _sqliteConn.LastInsertRowId;
        }

        public static Dictionary<int,MProject> GetProjects()
        {
            Dictionary<int, MProject> projs = new Dictionary<int, MProject>();

            SQLiteCommand cm = _sqliteConn.CreateCommand();
            cm.CommandText = "SELECT id, version, projectName, author, filename, created, lastChanged FROM projects WHERE 1;";
            SQLiteDataReader rd = cm.ExecuteReader();
            while (rd.Read())
            {
                MProject p = new MProject();
                p.ProjectID = rd.GetInt32(0);
                p.ProjectVersion = rd.GetInt32(1);
                p.ProjectName = rd.GetString(2);
                p.Author = rd.GetString(3);
                p.FileName = rd.GetString(4);
                p.Created = rd.GetDateTime(5);
                p.LastChanged = rd.GetDateTime(6);
                projs.Add(p.ProjectID, p);
            }

            return projs;
        }

        public static MProject GetProject(int projectid)
        {
            Dictionary<int, MProject> projs = new Dictionary<int, MProject>();

            SQLiteCommand cm = _sqliteConn.CreateCommand();
            cm.CommandText = "SELECT id, version, projectName, author, filename, created, lastChanged FROM projects WHERE id = @projectid;";
            cm.Prepare();

            cm.Parameters.AddWithValue("@projectid", projectid);

            SQLiteDataReader rd = cm.ExecuteReader();
            if (rd.HasRows)
            {
                rd.Read();
                MProject p = new MProject();
                p.ProjectID = rd.GetInt32(0);
                p.ProjectVersion = rd.GetInt32(1);
                p.ProjectName = rd.GetString(2);
                p.Author = rd.GetString(3);
                p.FileName = rd.GetString(4);
                p.Created = rd.GetDateTime(5);
                p.LastChanged = rd.GetDateTime(6);
                projs.Add(p.ProjectID, p);

                return p;
            }

            return null;
        }
    }
}
