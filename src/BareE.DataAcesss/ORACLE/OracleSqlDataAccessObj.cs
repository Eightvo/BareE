using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Data;
using BareE.Log;
using BareE.DataAcess.Config;
using System.Diagnostics;

namespace BareE.DataAcess
{
    [DebuggerDisplay("[Oracle]{User}@{DataSource}.{Database}")]
    public class OracleSqlDataAccessObj : DataAccessObjectBase
    {

        public OracleSqlDataAccessObj(String connectionName) : base(connectionName) 
        {
            var config = sqRlConfigSection.sqRlSettings;
            var connConfig = config.Connections[connectionName] as ConnectionElement;

            if (!String.IsNullOrEmpty(connConfig["Schema"]))
                Database=connConfig["Schema"];
            if (!String.IsNullOrEmpty(connConfig["Instance"]))
                Options.Add("Instance",connConfig["Instance"]);
            if (!String.IsNullOrEmpty(connConfig["TNS"]))
            {
                Options.Add("TNS", connConfig["TNS"]);
            }
            if (!String.IsNullOrEmpty(connConfig["Port"]))
                Options.Add("Port", connConfig["Port"]);
            if (!String.IsNullOrEmpty(connConfig["Service"]))
                Options.Add("Service", connConfig["Service"]);
        }

        public override String ConnectionString
        {
            get
            {
                String ErrorString="Oracle Connections must be configured to use either a TNS using the TNS Attribute, or the connection must be specified using the Server,Port, and Service attributes.";
                StringBuilder sb = new StringBuilder();
                if (Options.ContainsKey("TNS"))
                {
                    sb.AppendFormat("Data Source = {0}; User Id = {1}; Password = {2};", Options["TNS"], User, Pass);
                }
                else
                {
                    if (!(Options.ContainsKey("Port") && Options.ContainsKey("Service")))
                        throw new Exception(ErrorString);
                    sb.AppendFormat("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2})));", Server, Options["Port"], Options["Service"]);
                    sb.AppendFormat("User Id={0};Password={1};", User, Pass);
                }
                return sb.ToString();
            }
       }

        /*
        protected String DataSource
        {
            get
            {
                String Port;
                if (Options.ContainsKey("Port")) Port = Options["Port"];
                else Port = "1521";

                String Instance;
                if (Options.ContainsKey("instance")) Instance = Options["Instance"];
                else Instance = "";
                String Service;
                if (Options.ContainsKey("Service")) Service = Options["Service"];
                else Service="";

                if (!String.IsNullOrEmpty(Server))
                {
                    return String.Format("{0}/{1}@//{2}:{3}/{4}",User,Pass,Server,Port,Service);
                    //return String.Format("{0}:{1}{2};", Server, Port, String.IsNullOrEmpty(Instance)?"":String.Concat("/",Instance));
                }
                else
                {
                    String TNS;
                    if (Options.ContainsKey("TNS")) TNS = Options["TNS"];
                    else throw new Exception("No Data Source Found.");
                    return TNS;
                }
            }
        }
        */
        protected override IDbConnection GetConnection()
        {
            OracleConnection conn = new OracleConnection(ConnectionString);
            if (String.IsNullOrEmpty(Database)) return conn;
            var cmd = new OracleCommand("alter session set current_schema=" + Database, conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conn.Close();
            return conn;
        }
        protected override IDbCommand GetCommand(string CmdText, IDbConnection conn)
        {
            OracleCommand cmd = new OracleCommand(CmdText, (OracleConnection)conn);
            if (Options.ContainsKey("command timeout"))
            {
                int i;
                if (int.TryParse("command timeout", out i))
                    ((IDbCommand)cmd).CommandTimeout = i;
            }
            return cmd;
        }
        protected override IDbDataAdapter GetAdapter(IDbCommand cmd)
        {
            return new OracleDataAdapter((OracleCommand)cmd);
        }


        OracleType getOracleType(String desc)
        {
            OracleType pType;
            switch (desc.ToUpper())
            {
                case "VARCHAR2":
                    pType = OracleType.VarChar;
                    break;
                case "DATE":
                    pType = OracleType.DateTime;
                    break;
                default:
                    pType = (OracleType)Enum.Parse(typeof(OracleType), desc, true);
                    break;
            }
            return pType;
        }

        protected override IDbDataParameter CreateParameter(string ParamName, object ParamValue, String parmType)
        {

            if (String.IsNullOrEmpty(parmType))
                return new OracleParameter(ParamName, ParamValue);
            
            OracleParameter parm= new OracleParameter(ParamName, getOracleType(parmType));
            parm.Value = ParamValue;
            return parm;

        }
        protected override IDbDataParameter CreateParameter(ParameterInformation parmInfo, SPParameterDefinition parmDef)
        {
            //string pName=String.Format("{0}_{1}",parmDef.Name,parmDef.Direction== ParameterDirection.Input?"in":"out");
            string pName = parmDef.Name;

           OracleType pType = getOracleType(parmDef.Type);

            int size_ = parmDef.Size;
            if (parmDef.Size < 0)
            {
                switch (pType)
                {
                    case OracleType.VarChar:
                        size_ = 2000;
                        break;
                }
            }


            return new OracleParameter(pName, pType, size_ >= 0 ? size_ : 0, parmDef.Direction, parmDef.Name, DataRowVersion.Current, parmDef.IsNullable, parmInfo.Value);
        }

        protected override IDataReader GetReader(IDbCommand cmd)
        {
            return cmd.ExecuteReader();
        }
        protected override string FixCommandText(string CommandText)
        {
            return CommandText;
        }
        public override bool TableExists(string tableName)
        {
            if (_knownTables.ContainsKey(tableName)) return true;
            decimal d;
            d = ToScalar<decimal>(string.Format("Select count(*) from all_tables where TABLE_NAME='{0}'", tableName));
            if (d > 1)
                throw new Exception("Ambigious table name.");
            return d == 1;
        }

        public override TableInformation GetTableInformation(string tableName)
        {

            if (_knownTables.ContainsKey(tableName)) return _knownTables[tableName];

            DataTable dt;
            //Get the metadata for the table from sp_columns
            try
            {
                dt = ToDataTable(String.Format("select * from ALL_TAB_COLUMNS where TABLE_NAME = '{0}'", tableName));
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
                    tableInfo.Qualifier = row["OWNER"].ToString();
                }
                else
                {
                    if (row["OWNER"].ToString() != tableInfo.Qualifier)
                        throw new Exception(string.Format("Ambiguous table."));
                }
                tableInfo[row["COLUMN_NAME"].ToString()] = GetColumnInformation(row);
            }
            _knownTables.Add(tableName, tableInfo);
            return tableInfo;
        }
        public override bool ColumnExists(string tableName, string columnName)
        {
            if (_knownTables.ContainsKey(tableName))
            {
                if (_knownTables[tableName].Columns.Any(ci =>String.Compare(ci.ColumnName,columnName,true)==0))
                    return true;
            }
            decimal d;
            d = ToScalar<decimal>(string.Format("select count(*) from ALL_TAB_COLUMNS WHERE TABLE_NAME ='{0}' and COLUMN_NAME='{1}'", tableName, columnName));
            if (d > 1)
                throw new Exception("Ambigious table, column pair.");
            return d == 1;
        }

        public ColumnInformation GetColumnInformation(DataRow row)
        {
            ColumnInformation ci = new ColumnInformation(row["COLUMN_NAME"].ToString(), row["NULLABLE"].ToString() != "N", false);
            //We assumed we did not need to quote the value in the constructor. If it is any of the number formats then don't change that assumtion otherwise
            //note that is does need to be quoted.
            switch (row["DATA_TYPE"].ToString())
            {
                case "NUMBER":
                    break;
                case "PLS_INTEGER": break;
                case "BINARY_INTEGER": break;
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
                dt = ToDataTable(String.Format("select * from ALL_TAB_COLUMNS where TABLE_NAME = '{0}' and COLUMN_NAME = '{1}'", tableName, columnName));
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
            String spKey=String.Format("{0}.{1}.{2}",schemaName,pkgName,procedureName);
            if (_knownSP.ContainsKey(spKey)) return _knownSP[spKey];
            
            SPInformation spinfo = new SPInformation(schemaName,pkgName,procedureName);
            DataTable dt = ToDataTable("select ARGUMENT_NAME, DATA_TYPE, IN_OUT, DATA_LENGTH from DBA_ARGUMENTS where OWNER = :s and OBJECT_NAME=:p and PACKAGE_NAME= :pkg Order by POSITION",
                                        new ParameterInformation("s", schemaName, true),
                                        new ParameterInformation("p", procedureName, true),
                                        new ParameterInformation("pkg", pkgName, true));
            
            foreach (DataRow row in dt.Rows)
            {
                ParameterDirection dir;
                switch(row["IN_OUT"].ToString().ToLower())
                {
                    case "in":
                        dir=ParameterDirection.Input;
                        break;
                    case "out":
                        dir=ParameterDirection.Output;
                        break;
                    case "in/out":
                        dir=ParameterDirection.InputOutput;
                        break;
                    default:
                        throw new InvalidCastException(String.Format("Can not convert database IN_OUT value {0} to a Parameter Direction",row["IN_OUT"]));
                }

                int size_;
                if (!int.TryParse(row["DATA_LENGTH"].ToString(),out size_)) size_=-1;
                String argName = row["ARGUMENT_NAME"].ToString();
                if (String.IsNullOrEmpty(argName))
                {
                    argName = "RETURN_VALUE";
                    dir = ParameterDirection.ReturnValue;
                }
                spinfo.Parameters.Add(new SPParameterDefinition(argName,row["DATA_TYPE"].ToString(),dir,size_));

            }
            _knownSP.Add(spKey, spinfo);
            return spinfo;
        }
        protected override string GetExecuteSPCommandText(SPInformation spInfo)
        {
            bool started = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}.{1}.{2} (",spInfo.Schema,spInfo.PackageName,spInfo.Name);
            foreach (SPParameterDefinition paramDef in spInfo.Parameters)
            {
                sb.AppendFormat("{0}:{1}_{2}", started ? ", " : "", paramDef.Name, paramDef.Direction == ParameterDirection.Input ? "in" : "out");
                started = true;
            }
            sb.AppendFormat(");");
            return sb.ToString();
            //throw new NotImplementedException();
        }

        public override string ToParameterizedName(String ParameterName)
        {
            if (!ParameterName.StartsWith(":"))
                return String.Concat(":",ParameterName);
            return ParameterName;
        }
    }
}
