using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BareE.DataAcess.Config
{
    public class sqRlConfigSection : ConfigurationSection
    {
        public static sqRlConfigSection sqRlSettings = ConfigurationManager.GetSection("sqRl") as sqRlConfigSection;

        [ConfigurationProperty("Connections", IsDefaultCollection = true)]
        public ConnectionCollection Connections
        {
            get { return (ConnectionCollection)base["Connections"]; }
        }

    }
    public sealed class ConnectionCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConnectionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConnectionElement)element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "Connection"; }
        }

        public ConnectionElement this[int index]
        {
            get { return (ConnectionElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public ConnectionElement this[string connectionName]
        {
            get { return (ConnectionElement)BaseGet(connectionName); }
        }

        public bool ContainsKey(string key)
        {
            bool result = false;
            object[] keys = BaseGetAllKeys();
            foreach (object obj in keys)
            {
                if ((string)obj == key)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
    public class ConnectionElement : ConfigurationElement
    {

        Dictionary<String, String> _customData = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (_customData.ContainsKey(name)) throw new Exception(String.Format("A key by the name {0} already in configuration of connection {1}.", name, Name));
            _customData.Add(name, value);
            return true;
        }

        new public String this[String key]
        {
            get
            {
                if (!_customData.ContainsKey(key)) return string.Empty;
                return _customData[key];
            }
        }
    

        [ConfigurationProperty("Name",IsRequired=true)]
        public String Name { get { return (String)base["Name"]; } }

        [ConfigurationProperty("Type",IsRequired=false,DefaultValue="Oracle")]
        public String Type{get{return (String)base["Type"];}}

        [ConfigurationProperty("Server", IsRequired = false)]
        public String Server { get { return (String)base["Server"]; } }

        [ConfigurationProperty("Database",IsRequired=false)]
        public String Database { get { return (String)base["Database"]; } }

        [ConfigurationProperty("Attempts", IsRequired = false, DefaultValue = 1)]
        public int Attempts { get { return (int)base["Attempts"]; } }

        [ConfigurationProperty("Wait", IsRequired = false, DefaultValue = 0)]
        public int Wait { get { return (int)base["Wait"]; } }


        [ConfigurationProperty("User", IsRequired=true)]
        public String User { get { return (String)base["User"]; } }

        [ConfigurationProperty("Pass",IsRequired=false)]
        public String Pass { get { return (String)base["Pass"]; } }

        [ConfigurationProperty("EncryptedPass", IsRequired = false)]
        public String EncryptedPass { get { return (String)base["EncryptedPass"]; } }

        [ConfigurationProperty("FixCommands", IsRequired=false, DefaultValue=false)]
        public bool FixCommands { get { return (bool)base["FixCommands"]; } }

        [ConfigurationProperty("Timeout", IsRequired = false)]
        public String Timeout { get { return (String)base["Timeout"]; } }

        [ConfigurationProperty("CommandTimeout", IsRequired = false)]
        public String CommandTimeout { get { return (String)base["CommandTimeout"]; } }

        [ConfigurationProperty("TraceLog", IsRequired = false, DefaultValue = "")]
        public String TraceLog { get { return (String)base["TraceLog"]; } }

        [ConfigurationProperty("LogQuery",IsRequired=false,DefaultValue=true)]
        public bool LogQuery { get { return (bool)base["LogQuery"]; } }

        [ConfigurationProperty("LogParams",IsRequired=false,DefaultValue=true)]
        public bool LogParams { get { return (bool)base["LogParams"]; } }

        [ConfigurationProperty("LogResults", IsRequired = false, DefaultValue = false)]
        public bool LogResults { get { return (bool)base["LogResults"]; } }


    }

}
