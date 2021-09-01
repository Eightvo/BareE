using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BareE.DataAcess
{
    public delegate void QueryInformationExceptionHandler(QueryInformation sender, EventArgs e);

    public class QueryInformation
    {
        public object Tag { get; set; }
        public event QueryInformationExceptionHandler ExceptionOccured;
        public virtual void OnExceptionOccured(EventArgs e)
        {
            if (ExceptionOccured != null)
                ExceptionOccured(this, e);
        }

        public bool IsStoredProcedure { get { return _isStoredProcedure; } }
        public String QueryString
        {
            get
            {
                if (IsStoredProcedure)
                {
                    return String.Format("{0}{1}{2}", !String.IsNullOrEmpty(_schema) ? String.Concat(_schema, ".") : "", !String.IsNullOrEmpty(_package) ? String.Concat(_package, ".") : "", _procName);
                }
                else
                {
                    return _sql;
                }
            }
        }
        public IEnumerable<ParameterInformation> QueryParameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                _parameters = value;
            }
        }
        bool _isStoredProcedure;
        String _sql;
        String _schema;
        String _package;
        String _procName;

        IEnumerable<ParameterInformation> _parameters;

        public QueryInformation():this(String.Empty,String.Empty,String.Empty,false,new List<ParameterInformation>()){}
        public QueryInformation(String sql):this(String.Empty, String.Empty,sql,false,new List<ParameterInformation>()){}
        public QueryInformation(String sql, bool isSproc) : this(String.Empty,String.Empty,sql, isSproc, new List<ParameterInformation>()) { }
        public QueryInformation(String sql, bool isSproc, params ParameterInformation[] parameters) : this(String.Empty, String.Empty, sql, isSproc, parameters.ToList()) { }
        public QueryInformation(String sql, bool isSproc, IEnumerable<ParameterInformation> parameters) : this(String.Empty, String.Empty, sql, isSproc,parameters) { }

        public QueryInformation(String package, String sprocName, params ParameterInformation[] parameters) : this(String.Empty, package, sprocName,true, parameters.ToList()) { }
        public QueryInformation(String schema, String package, String sprocName, params ParameterInformation[] parameters) : this(schema, package, sprocName, true, parameters.ToList()) { }
        public QueryInformation(String schema, String package, String sprocName, IEnumerable<ParameterInformation> parameters) : this(schema, package, sprocName, true,parameters) { }

        QueryInformation(String schema, String package, String sprocName,bool isSproc, IEnumerable<ParameterInformation> parameters)
        {
            _isStoredProcedure = isSproc;
            _parameters = parameters??new List<ParameterInformation>();
            if (isSproc)
            {
                _schema = schema;
                _package = package;
                _procName = sprocName;
            }
            else
            {
                _sql = sprocName;
            }
        }

    }
}
