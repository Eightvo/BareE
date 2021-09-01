using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using BareE.Log;
using System.Diagnostics;

namespace BareE.DataAcess
{
    [DebuggerDisplay("[{Type}]{User}@{Server}.{Database}")]
    public class SqlDataAccessObj : DataAccessObjectBase
    {

        public SqlDataAccessObj(String connectionName) : base(connectionName) { }
        public SqlDataAccessObj(String server, String database, int timeout):base(String.Empty)
        {
            Server = server;
            Database = database;
            if (!Options.ContainsKey("Timeout"))
                Options.Add("Timeout", String.Empty);
            Options["Timeout"] = timeout.ToString();

            UseWidowsAuthentication = true;
        }
        public SqlDataAccessObj(String server, String database, String user, String pass, int timeout):base(String.Empty)
        {
            Server = server;
            Database = database;
            User = user;
            Pass = pass;
            if (!Options.ContainsKey("Timeout"))
                Options.Add("Timeout", String.Empty);
            Options["Timeout"] = timeout.ToString();

        }


        public bool UseWidowsAuthentication { get; set; }

        string WindowsAuthConnString
        {
            get
            {
                var timeout = "30";
                if (Options.ContainsKey("timeout"))
                    timeout = Options["timeout"];
                return $"server={Server};database={Database};Integrated Security=true;Connection Timeout={timeout}";
            }
        }

        public override string ConnectionString
        {
            get
            {
                if (UseWidowsAuthentication) return WindowsAuthConnString;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("server={0};", Server);
                sb.AppendFormat("database={0};", Database);
                sb.AppendFormat("User ID={0};", User);
                sb.AppendFormat("pwd={0};", Pass);

                if (Options.ContainsKey("timeout"))
                    sb.AppendFormat("Connection Timeout={0};", Options["timeout"]);

                return sb.ToString();
            }
        }

        protected override IDbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
        protected override IDbCommand GetCommand(string CmdText, IDbConnection conn)
        {
            SqlCommand cmd = new SqlCommand(CmdText, (SqlConnection)conn);
            if (Options.ContainsKey("command timeout"))
            {
                int i;
                if (int.TryParse(Options["command timeout"], out i))
                    cmd.CommandTimeout = i;
            }
            return cmd;
        }
        protected override IDbDataAdapter GetAdapter(IDbCommand cmd)
        {
            return new SqlDataAdapter((SqlCommand)cmd);
        }

        protected override IDbDataParameter CreateParameter(string ParamName, object ParamValue,String parmType)
        {
            if (String.IsNullOrEmpty(parmType))
                return new SqlParameter(ParamName, ParamValue);
            /*Get the sql parameter type)*/
            SqlParameter parm = new SqlParameter(ParamName, Enum.Parse(typeof(SqlDbType), parmType));
            parm.Value = ParamValue;
            return parm;
        }


        protected override IDataReader GetReader(IDbCommand cmd)
        {
            return cmd.ExecuteReader();
        }

        protected override string FixCommandText(string CommandText)
        {
            CommandText = CommandText.Replace("'", "''");
            return CommandText;
        }

        public override bool TableExists(string tableName)
        {
            return ToScalar<int>(String.Format("Select count(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{0}'", tableName)) >= 1;
        }

        public override TableInformation GetTableInformation(string tableName)
        {
            DataTable dt;
            //Get the metadata for the table from sp_columns
            try
            {
                dt = ToDataTable(String.Format("Exec sp_columns '{0}'", tableName));
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
                    tableInfo.Qualifier = row["TABLE_QUALIFIER"].ToString();
                }
                else
                {
                    if (row["TABLE_QUALIFIER"].ToString() != tableInfo.Qualifier)
                        throw new Exception(string.Format("Ambiguous table."));
                }
                tableInfo[row["COLUMN_NAME"].ToString()] = GetColumnInformation(row);
            }
            return tableInfo;
        }

        public override bool ColumnExists(string tableName, string columnName)
        {
            return ToScalar<int>(String.Format("Select count(1) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{0}' and COLUMN_NAME='{1}'", tableName, columnName)) >= 1;
        }

        public override ColumnInformation GetColumnInformation(string tableName, string columnName)
        {
            ColumnInformation colInfo = new ColumnInformation();
            DataTable dt = ToDataTable(string.Format("exec sp_columns  @table_name='{0}',@column_name='{1}'", tableName, columnName));
            if (dt.Rows.Count < 1) throw new Exception("Column not found.");
            DataRow sp_columns_row = dt.Rows[0];
            return GetColumnInformation(sp_columns_row);
        }

        ColumnInformation GetColumnInformation(DataRow sp_columns_row)
        {
            ColumnInformation colInfo = new ColumnInformation();
            colInfo.ColumnName = sp_columns_row["COLUMN_NAME"].ToString();
            colInfo.DataType = sp_columns_row["TYPE_NAME"].ToString();
            //colInfo.ColumnLength = int.Parse(sp_columns_row["LENGTH"].ToString());
            colInfo.isNullable = sp_columns_row["NULLABLE"].ToString() == "1";

            SqlDbType ColumnDataType = (SqlDbType)Enum.Parse(typeof(SqlDbType), colInfo.DataType, true);

            colInfo.isQuoted = false;
            switch (ColumnDataType)
            {
                case SqlDbType.Char: colInfo.isQuoted = true; break;
                case SqlDbType.Date: colInfo.isQuoted = true; break;
                case SqlDbType.DateTime: colInfo.isQuoted = true; break;
                case SqlDbType.DateTime2: colInfo.isQuoted = true; break;
                case SqlDbType.DateTimeOffset: colInfo.isQuoted = true; break;
                case SqlDbType.NChar: colInfo.isQuoted = true; break;
                case SqlDbType.NText: colInfo.isQuoted = true; break;
                case SqlDbType.NVarChar: colInfo.isQuoted = true; break;
                case SqlDbType.SmallDateTime: colInfo.isQuoted = true; break;
                case SqlDbType.Structured: colInfo.isQuoted = true; break;
                case SqlDbType.Text: colInfo.isQuoted = true; break;
                case SqlDbType.Time: colInfo.isQuoted = true; break;
                case SqlDbType.Timestamp: colInfo.isQuoted = true; break;
                case SqlDbType.VarChar: colInfo.isQuoted = true; break;
                case SqlDbType.Xml: colInfo.isQuoted = true; break;
            }
            return colInfo;
        }

        public override SPInformation GetSPInformation(string schema, string package, string procedure)
        {
            throw new NotImplementedException();
        }
        protected override string GetExecuteSPCommandText(SPInformation spInfo)
        {
            throw new NotImplementedException();
        }
        protected override IDbDataParameter CreateParameter(ParameterInformation parmInfo, SPParameterDefinition parmDef)
        {
            throw new NotImplementedException();
        }
        public override string ToParameterizedName(String ParameterName)
        {
            if (!ParameterName.StartsWith(":"))
                return String.Concat("@", ParameterName);
            return ParameterName;
        }

    }
}
