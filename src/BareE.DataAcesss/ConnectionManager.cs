using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BareE.DataAcess.Config;
using BareE.DataAcess.MySql;
using BareE.DataAcess.SQLite;

namespace BareE.DataAcess
{
    public static class ConnectionManager
    {
        static Dictionary<String, IDataAccessObject> _daos=new Dictionary<string,IDataAccessObject>();
        public static IDataAccessObject Get(String connectionName)
        {
            return Get(connectionName,true);
        }
        public static IDataAccessObject Get(String connectionName, bool reuse)
        {
            if (reuse && _daos.ContainsKey(connectionName))
                return _daos[connectionName];
            if (sqRlConfigSection.sqRlSettings.Connections.ContainsKey(connectionName))
            {
                IDataAccessObject ret;
                switch(sqRlConfigSection.sqRlSettings.Connections[connectionName].Type.ToLower())
                {
                    case "sqlite":
                        ret = new SqlLiteDataAccessObj(connectionName);
                        break;
                    case "oracle":
                        ret = new OracleSqlDataAccessObj(connectionName);
                        break;
                    case "sql":
                        ret = new SqlDataAccessObj(connectionName);
                        break;
                    case "mysql":

                        ret = new MySqlDataAccessObj(connectionName);
                        break;
                    default:
                        ret = null;
                        break;
                }
                if (!_daos.ContainsKey(connectionName)) _daos.Add(connectionName, null);
                _daos[connectionName] = ret;
                return ret;
            }
            return null;
        }
    }
}
