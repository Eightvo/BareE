using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{

    public class DataAccessContainer
    {
        //public IDataAccessObject DBO { get; set; }
        //public DataAccessContainer(IDataAccessObject _dbo) { DBO = _dbo; }
    }
    public interface IAttributeCollection
    {
        DataAccessContainer DataAccess { get; set; }
        bool IsKey(String attributeName);
        bool IsPersistedAttribute(String attributeName);
        object this[String key] { get; set; }
        T DataAs<T>(String key);

        /*
        bool Load(IEnumerable<EntityAttributeValue> key);
        bool Load(params EntityAttributeValue[] key);
        bool Load(DataRow row);
        bool Load(DataRow row, bool Persisted);

        bool Commit(IDataAccessObject DAO);
        bool Commit();
        bool Delete();
         */
    }
}
