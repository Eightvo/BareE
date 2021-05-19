using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BareE.DataStructures
{

    public class ComponentCache
    {
        #region ComponentSystemMaint
        static Dictionary<Type, ComponentAttribute> _componetTypeData;
        static AliasMap<ComponentAttribute> _componentAliasMap;
        public static AliasMap<ComponentAttribute> ComponentAliasMap
        {
            get
            {
                if (_componentAliasMap == null)
                    LoadComponentData();
                return _componentAliasMap;
            }
        }
        public static Dictionary<Type, ComponentAttribute> ComponentTypeData
        {
            get
            {
                if (_componetTypeData == null)
                    LoadComponentData();
                return _componetTypeData;
            }
        }
        public static void LoadComponentData()
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
        #endregion

        public object SyncRoot = new object();

        Dictionary<int, Dictionary<int, object>> _components = new Dictionary<int, Dictionary<int, object>>();

        public IEnumerable<String> ListComponents()
        {
            return ComponentAliasMap.Keys();
        }
        public int CountComponents(String componentName)
        {
            int i = ComponentAliasMap[componentName].CTypeID;
            if (!_components.ContainsKey(i))
                return 0;
            return (_components[i].Count);
        }

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
        /// Does not cause accumulation.
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

            //Getting a Component Cumulatively should be a different Function Call. 
            object ret = GetComponent(e, tDat.CTypeID);
            if (ret != null)
                return (T)ret;
            return default(T);
        }

        public object GetComponent(Entity e, String sType)
        {
            return GetComponent(e, ComponentAliasMap[sType].CTypeID);
        }
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

        public void SetComponent(Entity e, object component)
        {
            SetComponent(e, component, ComponentTypeData[component.GetType()]);
        }

        void SetComponent(Entity e, object component, ComponentAttribute cAttrib)
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

        void MaskComponent(Entity e, int componentTypeId, bool mask = true)
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

        IEnumerable<T> GetComponents<T>()
        {
            int i = ComponentTypeData[typeof(T)].CTypeID;
            if (!_components.ContainsKey(i))
                yield break;
            foreach (var v in _components[i].Values)
                yield return (T)v;
        }

        /// <summary>
        /// Checks component Directly.
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
        public bool HasComponent<T>(Entity ent)
        {
            int i = ComponentTypeData[typeof(T)].CTypeID;
            return HasComponent(i, ent);
        }
        private bool HasComponent(int i, Entity ent)
        {
            if (!_components.ContainsKey(i)) return false;
            return
                _components[i].ContainsKey(ent.Id)
            //|| _components.ContainsKey(-ent.Id)
            || _components.ContainsKey(ent.Ideal);
        }

        public IEnumerable<KeyValuePair<int, T>> GetEntitiesByComponentType<T>()
        {
            int i = ComponentTypeData[typeof(T)].CTypeID;
            if (!_components.ContainsKey(i))
                yield break;

            foreach (var v in _components[i])
                yield return new KeyValuePair<int, T>(v.Key, (T)v.Value);

        }

        public void RemoveByEntityID(int iD)
        {
            foreach (var v in _components)
            {
                v.Value.Remove(iD);
                v.Value.Remove(-iD);
            }
        }
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
