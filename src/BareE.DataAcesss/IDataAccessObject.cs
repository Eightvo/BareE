using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BareE.Log;
using System.Data;
using System.Diagnostics;

namespace BareE.DataAcess
{

    /*
     <DAO Name=""
        Server
        User
        Pass
        Database
     
      />
        <Option>
        </Options>
        <TraceLog>
       
     </DAO>
     */
    public interface IDataAccessObject:IDisposable
    {
#region Properties

        /// <summary>
        /// Number of times to retry a failed command before giving up.
        /// </summary>
        int Attempts { get; set; }
        /// <summary>
        /// Number of Seconds to wait between failed attempts
        /// </summary>
        int Wait { get; set; }
        
        /// <summary>
        /// Name of Server
        /// </summary>
        String Server{get;set;}

        /// <summary>
        /// User Name
        /// </summary>
        String User{get;set;}

        /// <summary>
        /// Password to database
        /// </summary>
        String Pass{get;set;}
        String Database{get;set;}

        /// <summary>
        /// Optional information, that applies to specific databases.
        /// I.E. Oracle can have a TNS Option which will replace the Server, Port with the TNS Name.
        /// </summary>
        Dictionary<string,string> Options{get;set;}

        /// <summary>
        /// Log to use to write Trace data to.
        /// </summary>
        ILog TraceLog{get;set;}

        /// <summary>
        /// Connection String used to connect to database, read-only.
        /// </summary>
        String ConnectionString{get;}


        bool LogQuery { get; set;}
        bool LogParms { get; set; }
        bool LogResults { get; set; }

#endregion
        #region Direct Query

        #region ProcessDataReader
        int ProcessDataReader(String cmdText, Func<IDataReader,Boolean> callBack);
        int ProcessDataReader(String cmdText, Func<IDataReader, Boolean> callBack, IEnumerable<ParameterInformation> parmInfo);
        int ProcessDataReader(String cmdText, Func<IDataReader, Boolean> callBack, params ParameterInformation[] parmInfo);
        int ProcessDataReader(String cmdText, int cmdTimeout, Func<IDataReader, Boolean> callBack, IEnumerable<ParameterInformation> parmInfo);
        int ProcessDataReader(String cmdText, int cmdTimeout, Func<IDataReader, Boolean> callBack, params ParameterInformation[] parmInfo);
        int ProcessDataReader(QueryInformation qInfo,Func<IDataReader,Boolean> callBack);
        #endregion

        #region DataReaderWrapper
        DataReaderWrapper ToDataReaderWrapper(String cmdText);
        DataReaderWrapper ToDataReaderWrapper(String cmdText, IEnumerable<ParameterInformation> parmInfo);
        DataReaderWrapper ToDataReaderWrapper(String cmdText, params ParameterInformation[] parmInfo);
        DataReaderWrapper ToDataReaderWrapper(String cmdText, int cmdTimeout, IEnumerable<ParameterInformation> parmInfo);
        DataReaderWrapper ToDataReaderWrapper(String cmdText, int cmdTimeout, params ParameterInformation[] parmInfo);
        DataReaderWrapper ToDataReaderWrapper(QueryInformation qInfo);
        #endregion

        #region ToDataTable
        DataTable ToDataTable(String cmdText);
        DataTable ToDataTable(String cmdText, IEnumerable<ParameterInformation> parmInfo);
        DataTable ToDataTable(String cmdText, int cmdTimeout, IEnumerable<ParameterInformation> parmInfo);
        DataTable ToDataTable(String cmdText, params ParameterInformation[] parmInfo);
        DataTable ToDataTable(String cmdText, int cmdTimeout, params ParameterInformation[] parmInfo);
        DataTable ToDataTable(QueryInformation qInfo);
        #endregion

        #region ToScalar
        T ToScalar<T>(String cmdText);
        T ToScalar<T>(String cmdText, IEnumerable<ParameterInformation> parmInfo);
        T ToScalar<T>(String cmdText, int cmdTimeout, IEnumerable<ParameterInformation> parmInfo);
        T ToScalar<T>(String cmdText, params ParameterInformation[] parmInfo);
        T ToScalar<T>(String cmdText, int cmdTimeout, params ParameterInformation[] parmInfo);
        T ToScalar<T>(QueryInformation qInfo);

        #endregion

        #region NonQuery
        int NonQuery(String cmdText);
        int NonQuery(String cmdText, IEnumerable<ParameterInformation> parmInfo);
        int NonQuery(String cmdText, int cmdTimeout, IEnumerable<ParameterInformation> parmInfo);
        int NonQuery(String cmdText, params ParameterInformation[] parmInfo);
        int NonQuery(String cmdText, int cmdTimeout, params ParameterInformation[] parmInfo);
        int NonQuery(QueryInformation qInfo);
        #endregion

        #endregion

        #region Stored Procedures
        #region SPNonQuery
        int SPNonQuery(String storedProcedureName, params ParameterInformation[] parmInfo);
        int SPNonQuery(String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        int SPNonQuery(String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        int SPNonQuery(String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo);

        int SPNonQuery(String package, String storedProcedureName, params ParameterInformation[] parmInfo);
        int SPNonQuery(String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        int SPNonQuery(String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        int SPNonQuery(String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo);

        int SPNonQuery(String schema, String package, String storedProcedureName, params ParameterInformation[] parmInfo);
        int SPNonQuery(String schema, String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        int SPNonQuery(String schema, String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        int SPNonQuery(String schema, String package, String procName, int timeOut, IEnumerable<ParameterInformation> parameters);

        int SPNonQuery(QueryInformation qInfo);
        #endregion

        #region SPToScalar
        T SPToScalar<T>(String storedProcedureName, params ParameterInformation[] parmInfo);
        T SPToScalar<T>(String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        T SPToScalar<T>(String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        T SPToScalar<T>(String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo);

        T SPToScalar<T>(String package, String storedProcedureName, params ParameterInformation[] parmInfo);
        T SPToScalar<T>(String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        T SPToScalar<T>(String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        T SPToScalar<T>(String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo);

        T SPToScalar<T>(String schema, String package, String storedProcedureName, params ParameterInformation[] parmInfo);
        T SPToScalar<T>(String schema, String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        T SPToScalar<T>(String schema, String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        T SPToScalar<T>(String schema, String package, String procName, int timeOut, IEnumerable<ParameterInformation> parameters);

        T SPToScalar<T>(QueryInformation qInfo);
        #endregion

        #region SPTpDataRpw
        DataRow SPToDataRow(String storedProcedureName, params ParameterInformation[] parmInfo);
        DataRow SPToDataRow(String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        DataRow SPToDataRow(String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        DataRow SPToDataRow(String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo);

        DataRow SPToDataRow(String package, String storedProcedureName, params ParameterInformation[] parmInfo);
        DataRow SPToDataRow(String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        DataRow SPToDataRow(String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        DataRow SPToDataRow(String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo);

        DataRow SPToDataRow(String schema, String package, String storedProcedureName, params ParameterInformation[] parmInfo);
        DataRow SPToDataRow(String schema, String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        DataRow SPToDataRow(String schema, String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        DataRow SPToDataRow(String schema, String package, String procName, int timeOut, IEnumerable<ParameterInformation> parameters);

        DataRow SPToDataRow(QueryInformation qInfo);
        #endregion

        #region SPToDataTable
        DataTable SPToDataTable(String storedProcedureName, params ParameterInformation[] parmInfo);
        DataTable SPToDataTable(String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        DataTable SPToDataTable(String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        DataTable SPToDataTable(String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo);

        DataTable SPToDataTable(String package, String storedProcedureName, params ParameterInformation[] parmInfo);
        DataTable SPToDataTable(String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        DataTable SPToDataTable(String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        DataTable SPToDataTable(String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo);

        DataTable SPToDataTable(String schema, String package, String storedProcedureName, params ParameterInformation[] parmInfo);
        DataTable SPToDataTable(String schema, String package, String storedProcedureName, IEnumerable<ParameterInformation> parmInfo);
        DataTable SPToDataTable(String schema, String package, String storedProcedureName, int timeOut, params ParameterInformation[] parmInfo);
        DataTable SPToDataTable(String schema, String package, String storedProcedureName, int timeOut, IEnumerable<ParameterInformation> parmInfo);

        DataTable SPToDataTable(QueryInformation qInfo);
        #endregion
        #endregion

        string ToParameterizedName(String ParameterName);

        bool LoggingEnabled { get; set; }

        bool BeginTransaction();
        bool CommitTransaction();
        bool RollbackTransaction();

        #region GetSchemea Information

        bool TestConnection();

        bool TableExists(String tableName);

       /*
        * Should there be another version that alows fresh database look up? In case it got changed since last lookup? 
        * Seems unlikly so I will not do it until required.
        */
       TableInformation GetTableInformation(String tableName);

       /* 
        * Individual columns will not be cached, since it should be cached with the table, so.. if a column 
        */
       bool ColumnExists(String tableName, String columnName);
       ColumnInformation GetColumnInformation(String tableName, String columnName);

       /*
        * 1) Until required I will not create a veersion that will get a fresh lookup for changes during execution.
        * 2) It might be nice to have a version that assumes current schema for sp's to work more similiar then everything else so far.
        */
       SPInformation GetSPInformation(String schemaName, String packageName, String procedureName);
        #endregion
    }
}
