using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BareE.DataAcess.MSQL
{
    /*
    class DbLog:BareE.Log.LogBase
    {
        IDataAccessObject dao;
        String Table;
        public DbLog(BareE.Log.Config.LogElement config):base(config)
        {
            if (String.IsNullOrEmpty(config["Connection"]))
                throw new Exception("Database logs must be configured with a Connection");

            if (String.IsNullOrEmpty(config["Table"]))
                throw new Exception("Database logs must be configured with a Table");

            //Allows configuration to skip verification b/c verification may take a couple sec to do and if you are 
            //sure that the table is set up correctly on the connection there is no need.
            bool verify = String.IsNullOrEmpty(config["Verify"]) || (String.Compare(config["Verify"],"true",true)==0);

            //Allows configuration to generate the table if it doesn't exist, requires create/drop permission on connection.
            bool generate = !String.IsNullOrEmpty(config["Generate"]) && (String.Compare(config["Generate"],"true",true)==0);

            dao = ConnectionManager.Get(config["Connection"]);
            Table=config["Table"];

            TableInformation ti=null;
            if (verify || generate)
            {
                if (!dao.TableExists(Table))
                {
                    if (!generate)
                        throw new Exception(String.Format("Table {0} not found using connection {1}",Table,config["Connection"]));
                    else ti=buildTable();
                }
                if (ti==null) ti=dao.GetTableInformation(Table);
                if (   ti.Columns.Any(x=>x.ColumnName=="MessageDate")==false
                    || ti.Columns.Any(x=>x.ColumnName=="MessageLevel")==false
                    || ti.Columns.Any(x=>x.ColumnName=="Message")==false
                    )
                    throw new Exception("Provided table is incorrectly defined.");
            }
        }

        protected TableInformation buildTable()
        {
            if (dao.GetType()==typeof(SqlDataAccessObj))
                return buildTableSql();
            //if (dao.GetType()==typeof(OracleSqlDataAccessObj))
            //    return buildTableOracle();
            return null;
        }
        protected TableInformation buildTableSql()
        {
            dao.NonQuery("Create Table @tbl (MessageDate datetime,MessageLevel varchar(50),Message varchar(MAX));", new List<ParameterInformation>() { new ParameterInformation("tbl", Table) });
            TableInformation ti = new TableInformation(Table);
            ti.Columns.Add(new ColumnInformation("MessageDate", false, true));
            ti.Columns.Add(new ColumnInformation("MessageLevel", false, true));
            ti.Columns.Add(new ColumnInformation("Message", false, true));
            return ti;
        }
        protected TableInformation buildTableOracle()
        {
            dao.NonQuery("Create Table :tbl (MessageDate date,MessageLevel varchar2(50),Message varchar2(1000));", new List<ParameterInformation>() { new ParameterInformation("tbl", Table) });
            TableInformation ti = new TableInformation(Table);
            ti.Columns.Add(new ColumnInformation("MessageDate", false, true));
            ti.Columns.Add(new ColumnInformation("MessageLevel", false, true));
            ti.Columns.Add(new ColumnInformation("Message", false, true));
            return ti;
        }
        public override bool Log(string message, BareE.Log.LogMessageLevel level, int verbosity)
        {
            if (dao.GetType() == typeof(SqlDataAccessObj))
                 return LogSql(message,level,verbosity);

            //if (dao.GetType()==typeof(OracleSqlDataAccessObj))
            //    return LogOracle(message,level,verbosity);

            throw new NotImplementedException();
        }
        protected bool LogSql(string message, BareE.Log.LogMessageLevel level, int verbosity)
        {
            throw new NotImplementedException();
        }
        protected bool LogOracle(String message, BareE.Log.LogMessageLevel level, int verbosity)
        {
            throw new NotImplementedException();
        }
    }
    */
}
