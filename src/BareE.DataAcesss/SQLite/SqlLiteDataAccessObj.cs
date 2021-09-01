using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Data;
using BareE.DataAcess.Config;
using Microsoft.Data.Sqlite;
using BareE.DataAcess;
using BareE.DataAcess.SQLite;

namespace BareE.DataAcess
{
    [DebuggerDisplay("[SqLite]{Datafile}")]
    public class SqlLiteDataAccessObj : DataAccessObjectBase
    {
        public static SqlLiteDataAccessObj AttachTo(String filename)
        {
            SqlLiteDataAccessObj ret = new SqlLiteDataAccessObj();
            ret.Database = filename;
            return ret;
        }
        private SqlLiteDataAccessObj():base(null)
        {

        }
        public SqlLiteDataAccessObj(String connectionName)
            : base(connectionName)
        {
            var config = sqRlConfigSection.sqRlSettings;
            var connConfig = config.Connections[connectionName] as ConnectionElement;

            if (!String.IsNullOrEmpty(connConfig["Schema"]))
                Database = connConfig["Schema"];

            //if (!String.IsNullOrEmpty(connConfig["Port"]))
            //    Options.Add("Port", connConfig["Port"]);
        }

        public override string ConnectionString
        {
            get
            {
                //String Port = "3306";
                return String.Format($"Data Source={Database}");
            }
        }

        protected override System.Data.IDbConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
        }

        protected override System.Data.IDbCommand GetCommand(string CmdText, System.Data.IDbConnection conn)
        {
            return new SqliteCommand(CmdText, conn as SqliteConnection);
        }

        protected override System.Data.IDbDataAdapter GetAdapter(System.Data.IDbCommand cmd)
        {
            return new SQLiteDataAdapter(cmd as SqliteCommand);
        }

        protected override System.Data.IDataReader GetReader(System.Data.IDbCommand cmd)
        {
            return cmd.ExecuteReader();
        }

        private MySqlDbType getParameterType(String parmType)
        {
            return (MySqlDbType)Enum.Parse(typeof(MySqlDbType), parmType);
        }

        protected override System.Data.IDbDataParameter CreateParameter(string ParamName, object ParamValue, string ParmType)
        {
            if (String.IsNullOrEmpty(ParmType))
                return new SqliteParameter(ParamName, ParamValue??DBNull.Value);
            SqliteParameter parm = new SqliteParameter(ParamName, getParameterType(ParmType));
            parm.Value = ParamValue??DBNull.Value;
            return parm;
        }

        protected override System.Data.IDbDataParameter CreateParameter(ParameterInformation parmInfo, SPParameterDefinition parmDef)
        {
            SqliteParameter ret = CreateParameter(parmInfo.Name,parmInfo.Value, parmDef.Type) as SqliteParameter;
            ret.Direction = parmDef.Direction;
            ret.IsNullable = parmDef.IsNullable;
            return ret;
        }

        protected override string FixCommandText(string CommandText)
        {
            //throw new NotImplementedException();
            return CommandText;
        }

        protected override string GetExecuteSPCommandText(SPInformation spInfo)
        {
            throw new NotImplementedException();
        }

        public override bool TableExists(string tableName)
        {
            if (_knownTables.ContainsKey(tableName)) return true;
            List<ParameterInformation> parameters = new List<ParameterInformation>();
            decimal d;
            StringBuilder sb = new StringBuilder();
            sb.Append("Select count(1) FROM sqlite_master where type='table' AND name=$tb");
            parameters.Add(new ParameterInformation("tb",tableName));
            d = ToScalar<decimal>(sb.ToString(),parameters);
            if (d > 1)
                throw new Exception("Ambigious table name.");
            return d == 1;
        }

        public override TableInformation GetTableInformation(string tableName)
        {
            if (_knownTables.ContainsKey(tableName)) return _knownTables[tableName];
            StringBuilder sb = new StringBuilder();
            DataTable dt;
            //Get the metadata for the table from sp_columns
            try
            {
                dt = ToDataTable(String.Format("select * from information_schema.columns where TABLE_NAME = '{0}'", tableName));
            }
            catch (Exception innerExp)
            {
                throw new Exception(string.Format("Error creating table information for table {0}", tableName), innerExp);
            }
            if (dt.Columns.Count <= 0) throw new ArgumentException("Could not locate a table named {0}", tableName);

            TableInformation tableInfo = new TableInformation(tableName);

            bool isStarted = false;
            //Columns = new Dictionary<string, ColumnInformation>(StringComparer.InvariantCultureIgnoreCase);


            foreach (DataRow row in dt.Rows)
            {
                //Ensure a unique table was specified
                if (!isStarted)
                {
                    tableInfo.Name = row["TABLE_NAME"].ToString();
                    tableInfo.Qualifier = row["TABLE_SCHEMA"].ToString();
                }
                else
                {
                    if (row["TABLE_SCHEMA"].ToString() != tableInfo.Qualifier)
                        throw new Exception(string.Format("Ambiguous table."));
                }
                tableInfo[row["COLUMN_NAME"].ToString()] = GetColumnInformation(row);
            }
            _knownTables.Add(tableName, tableInfo);
            return tableInfo;

        }

        public override bool ColumnExists(string tableName, string columnName)
        {
            throw new NotImplementedException();
        }
        public ColumnInformation GetColumnInformation(DataRow row)
        {
            ColumnInformation ci = new ColumnInformation(row["COLUMN_NAME"].ToString(), row["IS_NULLABLE"].ToString() != "NO)", false);
            //We assumed we did not need to quote the value in the constructor. If it is any of the number formats then don't change that assumtion otherwise
            //note that is does need to be quoted.
            switch (row["DATA_TYPE"].ToString().ToLower())
            {
                case "int":
                case "decimal":
                case "bindary":
                case "bigint":
                case "double":
                case "float":
                case "mediumint":
                case "smallint":
                case "tinyint":
                    break;
                default:
                    ci.isQuoted = true;
                    break;
            }
            return ci;
        }

        public override ColumnInformation GetColumnInformation(string tableName, string columnName)
        {
            if (_knownTables.ContainsKey(tableName))
            {
                ColumnInformation ci = _knownTables[tableName].Columns.FirstOrDefault(sci => String.Compare(sci.ColumnName, columnName, true) == 0);
                if (ci != null)
                    return ci;
            }

            DataTable dt;
            //Get the metadata for the table from sp_columns
            try
            {
                dt = ToDataTable(String.Format("select * from Information_Schema where TABLE_NAME = '{0}' and COLUMN_NAME = '{1}'", tableName, columnName));
            }
            catch (Exception innerExp)
            {
                throw new Exception(string.Format("Error creating column information for table {0}", tableName), innerExp);
            }
            if (dt.Columns.Count <= 0) throw new ArgumentException("Could not locate a table named {0}", tableName);
            if (dt.Columns.Count > 1) throw new ArgumentException("Ambigious column name.");
            return GetColumnInformation(dt.Rows[0]);
        }

        public override SPInformation GetSPInformation(string schemaName, string pkgName, string procedureName)
        {
            throw new NotImplementedException();
        }

        public override string ToParameterizedName(string ParameterName)
        {
            return String.Concat("@", ParameterName);
            //throw new NotImplementedException();
        }
    }
}
