using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Linq.Expressions;

namespace BareE.DataStructures
{
    /// <summary>
    /// A concrete representation of a set of entities and their corrosponding components.
    /// </summary>
    public class EntityComponentContext
    {
        /// <summary>
        /// All instantiated Entities by name or index within a context.
        /// </summary>
        public AliasMap<Entity> Entities;

        /// <summary>
        /// All instantiated Components within a context.
        /// </summary>
        public ComponentCache Components;

        public EntityComponentContext() : this(new AliasMap<Entity>(), new ComponentCache())
        {
        }

        public EntityComponentContext(AliasMap<Entity> _entities, ComponentCache _components)
        {
            Entities = _entities;
            Components = _components;
        }

        /// <summary>
        /// Instantiate a new Entity and associate it with the set of provided components.
        /// </summary>
        /// <param name="componentList"></param>
        /// <returns></returns>
        public Entity SpawnEntity(params object[] componentList)
        {
            Entity spawn = new Entity();
            spawn.Id = Entities.Index(spawn);
            foreach (var component in componentList)
            {
                Components.SetComponent(spawn, component);
            }
            return spawn;
        }

        /// <summary>
        /// Instantiate a new entity by name as associate it with the provided components.
        /// </summary>
        /// <param name="Alias"></param>
        /// <param name="componentList"></param>
        /// <returns></returns>
        public Entity SpawnEntity(String Alias, params object[] componentList)
        {
            Entity spawn = new Entity();
            spawn.Id = Entities.Alias(Alias, spawn);
            foreach (var component in componentList)
            {
                Components.SetComponent(spawn, component);
            }
            return spawn;
        }

        /// <summary>
        /// Instantiate a new entity with the given parent and associate it with the given components.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="componetLst"></param>
        /// <returns></returns>
        public Entity SpawnChildEntity(Entity parent, params object[] componetLst)
        {
            var ent = SpawnEntity(componetLst);
            ent.Parent = parent.Id;
            return ent;
        }

        /// <summary>
        /// Instantiate a new entity by name with the given parent and associate it with the given components.
        /// </summary>
        /// <param name="Alias"></param>
        /// <param name="parent"></param>
        /// <param name="componetLst"></param>
        /// <returns></returns>
        public Entity SpawnChildEntity(String Alias, Entity parent, params object[] componetLst)
        {
            var ent = SpawnEntity(Alias, componetLst);
            ent.Parent = parent.Id;
            return ent;
        }

        public Entity SpawnFromAsset(String Alias, String Asset, params object[] componentOverrides)
        {
            
            var model = AttributeCollectionDeserializer.FromAsset(Asset);

            List<object> componentList = AttributeCollectionToComponentSet(model);


            Entity spawn = new Entity();

            spawn.Id = Entities.Alias(Alias, spawn);
            //spawn.Id = Entities.Index(spawn);
            foreach (var component in componentList)
            {
                Components.SetComponent(spawn, component);
            }
            foreach(var component in componentOverrides)
            {
                Components.SetComponent(spawn, component);
            }
            return spawn;
        }
        public Entity SpawnFromAsset(String Asset, params object[] componentOverrides)
        {
            var model = AttributeCollectionDeserializer.FromAsset(Asset);
            List<object> componentList = AttributeCollectionToComponentSet(model);
            Entity spawn = new Entity();
            var Alias = model.DataAs<String>("Alias");
            if (!String.IsNullOrEmpty(Alias))
                spawn.Id = Entities.Alias(Alias, spawn);
            else 
                spawn.Id = Entities.Index(spawn);

            foreach (var component in componentList)
            {
                Components.SetComponent(spawn, component);
            }
            foreach (var component in componentOverrides)
            {
                Components.SetComponent(spawn, component);
            }
            return spawn;
        }

        private List<object> AttributeCollectionToComponentSet(AttributeCollection collection)
        {
            List<object> ret = new List<object>();
            foreach(var v in collection.Attributes)
            {
                if (ComponentCache.ComponentAliasMap.AliasExists(v.AttributeName))
                {
                    ret.Add(AttributeCollectionAs(ComponentCache.ComponentAliasMap[v.AttributeName].OriginatingType, (AttributeCollection)v.Value));
                }
            }
            return ret;
        }


        private object AttributeCollectionAs<T>(AttributeCollection src)
        {
            return AttributeCollectionAs(typeof(T), src);
                
        }
        private object AttributeCollectionAs(Type t, AttributeCollection src)
        {
            var ret = Activator.CreateInstance(t);
            foreach(var v in t.GetProperties())
            {
                if (v.PropertyType.IsPrimitive)
                {
                    v.SetValue(ret, src[v.Name]);
                    continue;
                } 
                if (v.PropertyType == typeof(String))
                {
                    v.SetValue(ret, src[v.Name]);
                    continue;
                }
                if (v.PropertyType.IsArray)
                {
                    var arryObj = AttributeAsArrayOf(v.PropertyType.GetElementType(), (object[])src[v.Name]);
                    
                    v.SetValue(ret, arryObj);
                    continue;
                }

                v.SetValue(ret, AttributeAsObject(v.PropertyType, (object)src[v.Name]));
            }
            return ret;
        }
        private object AttributeAsObject(Type t, object src)
        {
            if (t.IsPrimitive)
            {
                return src;
            }
            if (t == typeof(String))
            {
                return src.ToString();
            }
            if (src.GetType()==typeof(String))
            {
                if (t==typeof(Vector3) || t==typeof(Vector4))
                {
                    return Helper.DecodeColor(src.ToString());
                }
            }
            return AttributeCollectionAs(t, (AttributeCollection)src);
        }
        private object AttributeAsArrayOf(Type t, object[] src)
        {
            var ret = Array.CreateInstance(t, src.Length);

            //foreach(var elementSrc in src)
            for(int i =0;i<src.Length;i++)
            {
                if (t.IsPrimitive)
                {
                    ret.SetValue(src[i], i);
                    continue;
                }
                if (t == typeof(String))
                {
                    ret.SetValue(src[i].ToString(), i);
                    continue;
                }
                var val = AttributeCollectionAs(t, (AttributeCollection)src[i]);
                ret.SetValue(val, i);
            }
            //Activator.CreateInstance(t);
            return ret;
            //return ret.ToArray();
        }

    }
}