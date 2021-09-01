using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BareE.Log;

namespace BareE.DataAcess
{
    public class DataReaderWrapper:IDisposable
    {
        IDbConnection Connection;
        IDbCommand Command;
        public IDataReader Reader { get; set; }
        ILog ResultsLog { get; set; }
        bool LogResults { get; set; }
        public int ReadCount { get; protected set; }
        public DataReaderWrapper(IDbConnection conn, IDbCommand cmd, ILog resultsLog)
        {
            Connection = conn;
            Command = cmd;
            Connection.Open();
            Reader = cmd.ExecuteReader();
            ResultsLog = resultsLog ?? new NullLogger();
            LogResults = ResultsLog.GetType() != typeof(NullLogger);
            ReadCount = 0;
        }

        public object this[int i]
        {
            get
            {
                return Reader[i];
            }
        }
        public object this[string s]
        {
            get
            {
                return Reader[s];
            }
        }

        public bool Read()
        {
            bool Success = Reader.Read();

            if (Success)
            {
                ReadCount++;
                if (LogResults)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < Reader.FieldCount; i++)
                        sb.AppendFormat("{0}{1}", i == 0 ? "" : ", ", Reader[i].ToString());
                    ResultsLog.Log(sb.ToString());
                    ResultsLog.Log("Result {0}. More results may exist.", ReadCount);
                }
            }
            return Success;
        }

        public void Close()
        {
            Reader.Close();
        }

        public void Dispose()
        {
            Reader.Close();
            Connection.Close();
            Reader.Dispose();
            Command.Dispose();
            Connection.Dispose();
        }
    }
}
