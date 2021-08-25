using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BareE.DataStructures
{
    /// <summary>
    /// Maintains Entity->Component Relationships.
    /// </summary>
    public class ComponentCache
    {
        #region ComponentSystemMaint

        private static Dictionary<Type, ComponentAttribute> _componetTypeData;
        private static AliasMap<ComponentAttribute> _componentAliasMap;

        /// <summary>
        /// Obtain Information Regarding Components by Name.
        /// </summary>
        public static AliasMap<ComponentAttribute> ComponentAliasMap
        {
            get
            {
                if (_componentAliasMap == null)
                    LoadComponentData();
                return _componentAliasMap;
            }
        }

        /// <summary>
        /// Obtain Information Regardign Components by Type.
        /// </summary>
        public static Dictionary<Type, ComponentAttribute> ComponentTypeData
        {
            get
            {
                if (_componetTypeData == null)
                    LoadComponentData();
                return _componetTypeData;
            }
        }

        internal static void LoadComponentData()
        {
            _componetTypeData = new Dictionary<Type, ComponentAttribute>();
            _componentAliasMap = new AliasMap<ComponentAttribute>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        var attr = type.GetCustomAttribute<ComponentAttribute>(false);
                        if (attr == null)
                            continue;
                        attr.OriginatingType = type;
                        Log.EmitTrace($"Registering Component: {(type.Name)}=>{attr}");
                        _componetTypeData.Add(type, attr);
                        _componentAliasMap.Alias(attr.Name, attr);
                    }
                }
                catch (Exception e)
                {
                    Log.EmitError(e);
                }
            }
        }

        #endregion ComponentSystemMaint

        private object SyncRoot = new object();

        private Dictionary<int, Dictionary<int, object>> _components = new Dictionary<int, Dictionary<int, object>>();

        /// <summary>
        /// Iterate all Components By Name.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<String> ListComponents()
        {
            return ComponentAliasMap.Keys();
        }

        /// <summary>
        /// Get number of existing Components By name.
        /// </summary>
        /// <param name="componentName"></param>
        /// <returns></returns>
        public int CountComponents(String componentName)
        {
            int i = ComponentAliasMap[componentName].CTypeID;
            if (!_components.ContainsKey(i))
                return 0;
            return (_components[i].Count);
        }

        /// <summary>
        /// Remove a component from an Entity.
        /// </summary>
        /// <param name="componentType"></param>
        /// <param name="e"></param>
        public void RemoveComponent(String componentType, Entity e)
        {
            int i = ComponentAliasMap[componentType].CTypeID;
            RemoveComponent(i, e);
        }

        private void RemoveComponent(int componentID, Entity ent)
        {
            if (!_components.ContainsKey(componentID)) return;
            if (_components[componentID].ContainsKey(ent.Ideal))
                _components[componentID].Add(-ent.Id, null);
            if (!_components[componentID].ContainsKey(ent.Id)) return;
            _components[componentID].Remove(ent.Id);
        }

        /// <summary>
        /// Get the Component associated with an entity.
        /// A default instance of the compeont type will be returned if no Component of that type exists for the entity.
        /// Does not cause accumulation through parent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <param name="entityPool"></param>
        /// <returns></returns>
        public T GetComponent<T>(Entity e)
        {
            var tDat = ComponentTypeData[typeof(T)];
            int i = tDat.CTypeID;
            if (!_components.ContainsKey(i))
                return default(T);

            object ret = GetComponent(e, tDat.CTypeID);
            if (ret != null)
                return (T)ret;
            return default(T);
        }

        /// <summary>
        /// Get a component for an entity by name.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sType"></param>
        /// <returns></returns>
        public object GetComponent(Entity e, String sType)
        {
            return GetComponent(e, ComponentAliasMap[sType].CTypeID);
        }

        /// <summary>
        /// Get a component for an Entity by ComponentID.
        /// A componentID is static only though the lifetime of the application.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="cAttribId"></param>
        /// <returns></returns>
        public object GetComponent(Entity e, int cAttribId)
        {
            /* Done in calling func
            if (!_components.ContainsKey(typeID))
                return null;
            */
            if (e == null) return null;
            //If it is masked, specifically return default
            //This would be used to remove a component from an entity with an ideal with a component.
            if (_components[cAttribId].ContainsKey(-e.Id))
                return null;

            if (_components[cAttribId].ContainsKey(e.Id))
                return _components[cAttribId][e.Id];
            if (_components[cAttribId].ContainsKey(e.Ideal))
                return _components[cAttribId][e.Ideal];
            return null;
        }

        /// <summary>
        /// Associate a Component with an Entity overwriting an existing component of that type currently associated with the entity.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="component"></param>
        public void SetComponent(Entity e, object component)
        {
            SetComponent(e, component, ComponentTypeData[component.GetType()]);
        }

        private void SetComponent(Entity e, object component, ComponentAttribute cAttrib)
        {
            if (!_components.ContainsKey(cAttrib.CTypeID))
                _components.Add(cAttrib.CTypeID, new Dictionary<int, object>());

            if (_components[cAttrib.CTypeID].ContainsKey(-e.Id))
                _components[cAttrib.CTypeID].Remove(-e.Id);
            if (!_components[cAttrib.CTypeID].ContainsKey(e.Id))
                _components[cAttrib.CTypeID].Add(e.Id, component);
            else
                _components[cAttrib.CTypeID][e.Id] = component;
        }

        /// <summary>
        /// Hide a component on an entity.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="componentTypeId"></param>
        /// <param name="mask"></param>
        private void MaskComponent(Entity e, int componentTypeId, bool mask = true)
        {
            if (!_components.ContainsKey(componentTypeId))
                return;
            if (mask)
            {
                if (!_components[componentTypeId].ContainsKey(-e.Id))
                    _components[componentTypeId].Add(-e.Id, mask);
            }
            else
            {
                if (_components[componentTypeId].ContainsKey(-e.Id))
                    _components[componentTypeId].Remove(-e.Id);
            }
        }

        internal T[] GetAll<T>()
        {
            var tDat = ComponentTypeData[typeof(T)];
            int i = tDat.CTypeID;
            if (!_components.ContainsKey(i))
                return Array.Empty<T>();
            var subDict = _components[i];
            T[] ret = (new List<T>(subDict.Values.Cast<T>())).ToArray();
            return ret;
            //_components[i].Values.Cast<T>().CopyTo(ret, 0);
        }

        private IEnumerable<T> GetComponents<T>()
        {
            int i = ComponentTypeData[typeof(T)].CTypeID;
            if (!_components.ContainsKey(i))
                yield break;
            foreach (var v in _components[i].Values)
                yield return (T)v;
        }

        /// <summary>
        /// Returns true if the component or the ideal of a component contains this type of component.
        /// Returns false if a component has been masked.
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="ent"></param>
        /// <param name="entityPool"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public bool HasComponent(String componentName, Entity ent)
        {
            int i = ComponentAliasMap[componentName].CTypeID;
            return HasComponent(i, ent);
        }

        /// <summary>
        /// Returns true if the component or the ideal of a component contains this type of component.
        /// Returns false if a component has been masked.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <returns></returns>
        public bool HasComponent<T>(Entity ent)
        {
            int i = ComponentTypeData[typeof(T)].CTypeID;
            return HasComponent(i, ent);
        }

        /// <summary>
        /// Returns true if the component or the ideal of a component contains this type of component.
        /// Returns false if a component has been masked.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        private bool HasComponent(int i, Entity ent)
        {
            if (!_components.ContainsKey(i)) return false;
            return
                _components[i].ContainsKey(ent.Id)
            //|| _components.ContainsKey(-ent.Id)
            || _components.ContainsKey(ent.Ideal);
        }

        /// <summary>
        /// Enumerates a collection of KeyValuePairs.
        /// The Key is the EntityId
        /// The Value is the Component Value for that Entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<int, T>> GetEntitiesByComponentType<T>()
        {
            int i = ComponentTypeData[typeof(T)].CTypeID;
            if (!_components.ContainsKey(i))
                yield break;

            foreach (var v in _components[i])
                yield return new KeyValuePair<int, T>(v.Key, (T)v.Value);
        }
        public IEnumerable<KeyValuePair<int, object>> GetEntitiesByComponentType(Type t)
        {
            int i = ComponentTypeData[t].CTypeID;
            if (!_components.ContainsKey(i))
                yield break;

            foreach (var v in _components[i])
                yield return new KeyValuePair<int, object>(v.Key, v.Value);
        }
        /// <summary>
        /// Removes all components from the cache by EntityId.
        /// </summary>
        /// <param name="iD"></param>
        public void RemoveByEntityID(int iD)
        {
            foreach (var v in _components)
            {
                v.Value.Remove(iD);
                v.Value.Remove(-iD);
            }
        }

        /// <summary>
        /// Enumerates all Components Associated with an Entity.
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public IEnumerable<object> GetComponentsByEntity(Entity ent)
        {
            foreach (var v in _components)
            {
                if (!HasComponent(v.Key, ent))
                    continue;
                yield return GetComponent(ent, v.Key);
            }
        }

        public void Dispose()
        {
            foreach (var v in _components)
            {
                v.Value.Clear();
            }
            _components.Clear();
        }
    }
}