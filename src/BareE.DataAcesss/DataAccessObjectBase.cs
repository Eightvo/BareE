using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BareE.Log;
using BareE.DataAcess.Config;
using BareE.DataAcess.Exceptions;

namespace BareE.DataAcess
{
    public abstract class DataAccessObjectBase : IDataAccessObject
    {

        protected Dictionary<String, TableInformation> _knownTables = new Dictionary<string, TableInformation>();
        protected Dictionary<String, SPInformation> _knownSP = new Dictionary<string, SPInformation>();

        private IDbTransaction _currTransaction = null;
        private int _Attempts;
        private int _Wait;
        private Dictionary<string, string> _Options;
        private ILog _TraceLog { get; set; }
        private ILog _DisabledLog;
        private bool _loggingEnabled=true;
        public bool LoggingEnabled
        {
            get
            {
                return _loggingEnabled;
            }
            set
            {
                if (value && !_loggingEnabled)
                {
                    _TraceLog = _DisabledLog;
                    _DisabledLog = new BareE.Log.NullLogger();
                }
                if (!value && _loggingEnabled)
                {
                    _DisabledLog = _TraceLog;
                    _TraceLog = new BareE.Log.NullLogger();
                }
                _loggingEnabled = value;
            }
        }

        public DataAccessObjectBase(String connectionName)
        {
            if (String.IsNullOrEmpty(connectionName))
                return;
            var config = sqRlConfigSection.sqRlSettings;
            var connConfig = config.Connections[connectionName];
            Server = connConfig.Server;
            User = connConfig.User;
            LogQuery = connConfig.LogQuery;
            LogParms = connConfig.LogParams;
            LogResults = connConfig.LogResults;

            if (!String.IsNullOrEmpty(connConfig.EncryptedPass))
                throw new Exception("Encrypted Passwords not yet implemented.");
            else if (String.IsNullOrEmpty(connConfig.Pass))
                throw new Exception(String.Format("Either Pass or Encrypted Pass must be set in configurationation for sqRl connection {0}", connectionName));
            else Pass = connConfig.Pass;

            Database = connConfig.Database;

            Options["fixcommands"] = connConfig.FixCommands ? "true" : "false";

            if (!String.IsNullOrEmpty(connConfig.Timeout))
            {
                if (!Options.ContainsKey("timeout")) Options.Add("timeout", "");
                Options["timeout"] = connConfig.Timeout;
            }

            if (!String.IsNullOrEmpty(connConfig.CommandTimeout))
            {
                if (!Options.ContainsKey("command timeout")) Options.Add("command timeout", "");
                Options["command timeout"] = connConfig.Timeout;
            }

            if (!String.IsNullOrEmpty(connConfig.TraceLog))
            {
                TraceLog = BareE.Log.LogManager.Get(connConfig.TraceLog);
            }




        }

        public int Attempts { get { if (_Attempts < 1) return 1; return _Attempts; } set { _Attempts = value; } }
        public int Wait { get { if (_Wait < 0) return 0; return _Wait; } set { _Wait = value; } }
        public string Server { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
        public string Database { get; set; }
        public Dictionary<string, string> Options { get { if (_Options == null) { _Options = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase); return _Options; } return _Options; } set { _Options = value; } }
        public ILog TraceLog { get { if (_TraceLog == null) return new NullLogger(); return _TraceLog; } set { _TraceLog = value; } }

        public bool LogQuery { get; set; }
        public bool LogParms { get; set; }
        public bool LogResults { get; set; }

        public abstract string ConnectionString { get; }

        protected abstract IDbConnection GetConnection();
        protected abstract IDbCommand GetCommand(String CmdText, IDbConnection conn);
        protected abstract IDbDataAdapter GetAdapter(IDbCommand cmd);
        protected abstract IDataReader GetReader(IDbCommand cmd);
        
        protected IDbDataParameter CreateParameter(ParameterInformation paramInfo)
        {
            return CreateParameter(paramInfo.Name, paramInfo.Value,paramInfo.Type);
        }
        protected IDbDataParameter CreateParameter(String ParamName,Object ParamValue)
        {
            return CreateParameter(ParamName, ParamValue, String.Empty);
        }
        protected abstract IDbDataParameter CreateParameter(String ParamName, Object ParamValue,String ParmType);
        protected abstract IDbDataParameter CreateParameter(ParameterInformation parmInfo, SPParameterDefinition parmDef);
        protected abstract String FixCommandText(String CommandText);

        private String BeforeCommand(String CmdText)
        {
            if (ConnectionString.Length <= 0)
            {
                TraceLog.Log("Invalid Connection String.", BareE.Log.LogMessageLevel.Critical);
                throw new Exception("Invalid Connection String.");
            }

            String NormalizedCmdText;
            if (Options.ContainsKey("fixcommands"))
                NormalizedCmdText = FixCommandText(CmdText);
            else
                NormalizedCmdText = CmdText;

            if (LogQuery) TraceLog.Log(NormalizedCmdText);
            return NormalizedCmdText;
        }
        private void AddParameters(IDbCommand cmd, IEnumerable<ParameterInformation> parmInfo)
        {
            foreach (ParameterInformation pi in parmInfo)
            {
                var enumerable = pi.Value as System.Collections.IEnumerable;
                if (enumerable != null && pi.Value.GetType()!=typeof(String) && String.IsNullOrEmpty(pi.Type))
                {
                    String replaceStr = ToParameterizedName(pi.Name);
                    int pCount = 0;
                    StringBuilder replacementStr = new StringBuilder();
                    StringBuilder logStr = new StringBuilder();
                    foreach (var item in enumerable)
                    {
                        replacementStr.Append(String.Concat(pCount>0?",":"",replaceStr, pCount.ToString()));
                        logStr.Append(String.Format("{0}{1}{2}{1}",pCount>0?",":"",pi.IsQuoted?"'":"",item.ToString()));
                        cmd.Parameters.Add(CreateParameter(String.Concat(replaceStr, pCount), item));
                        pCount++;
                    }
                    cmd.CommandText = cmd.CommandText.Replace(replaceStr, String.Concat("(", replacementStr.ToString(), ")"));
                    if (LogParms) TraceLog.Log(String.Format("\t{0}=({1})", pi.Name, logStr));

                }
                else
                {
                    if (LogParms) TraceLog.Log(string.Format("\t{0}={2}{1}{2}", pi.Name, pi.Value, pi.IsQuoted ? "'" : ""));
                    cmd.Parameters.Add(CreateParameter(pi.Name, pi.Value,pi.Type));
                }
            }
        }

        #region DirectQueries
        #region ProcessDataReader
        public int ProcessDataReader(string cmdText, Func<IDataReader, Boolean> callBack)
        {
            return ProcessDataReader(cmdText, 0, callBack, new List<ParameterInformation>());
        }
        public int ProcessDataReader(String cmdText, Func<IDataReader, Boolean> callBack, IEnumerable<ParameterInformation> parmInfo)
        {
            return ProcessDataReader(cmdText, 0, callBack, parmInfo);
        }
        public int ProcessDataReader(String cmdText, Func<IDataReader, Boolean> callBack, params ParameterInformation[] parms)
        {
            return ProcessDataReader(cmdText, callBack, parms.ToList());
        }
        public int ProcessDataReader(string cmdText, int cmdTimeout, Func<IDataReader, Boolean> callBack, params ParameterInformation[] parmInfo) { return ProcessDataReader(cmdText, cmdTimeout, callBack, parmInfo.ToList()); }
        public int ProcessDataReader(string cmdText, int cmdTimeout, Func<IDataReader, Boolean> callBack, IEnumerable<ParameterInformation> parmInfo)
        {
            cmdText = BeforeCommand(cmdText);
            using (IDbConnection conn = GetConnection())
            {
                using (IDbCommand cmd = GetCommand(cmdText, conn))
                {
                    if (cmdTimeout > 0)
                        cmd.CommandTimeout = cmdTimeout;

                    AddParameters(cmd, parmInfo);
                    Exception prevException = null;
                    int attemptCount = 0;
                    while (attemptCount < Attempts)
                    {
                        try
                        {
                            cmd.Connection.Open();
                            IDataReader rdr = cmd.ExecuteReader();
                            int rCount = 0;
                            Boolean Continue = true;
                            while (Continue && rdr.Read())
                            {
                                rCount++;
                                Continue = callBack(rdr);
                            }
                            cmd.Connection.Close();
                            return rCount;
                        }
                        catch (Exception curException)
                        {
                            if (prevException == null || curException.Message != prevException.Message)
                            {
                                attemptCount = 1;
                                prevException = curException;
                            }
                            else
                                attemptCount++;
                        }
                        TraceLog.Log(String.Format("Failure {0} : {1} \n{2}", attemptCount, prevException.Message, prevException.StackTrace));
                        System.Threading.Thread.Sleep(Wait * 1000);
                    }
                    TraceLog.Log(String.Format("Aborting : {0}\n{1}", prevException.Message, prevException.StackTrace));
                    throwCommandException(cmd, prevException);
                    throw prevException; //This line will never execute b/c a command exception will be thrown in the above function. 
                    //This is just a place holder because other wise "Not all code paths return a value."
                }
            }
        }
        
        public int ProcessDataReader(QueryInformation qInfo,Func<IDataReader,Boolean> callBack)
        {
            if (qInfo==null) throw new ArgumentException("Null query information","qInfo");
            if (qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a stored procedure using a direct query method.");
            try
            {
                return ProcessDataReader(qInfo.QueryString, callBack, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }
        #endregion

        #region DataReaderWrapper
        public DataReaderWrapper ToDataReaderWrapper(string cmdText) { return ToDataReaderWrapper(cmdText, 0, new List<ParameterInformation>()); }
        public DataReaderWrapper ToDataReaderWrapper(String cmdText, IEnumerable<ParameterInformation> parmInfo)
        {
            return ToDataReaderWrapper(cmdText, 0, parmInfo);
        }
        public DataReaderWrapper ToDataReaderWrapper(String cmdText, params ParameterInformation[] parms)
        {
            return ToDataReaderWrapper(cmdText, 0, parms.ToList());
        }
        public DataReaderWrapper ToDataReaderWrapper(String cmdText, int cmdTimeout, params ParameterInformation[] parms)
        {
            return ToDataReaderWrapper(cmdText, cmdTimeout, parms.ToList());
        }
        public DataReaderWrapper ToDataReaderWrapper(string cmdText, int cmdTimeout, IEnumerable<ParameterInformation> parmInfo)
        {
            cmdText = BeforeCommand(cmdText);
            IDbConnection conn = GetConnection();
            IDbCommand cmd = GetCommand(cmdText, conn);
            if (cmdTimeout > 0) cmd.CommandTimeout = cmdTimeout;
            AddParameters(cmd, parmInfo);

            Exception prevException = null;
            int attemptCount = 0;
            while (attemptCount < Attempts)
            {
                try
                {
                    return new DataReaderWrapper(conn, cmd,LogResults?TraceLog:null);
                }
                catch (Exception curException)
                {
                    if (cmd.Connection.State != ConnectionState.Closed)
                        cmd.Connection.Close();

                    if (prevException == null || curException.Message != prevException.Message)
                    {
                        attemptCount = 1;
                        prevException = curException;
                    }
                    else
                        attemptCount++;
                }
                TraceLog.Log(String.Format("Failure {0} : {1} \n{2}", attemptCount, prevException.Message, prevException.StackTrace));
                System.Threading.Thread.Sleep(Wait * 1000);
            }
            TraceLog.Log(String.Format("Aborting : {0}\n{1}", prevException.Message, prevException.StackTrace));
            throwCommandException(cmd, prevException);
            throw prevException; //This line will never execute b/c a command exception will be thrown in the above function. 
            //This is just a place holder because other wise "Not all code paths return a value."
        }

        public DataReaderWrapper ToDataReaderWrapper(QueryInformation qInfo)
        {
            if (qInfo == null) throw new ArgumentException("Null query information", "qInfo");
            if (qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a stored procedure using a direct query method.");
            try
            {
                return ToDataReaderWrapper(qInfo.QueryString, 0, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }

        #endregion

        #region ToDataTable
        public DataTable ToDataTable(string cmdText)
        {
            return ToDataTable(cmdText, 0, new List<ParameterInformation>());
        }
        public DataTable ToDataTable(String cmdText, params ParameterInformation[] parms) { return ToDataTable(cmdText, 0, parms.ToList()); }
        public DataTable ToDataTable(String cmdText, IEnumerable<ParameterInformation> parmInfo)
        {
            return ToDataTable(cmdText, 0, parmInfo);
        }
        public DataTable ToDataTable(String cmdText, int cmdTimeout, params ParameterInformation[] parms) { return ToDataTable(cmdText, cmdTimeout, parms.ToList()); }
        public DataTable ToDataTable(string cmdText, int cmdTimeout, IEnumerable<ParameterInformation> parmInfo)
        {
            cmdText = BeforeCommand(cmdText);
            //DataTable dt = new DataTable();
            using (IDbConnection conn = GetConnection())
            {
                using (IDbCommand cmd = GetCommand(cmdText, conn))
                {
                    if (cmdTimeout > 0)
                        cmd.CommandTimeout = cmdTimeout;
                    AddParameters(cmd, parmInfo);
                    IDbDataAdapter adapter = GetAdapter(cmd);

                    Exception prevException = null;
                    int attemptCount = 0;
                    while (attemptCount < Attempts)
                    {
                        try
                        {
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);
                                if (LogResults)
                                {
                                    int rCount = 0;

                                    if (ds != null)
                                    {
                                        DataTable dt;
                                        if (ds.Tables.Count>0)
                                            dt = ds.Tables[0];
                                        else 
                                            dt=new DataTable();

                                        foreach (DataRow r in dt.Rows)
                                        {
                                            bool started = false;
                                            rCount++;
                                            StringBuilder sb = new StringBuilder();
                                            foreach (DataColumn dc in dt.Columns)
                                            {
                                                sb.AppendFormat("{0}{1}", started ? " ," : "", r[dc.ColumnName].ToString());
                                                started = true;
                                            }
                                            TraceLog.Log(sb.ToString());
                                        }
                                        TraceLog.Log(String.Format("{0} Rows Returned.", rCount));
                                    }
                                }
                                if (ds.Tables.Count > 0)
                                    return ds.Tables[0];
                                else return new DataTable();
                            //}
                        }
                        catch (Exception curException)
                        {
                            if (cmd.Connection.State != ConnectionState.Closed)
                                cmd.Connection.Close(); //CloseConnection(cmd.Connection);

                            if (prevException == null || curException.Message != prevException.Message)
                            {
                                attemptCount = 1;
                                prevException = curException;
                            }
                            else
                                attemptCount++;
                        }
                        TraceLog.Log(String.Format("Failure {0} : {1} \n{2}", attemptCount, prevException.Message, prevException.StackTrace));
                        System.Threading.Thread.Sleep(Wait * 1000);
                    }
                    TraceLog.Log(String.Format("Aborting : {0}\n{1}", prevException.Message, prevException.StackTrace));
                    throwCommandException(cmd, prevException);//throw prevException;
                    throw prevException; //This line will never execute b/c a command exception will be thrown in the above function. 
                    //This is just a place holder because other wise "Not all code paths return a value."
                }
            }
        }

        public DataTable ToDataTable(QueryInformation qInfo)
        {
            if (qInfo == null) throw new ArgumentException("Null query information", "qInfo");
            if (qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a stored procedure using a direct query method.");
            try
            {
                return ToDataTable(qInfo.QueryString, 0, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }
        #endregion


        #region ToScalar
        public T ToScalar<T>(string cmdText)
        {
            return ToScalar<T>(cmdText, 0, new List<ParameterInformation>());
        }
        public T ToScalar<T>(String cmdText, params ParameterInformation[] parms) { return ToScalar<T>(cmdText, parms.ToList()); }
        public T ToScalar<T>(String cmdText, IEnumerable<ParameterInformation> parmInfo)
        {
            return ToScalar<T>(cmdText, 0, parmInfo);
        }
        public T ToScalar<T>(String cmdText, int cmdTimeOUt, params ParameterInformation[] parms) { return ToScalar<T>(cmdText, cmdTimeOUt, parms.ToList()); }
        public T ToScalar<T>(string cmdText, int cmdTimeout, IEnumerable<ParameterInformation> parmInfo)
        {
            cmdText = BeforeCommand(cmdText);
            using (IDbConnection conn = GetConnection())
            {
                using (IDbCommand cmd = GetCommand(cmdText, conn))
                {
                    if (cmdTimeout > 0)
                        cmd.CommandTimeout = cmdTimeout;
                    AddParameters(cmd, parmInfo);
                    Exception prevException = null;
                    int attemptCount = 0;
                    while (attemptCount < Attempts)
                    {
                        try
                        {
                            cmd.Connection.Open();//OpenConnection(cmd.Connection);
                            T ret = default(T);
                            Object temp = cmd.ExecuteScalar();
                            if (!(temp == null || temp.GetType() == typeof(DBNull)))
                                ret = (T)Convert.ChangeType(temp, typeof(T));
                            if (LogResults)
                                TraceLog.Log("Result = "+ret.ToString());
                            cmd.Connection.Close();//CloseConnection(cmd.Connection);
                            return ret;
                        }
                        catch (Exception curException)
                        {
                            if (cmd.Connection.State != ConnectionState.Closed)
                                cmd.Connection.Close();//CloseConnection(cmd.Connection);
                            if (prevException == null || curException.Message != prevException.Message)
                            {
                                attemptCount = 1;
                                prevException = curException;
                            }
                            else
                                attemptCount++;
                        }
                        TraceLog.Log(String.Format("Failure {0} : {1} \n{2}", attemptCount, prevException.Message, prevException.StackTrace));
                        System.Threading.Thread.Sleep(Wait * 1000);
                    }
                    TraceLog.Log(String.Format("Aborting : {0}\n{1}", prevException.Message, prevException.StackTrace));
                    throwCommandException(cmd,prevException);
                    throw prevException; //This line will never execute b/c a command exception will be thrown in the above function. 
                    //This is just a place holder because other wise "Not all code paths return a value."
                }
            }
        }


        public T ToScalar<T>(QueryInformation qInfo)
        {
            if (qInfo == null) throw new ArgumentException("Null query information", "qInfo");
            if (qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a stored procedure using a direct query method.");
            try
            {
                return ToScalar<T>(qInfo.QueryString, 0, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }
        #endregion

        #region NonQuery
        public int NonQuery(string cmdText){return NonQuery(cmdText, 0, new List<ParameterInformation>());}
        public int NonQuery(String cmdText, params ParameterInformation[] parms) {return NonQuery(cmdText, 0, parms.ToList()); }
        public int NonQuery(String cmdText, IEnumerable<ParameterInformation> parmInfo){return NonQuery(cmdText, 0, parmInfo);}
        public int NonQuery(String cmdText, int cmdTimeout, params ParameterInformation[] parms) {return NonQuery(cmdText, cmdTimeout, parms.ToList()); }
        public int NonQuery(string cmdText, int cmdTimeout, IEnumerable<ParameterInformation> parmInfo)
        {
            cmdText = BeforeCommand(cmdText);
            using (IDbConnection conn = GetConnection())
            {
                using (IDbCommand cmd = GetCommand(cmdText, conn))
                {

                    if (_currTransaction != null)
                    {
                        cmd.Transaction = _currTransaction;
                        cmd.Connection = cmd.Transaction.Connection;
                    }

                    if (cmdTimeout > 0) cmd.CommandTimeout = cmdTimeout;
                    AddParameters(cmd, parmInfo);
                    Exception prevException = null;
                    int attemptCount = 0;
                    while (attemptCount < Attempts)
                    {
                        try
                        {
                            OpenConnection(cmd.Connection);
                            int ret = cmd.ExecuteNonQuery();
                            CloseConnection(cmd.Connection);
                            if (LogResults)
                                TraceLog.Log(ret.ToString() + " affected rows.");
                            return ret;
                        }
                        catch (Exception curException)
                        {
                            if (cmd.Connection.State != ConnectionState.Closed)
                                CloseConnection(cmd.Connection);

                            if (prevException == null || curException.Message != prevException.Message)
                            {
                                attemptCount = 1;
                                prevException = curException;
                            }
                            else
                                attemptCount++;
                        }
                        TraceLog.Log(String.Format("Failure {0} : {1} \n{2}", attemptCount, prevException.Message, prevException.StackTrace));
                        System.Threading.Thread.Sleep(Wait * 1000);
                    }
                    TraceLog.Log(String.Format("Aborting : {0}\n{1}", prevException.Message, prevException.StackTrace));
                    throwCommandException(cmd,prevException);
                    throw prevException; //This line will never execute b/c a command exception will be thrown in the above function. 
                    //This is just a place holder because other wise "Not all code paths return a value."
                }
            }
        }

        public int NonQuery(QueryInformation qInfo)
        {
            if (qInfo == null) throw new ArgumentException("Null query information", "qInfo");
            if (qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a stored procedure using a direct query method.");
            try
            {
                return NonQuery(qInfo.QueryString, 0, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }
        #endregion
        #endregion

        #region StoredProcedures
        protected abstract String GetExecuteSPCommandText(SPInformation spInfo);
        #region SPNonQuery
        public int SPNonQuery(String storedProcedureName, params ParameterInformation[] parmInfo) { return SPNonQuery(Database, String.Empty, storedProcedureName, 0, parmInfo.ToList()); }
        public int SPNonQuery(String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPNonQuery(Database, String.Empty, storedProcedureName, 0, parmInfo); }
        public int SPNonQuery(String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPNonQuery(Database, String.Empty, storedProcedureName, timeOut, parmInfo.ToList()); }
        public int SPNonQuery(String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPNonQuery(Database, String.Empty, storedProcedureName, timeOut, parmInfo); }

        public int SPNonQuery(String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPNonQuery(Database, package, storedProcedureName, 0, parmInfo.ToList()); }
        public int SPNonQuery(String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPNonQuery(Database, package, storedProcedureName, 0, parmInfo); }
        public int SPNonQuery(String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPNonQuery(Database, package, storedProcedureName, timeOut, parmInfo.ToList()); }
        public int SPNonQuery(String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPNonQuery(Database, package, storedProcedureName, timeOut, parmInfo); }

        public int SPNonQuery(String schema, String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPNonQuery(schema, package, storedProcedureName, 0, parmInfo.ToList()); }
        public int SPNonQuery(String schema, String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPNonQuery(schema, package, storedProcedureName, 0, parmInfo); }
        public int SPNonQuery(String schema, String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPNonQuery(schema, package, storedProcedureName, timeOut, parmInfo.ToList()); }
        public int SPNonQuery(String schema, String package, String procName, int timeOut, IEnumerable<ParameterInformation> parameters)
        {
            int affectedRows = 0;
            String spKey = String.Format("{0}{1}{2}", !String.IsNullOrEmpty(schema) ? String.Concat(schema, ".") : "", !String.IsNullOrEmpty(package) ? String.Concat(package, ".") : "", procName);
            SPInformation procInfo = GetSPInformation(schema, package, procName);
            if (procInfo == null)
                throw new Exception(String.Format("Can't find any stored procedure {0}", spKey));

            String cmdText = spKey;
            using (IDbConnection conn = GetConnection())
            {
                using (IDbCommand cmd = GetCommand(cmdText, conn))
                {
                    if (_currTransaction != null)
                    {
                        cmd.Transaction = _currTransaction;
                        cmd.Connection = _currTransaction.Connection;
                    }

                    if (timeOut > 0) cmd.CommandTimeout = timeOut;
                    if (parameters.Count() < procInfo.InputCount)
                        throw new ArgumentException("Too few arguments supplied.");

                    foreach (SPParameterDefinition def in procInfo.Parameters)
                    {
                        if (def.Direction == ParameterDirection.Input || def.Direction == ParameterDirection.InputOutput)
                        {
                            foreach (ParameterInformation parm in parameters)
                            {
                                if (String.Compare(def.Name, parm.Name, true) == 0)
                                {
                                    cmd.Parameters.Add(CreateParameter(parm, def));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(CreateParameter(new ParameterInformation(def.Name, DBNull.Value), def));
                        }
                    }
                    IDataAdapter adapter = GetAdapter(cmd);


                    cmd.CommandType = CommandType.StoredProcedure;
                    Exception prevException = null;
                    int attemptCount = 0;
                    while (attemptCount < Attempts)
                    {
                        try
                        {
                            OpenConnection(cmd.Connection);
                            affectedRows = cmd.ExecuteNonQuery();
                            CloseConnection(cmd.Connection);

                            foreach (IDbDataParameter resultParam in cmd.Parameters)
                            {
                                if (resultParam.Direction == ParameterDirection.InputOutput || resultParam.Direction == ParameterDirection.Output || resultParam.Direction == ParameterDirection.ReturnValue)
                                {
                                    foreach (ParameterInformation pi in parameters)
                                    {
                                        if (String.Compare(pi.Name, resultParam.ParameterName, true) == 0)
                                        {
                                            pi.Value = resultParam.Value;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (LogResults)
                                TraceLog.Log(String.Format("{0} Rows Affected", affectedRows));
                            return affectedRows;
                        }
                        catch (Exception curException)
                        {
                            if (cmd.Connection.State != ConnectionState.Closed)
                                CloseConnection(cmd.Connection);
                            if (prevException == null || curException.Message != prevException.Message)
                            {
                                attemptCount = 1;
                                prevException = curException;
                            }
                            else
                                attemptCount++;
                        }
                        TraceLog.Log(String.Format("Failure {0} : {1} \n{2}", attemptCount, prevException.Message, prevException.StackTrace));
                        System.Threading.Thread.Sleep(Wait * 1000);
                    }
                    TraceLog.Log(String.Format("Aborting : {0}\n{1}", prevException.Message, prevException.StackTrace));
                    throwCommandException(cmd, prevException);
                    throw prevException; //This line will never execute b/c a command exception will be thrown in the above function. 
                    //This is just a place holder because other wise "Not all code paths return a value."

                }
            }
        }

        public int SPNonQuery(QueryInformation qInfo)
        {
            if (qInfo == null) throw new ArgumentException("Null query information", "qInfo");
            if (!qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a direct query using a stored procedure method.");
            try
            {
                return SPNonQuery(qInfo.QueryString, 0, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }

        #endregion
        #region SPToDataReaderWrapper
        public DataReaderWrapper SPToDataReaderWrapper(String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToDataReaderWrapper(Database, String.Empty, storedProcedureName, 0, parmInfo.ToList()); }
        public DataReaderWrapper SPToDataReaderWrapper(String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToDataReaderWrapper(Database, String.Empty, storedProcedureName, 0, parmInfo); }
        public DataReaderWrapper SPToDataReaderWrapper(String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToDataReaderWrapper(Database, String.Empty, storedProcedureName, timeOut, parmInfo.ToList()); }
        public DataReaderWrapper SPToDataReaderWrapper(String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPToDataReaderWrapper(Database, String.Empty, storedProcedureName, timeOut, parmInfo); }

        public DataReaderWrapper SPToDataReaderWrapper(String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToDataReaderWrapper(Database, package, storedProcedureName, 0, parmInfo.ToList()); }
        public DataReaderWrapper SPToDataReaderWrapper(String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToDataReaderWrapper(Database, package, storedProcedureName, 0, parmInfo); }
        public DataReaderWrapper SPToDataReaderWrapper(String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToDataReaderWrapper(Database, package, storedProcedureName, timeOut, parmInfo.ToList()); }
        public DataReaderWrapper SPToDataReaderWrapper(String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPToDataReaderWrapper(Database, package, storedProcedureName, timeOut, parmInfo); }

        public DataReaderWrapper SPToDataReaderWrapper(String schema, String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToDataReaderWrapper(schema, package, storedProcedureName, 0, parmInfo.ToList()); }
        public DataReaderWrapper SPToDataReaderWrapper(String schema, String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToDataReaderWrapper(schema, package, storedProcedureName, 0, parmInfo); }
        public DataReaderWrapper SPToDataReaderWrapper(String schema, String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToDataReaderWrapper(schema, package, storedProcedureName, timeOut, parmInfo.ToList()); }
        public DataReaderWrapper SPToDataReaderWrapper(String schema, String package, String procName, int timeOut, IEnumerable<ParameterInformation> parameters)
        {
            String spKey = String.Format("{0}{1}{2}", !String.IsNullOrEmpty(schema) ? String.Concat(schema, ".") : "", !String.IsNullOrEmpty(package) ? String.Concat(package, ".") : "", procName);
            SPInformation procInfo = GetSPInformation(schema, package, procName);
            if (procInfo == null)
                throw new CommandException(String.Format("Can't find any stored procedure {0}", spKey));

            String cmdText = spKey;
            using (IDbConnection conn = GetConnection())
            {
                using (IDbCommand cmd = GetCommand(cmdText, conn))
                {
                    if (timeOut > 0) cmd.CommandTimeout = timeOut;
                    if (parameters.Count() < procInfo.InputCount)
                        throw new ArgumentException("Too few arguments supplied.");

                    foreach (SPParameterDefinition def in procInfo.Parameters)
                    {
                        if (def.Direction == ParameterDirection.Input || def.Direction == ParameterDirection.InputOutput)
                        {
                            foreach (ParameterInformation parm in parameters)
                            {
                                if (String.Compare(def.Name, parm.Name, true) == 0)
                                {
                                    cmd.Parameters.Add(CreateParameter(parm, def));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(CreateParameter(new ParameterInformation(def.Name, DBNull.Value), def));
                        }
                    }
                    IDataAdapter adapter = GetAdapter(cmd);


                    cmd.CommandType = CommandType.StoredProcedure;
                    Exception prevException = null;
                    int attemptCount = 0;
                    while (attemptCount < Attempts)
                    {
                        try
                        {
                            cmd.Connection.Open();
                            DataReaderWrapper ret = new DataReaderWrapper(cmd.Connection, cmd,LogResults?TraceLog:null);
                            cmd.Connection.Close();

                            foreach (IDbDataParameter resultParam in cmd.Parameters)
                            {
                                if (resultParam.Direction == ParameterDirection.InputOutput || resultParam.Direction == ParameterDirection.Output || resultParam.Direction == ParameterDirection.ReturnValue)
                                {
                                    foreach (ParameterInformation pi in parameters)
                                    {
                                        if (String.Compare(pi.Name, resultParam.ParameterName, true) == 0)
                                        {
                                            pi.Value = resultParam.Value;
                                            break;
                                        }
                                    }
                                }
                            }
                            return ret;
                        }
                        catch (Exception curException)
                        {
                            if (cmd.Connection.State != ConnectionState.Closed)
                                cmd.Connection.Close();
                            if (prevException == null || curException.Message != prevException.Message)
                            {
                                attemptCount = 1;
                                prevException = curException;
                            }
                            else
                                attemptCount++;
                        }
                        TraceLog.Log(String.Format("Failure {0} : {1} \n{2}", attemptCount, prevException.Message, prevException.StackTrace));
                        System.Threading.Thread.Sleep(Wait * 1000);
                    }
                    TraceLog.Log(String.Format("Aborting : {0}\n{1}", prevException.Message, prevException.StackTrace));
                    throwCommandException (cmd,prevException);
                    throw prevException; //This line will never execute b/c a command exception will be thrown in the above function. 
                    //This is just a place holder because other wise "Not all code paths return a value."

                }
            }

        }

        public DataReaderWrapper SPToDataReaderWrapper(QueryInformation qInfo)
        {
            if (qInfo == null) throw new ArgumentException("Null query information", "qInfo");
            if (!qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a direct query using a stored procedure method.");
            try
            {
                return SPToDataReaderWrapper(qInfo.QueryString, 0, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }
        #endregion
        #region SPToScalar
        public T SPToScalar<T>(String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToScalar<T>(Database, String.Empty, storedProcedureName, 0, parmInfo.ToList()); }
        public T SPToScalar<T>(String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToScalar<T>(Database, String.Empty, storedProcedureName, 0, parmInfo); }
        public T SPToScalar<T>(String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToScalar<T>(Database, String.Empty, storedProcedureName, timeOut, parmInfo.ToList()); }
        public T SPToScalar<T>(String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPToScalar<T>(Database, String.Empty, storedProcedureName, timeOut, parmInfo); }

        public T SPToScalar<T>(String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToScalar<T>(Database, package, storedProcedureName, 0, parmInfo.ToList()); }
        public T SPToScalar<T>(String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToScalar<T>(Database, package, storedProcedureName, 0, parmInfo); }
        public T SPToScalar<T>(String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToScalar<T>(Database, package, storedProcedureName, timeOut, parmInfo.ToList()); }
        public T SPToScalar<T>(String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPToScalar<T>(Database, package, storedProcedureName, timeOut, parmInfo); }

        public T SPToScalar<T>(String schema, String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToScalar<T>(schema, package, storedProcedureName, 0, parmInfo.ToList()); }
        public T SPToScalar<T>(String schema, String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToScalar<T>(schema, package, storedProcedureName, 0, parmInfo); }
        public T SPToScalar<T>(String schema, String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToScalar<T>(schema, package, storedProcedureName, timeOut, parmInfo.ToList()); }
        public T SPToScalar<T>(String schema, String package, String procName, int timeOut, IEnumerable<ParameterInformation> parameters)
        {
            String spKey = String.Format("{0}{1}{2}", !String.IsNullOrEmpty(schema) ? String.Concat(schema, ".") : "", !String.IsNullOrEmpty(package) ? String.Concat(package, ".") : "", procName);
            SPInformation procInfo = GetSPInformation(schema, package, procName);
            if (procInfo == null)
                throw new CommandException(String.Format("Can't find any stored procedure {0}", spKey));

            String cmdText = spKey;
            using (IDbConnection conn = GetConnection())
            {
                using (IDbCommand cmd = GetCommand(cmdText, conn))
                {
                    if (timeOut > 0) cmd.CommandTimeout = timeOut;
                    if (parameters.Count() < procInfo.InputCount)
                        throw new ArgumentException("Too few arguments supplied.");

                    foreach (SPParameterDefinition def in procInfo.Parameters)
                    {
                        if (def.Direction == ParameterDirection.Input || def.Direction == ParameterDirection.InputOutput)
                        {
                            foreach (ParameterInformation parm in parameters)
                            {
                                if (String.Compare(def.Name, parm.Name, true) == 0)
                                {
                                    cmd.Parameters.Add(CreateParameter(parm, def));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(CreateParameter(new ParameterInformation(def.Name, DBNull.Value), def));
                        }
                    }
                    IDataAdapter adapter = GetAdapter(cmd);


                    cmd.CommandType = CommandType.StoredProcedure;
                    Exception prevException = null;
                    int attemptCount = 0;
                    while (attemptCount < Attempts)
                    {
                        try
                        {
                            cmd.Connection.Open();
                            T ret = default(T);
                            Object temp = cmd.ExecuteScalar();
                            if (!(temp == null || temp.GetType() == typeof(DBNull)))
                                ret = (T)Convert.ChangeType(temp, typeof(T));
                            cmd.Connection.Close();

                            foreach (IDbDataParameter resultParam in cmd.Parameters)
                            {
                                if (resultParam.Direction == ParameterDirection.InputOutput || resultParam.Direction == ParameterDirection.Output || resultParam.Direction == ParameterDirection.ReturnValue)
                                {
                                    foreach (ParameterInformation pi in parameters)
                                    {
                                        if (String.Compare(pi.Name, resultParam.ParameterName, true) == 0)
                                        {
                                            pi.Value = resultParam.Value;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (LogResults)
                                TraceLog.Log(ret.ToString()+"\n1 Result");
                            return ret;
                        }
                        catch (Exception curException)
                        {
                            if (cmd.Connection.State != ConnectionState.Closed)
                                cmd.Connection.Close();
                            if (prevException == null || curException.Message != prevException.Message)
                            {
                                attemptCount = 1;
                                prevException = curException;
                            }
                            else
                                attemptCount++;
                        }
                        TraceLog.Log(String.Format("Failure {0} : {1} \n{2}", attemptCount, prevException.Message, prevException.StackTrace));
                        System.Threading.Thread.Sleep(Wait * 1000);
                    }
                    TraceLog.Log(String.Format("Aborting : {0}\n{1}", prevException.Message, prevException.StackTrace));
                    throwCommandException(cmd, prevException);
                    throw prevException; //This line will never execute b/c a command exception will be thrown in the above function. 
                    //This is just a place holder because other wise "Not all code paths return a value."

                }
            }

        }


        public T SPToScalar<T>(QueryInformation qInfo)
        {
            if (qInfo == null) throw new ArgumentException("Null query information", "qInfo");
            if (!qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a direct query using a stored procedure method.");
            try
            {
                return SPToScalar<T>(qInfo.QueryString, 0, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }

        #endregion
        #region SPToDataRow
        public DataRow SPToDataRow(String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToDataRow(Database, String.Empty, storedProcedureName, 0, parmInfo.ToList()); }
        public DataRow SPToDataRow(String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToDataRow(Database, String.Empty, storedProcedureName, 0, parmInfo); }
        public DataRow SPToDataRow(String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToDataRow(Database, String.Empty, storedProcedureName, timeOut, parmInfo.ToList()); }
        public DataRow SPToDataRow(String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPToDataRow(Database, String.Empty, storedProcedureName, timeOut, parmInfo); }

        public DataRow SPToDataRow(String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToDataRow(Database, package, storedProcedureName, 0, parmInfo.ToList()); }
        public DataRow SPToDataRow(String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToDataRow(Database, package, storedProcedureName, 0, parmInfo); }
        public DataRow SPToDataRow(String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToDataRow(Database, package, storedProcedureName, timeOut, parmInfo.ToList()); }
        public DataRow SPToDataRow(String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPToDataRow(Database, package, storedProcedureName, timeOut, parmInfo); }

        public DataRow SPToDataRow(String schema, String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToDataRow(schema, package, storedProcedureName, 0, parmInfo.ToList()); }
        public DataRow SPToDataRow(String schema, String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToDataRow(schema, package, storedProcedureName, 0, parmInfo); }
        public DataRow SPToDataRow(String schema, String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToDataRow(schema, package, storedProcedureName, timeOut, parmInfo.ToList()); }

        public DataRow SPToDataRow(String schema, String package, String procName, int timeOut, IEnumerable<ParameterInformation> parameters)
        {
            DataTable dt = SPToDataTable(schema, package, procName, timeOut, parameters);
            if (dt == null) return new DataTable().NewRow();
            if (dt.Rows.Count >= 1) return dt.Rows[0];
            return dt.NewRow();
        }

        public DataRow SPToDataRow(QueryInformation qInfo)
        {
            if (qInfo == null) throw new ArgumentException("Null query information", "qInfo");
            if (!qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a direct query using a stored procedure method.");
            try
            {
                return SPToDataRow(qInfo.QueryString, 0, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }

        #endregion
        #region SPToDataTable
        public DataTable SPToDataTable(String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToDataTable(Database, String.Empty, storedProcedureName, 0, parmInfo.ToList()); }
        public DataTable SPToDataTable(String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToDataTable(Database, String.Empty, storedProcedureName, 0, parmInfo); }
        public DataTable SPToDataTable(String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToDataTable(Database, String.Empty, storedProcedureName, timeOut, parmInfo.ToList()); }
        public DataTable SPToDataTable(String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPToDataTable(Database, String.Empty, storedProcedureName, timeOut, parmInfo); }

        public DataTable SPToDataTable(String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToDataTable(Database, package, storedProcedureName, 0, parmInfo.ToList()); }
        public DataTable SPToDataTable(String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToDataTable(Database, package, storedProcedureName, 0, parmInfo); }
        public DataTable SPToDataTable(String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToDataTable(Database, package, storedProcedureName, timeOut, parmInfo.ToList()); }
        public DataTable SPToDataTable(String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo) { return SPToDataTable(Database, package, storedProcedureName, timeOut, parmInfo); }

        public DataTable SPToDataTable(String schema, String package, String storedProcedureName, params ParameterInformation[] parmInfo) { return SPToDataTable(schema, package, storedProcedureName, 0, parmInfo.ToList()); }
        public DataTable SPToDataTable(String schema, String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo) { return SPToDataTable(schema, package, storedProcedureName, 0, parmInfo); }
        public DataTable SPToDataTable(String schema, String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo) { return SPToDataTable(schema, package, timeOut, parmInfo.ToList()); }

        public DataTable SPToDataTable(String schema, String package, String procName, int timeOut, IEnumerable<ParameterInformation> parameters)
        {
            String spKey = String.Format("{0}{1}{2}", !String.IsNullOrEmpty(schema) ? String.Concat(schema, ".") : "", !String.IsNullOrEmpty(package) ? String.Concat(package, ".") : "", procName);
            SPInformation procInfo = GetSPInformation(schema, package, procName);
            if (procInfo == null)
                throw new CommandException(String.Format("Can't find any stored procedure {0}", spKey));

            String cmdText = spKey;
            using (IDbConnection conn = GetConnection())
            {
                using (IDbCommand cmd = GetCommand(cmdText, conn))
                {
                    if (timeOut > 0) cmd.CommandTimeout = timeOut;
                    if (parameters.Count() < procInfo.InputCount)
                        throw new ArgumentException("Too few arguments supplied.");

                    foreach (SPParameterDefinition def in procInfo.Parameters)
                    {
                        if (def.Direction == ParameterDirection.Input || def.Direction == ParameterDirection.InputOutput)
                        {
                            foreach (ParameterInformation parm in parameters)
                            {
                                if (String.Compare(def.Name, parm.Name, true) == 0)
                                {
                                    cmd.Parameters.Add(CreateParameter(parm, def));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(CreateParameter(new ParameterInformation(def.Name, DBNull.Value), def));
                        }
                    }
                    IDataAdapter adapter = GetAdapter(cmd);


                    cmd.CommandType = CommandType.StoredProcedure;
                    Exception prevException = null;
                    int attemptCount = 0;
                    while (attemptCount < Attempts)
                    {
                        try
                        {
                            cmd.Connection.Open();
                            DataSet ds = new DataSet();
                            adapter.Fill(ds);
                            DataTable dt = new DataTable();
                            if (ds.Tables.Count > 0)
                                dt = ds.Tables[0];
                            cmd.Connection.Close();
                            
                            foreach (IDbDataParameter resultParam in cmd.Parameters)
                            {
                                if (resultParam.Direction == ParameterDirection.InputOutput || resultParam.Direction == ParameterDirection.Output || resultParam.Direction == ParameterDirection.ReturnValue)
                                {
                                    foreach (ParameterInformation pi in parameters)
                                    {
                                        if (String.Compare(pi.Name, resultParam.ParameterName, true) == 0)
                                        {
                                            pi.Value = resultParam.Value;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (LogResults)
                            {
                                int rCount=0;

                                if (dt!=null)
                                foreach (DataRow r in dt.Rows)
                                {
                                    bool started=false;
                                    rCount++;
                                    StringBuilder sb = new StringBuilder();
                                    foreach (DataColumn dc in dt.Columns)
                                    {
                                        sb.AppendFormat("{0}{1}", started ? " ," : "", r[dc.ColumnName].ToString());
                                        started=true;
                                    }
                                    TraceLog.Log(sb.ToString());
                                }
                                TraceLog.Log(String.Format("{0} Rows Returned.", rCount));
                            }
                            return dt;
                        }
                        catch (Exception curException)
                        {
                            if (cmd.Connection.State != ConnectionState.Closed)
                                cmd.Connection.Close();
                            if (prevException == null || curException.Message != prevException.Message)
                            {
                                attemptCount = 1;
                                prevException = curException;
                            }
                            else
                                attemptCount++;
                        }
                        TraceLog.Log(String.Format("Failure {0} : {1} \n{2}", attemptCount, prevException.Message, prevException.StackTrace));
                        System.Threading.Thread.Sleep(Wait * 1000);
                    }
                    TraceLog.Log(String.Format("Aborting : {0}\n{1}", prevException.Message, prevException.StackTrace));
                    throwCommandException(cmd, prevException);
                    throw prevException; //This line will never execute b/c a command exception will be thrown in the above function. 
                    //This is just a place holder because other wise "Not all code paths return a value."
                }
            }

        }

        public DataTable SPToDataTable(QueryInformation qInfo)
        {
            if (qInfo == null) throw new ArgumentException("Null query information", "qInfo");
            if (!qInfo.IsStoredProcedure) throw new CommandException("Attempting to execute a direct query using a stored procedure method.");
            try
            {
                return SPToDataTable(qInfo.QueryString, 0, qInfo.QueryParameters);
            }
            catch (Exception e)
            {
                qInfo.OnExceptionOccured(null);
                throw e;
            }
        }
        #endregion
        #endregion

        #region Database information
        public virtual bool TestConnection()
        {
            using (IDbConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    conn.Close();
                    return true;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    return false;
                }
            }
        }
        public abstract bool TableExists(string tableName);
        public abstract TableInformation GetTableInformation(string tableName);
        public abstract bool ColumnExists(string tableName, string columnName);
        public abstract ColumnInformation GetColumnInformation(string tableName, string columnName);
        public abstract SPInformation GetSPInformation(String schemaName, String pkgName, String procedureName);
        #endregion

        #region Transaction
        protected void OpenConnection(IDbConnection conn)
        {
            if (_currTransaction == null)
                conn.Open();
        }
        protected void CloseConnection(IDbConnection conn)
        {
            if (_currTransaction == null)
                conn.Close();
        }

        /// <summary>
        /// Will only affect NonQuery and SPNonQuery. Any may produce a result set will not honor the transaction.
        /// </summary>
        /// <returns></returns>
        public bool BeginTransaction()
        {
            if (_currTransaction != null)
                throw new TransactionException("A new transaction can not be started while an existing transaction has not be commited or rolled back.");
            _TraceLog.Log("Starting Transaction");
            IDbConnection conn = GetConnection();
            conn.Open();
            _currTransaction = conn.BeginTransaction();
            return true;

        }
        /// <summary>
        /// Commit the transaction as it is being ended sucessfully.
        /// </summary>
        /// <returns></returns>
        public bool CommitTransaction() 
        {
            if (_currTransaction == null)
                throw new TransactionException("No transaction to commit.");
            lock (_currTransaction)
            {
                _TraceLog.Log("Committing Transaction");
                _currTransaction.Commit();
                if (_currTransaction.Connection!=null && _currTransaction.Connection.State != ConnectionState.Closed)
                    _currTransaction.Connection.Close();
                _currTransaction.Dispose();
                _currTransaction = null;
                return true;
            }
        }
        /// <summary>
        /// Roll back the transaction.
        /// </summary>
        /// <returns></returns>
        public bool RollbackTransaction() {
            if (_currTransaction == null)
                throw new TransactionException("No transaction to roll back.");
            lock (_currTransaction)
            {
                _TraceLog.Log("Rolling back Transaction");
                _currTransaction.Rollback();
                if (_currTransaction.Connection!=null && _currTransaction.Connection.State != ConnectionState.Closed)
                    _currTransaction.Connection.Close();
                _currTransaction.Dispose();
                _currTransaction = null;
                return true;
            }
        }
        #endregion

        public void throwCommandException(IDbCommand cmd, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("sql {0}:\n{1}\nwith parameters:\n",cmd.CommandType== CommandType.Text?"command":"stored procedure", cmd.CommandText);
            foreach (IDbDataParameter parm in cmd.Parameters)
                sb.AppendFormat("{0} {1}={2}\n",parm.DbType.ToString(), parm.ParameterName, parm.Value.ToString());
            sb.AppendFormat("encountered an exception. See inner exception for details.");
            Exception cmdEx = new Exception(sb.ToString(),ex);
            throw cmdEx;
        }

        public abstract string ToParameterizedName(String ParameterName);

        
        
        ~DataAccessObjectBase()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (_currTransaction != null)
            {
                try
                {
                    _currTransaction.Rollback();
                    TraceLog.Log("Uncommitted transaction being rolled back on Dispose.");
                }
                catch (InvalidOperationException) { }
				finally {
                    if (_currTransaction.Connection != null) 
                        _currTransaction.Connection.Dispose();
                    _currTransaction.Dispose();
                    _currTransaction = null;
				}
            }
        }
    }
}
