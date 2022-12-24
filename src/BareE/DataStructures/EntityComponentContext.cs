using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using BareE.Components;
using SixLabors.ImageSharp;
using FFmpeg.AutoGen;
using System.Collections;
using Veldrid.MetalBindings;

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

        public void Merge(EntityComponentContext mergeWith)
        {
            HashSet<int> merged = new HashSet<int>();
            Dictionary<int, int> entIdRemap = new Dictionary<int, int>();
            foreach(var key in mergeWith.Entities.Keys())
            {
                Entity ent = mergeWith.Entities[key];
                merged.Add(ent.Id);
                var components = mergeWith.Components.GetComponentsByEntity(ent).ToArray();
                var newEnt = SpawnEntity(key, components);
                newEnt.Parent = ent.Parent;
                newEnt.Ideal = ent.Ideal;
                entIdRemap.Add(ent.Id,newEnt.Id);
            }
            foreach (Entity ent in mergeWith.Entities.Values())
            {
                if (ent == null) continue;
                if (!merged.Add(ent.Id))
                    continue;
                var components = mergeWith.Components.GetComponentsByEntity(ent).ToArray();
                var newEnt = SpawnEntity(components);
                newEnt.Parent = ent.Parent;
                newEnt.Id = ent.Id;
                entIdRemap.Add(ent.Id, newEnt.Id);
            }
            foreach(var v in entIdRemap)
            {
                var ent = Entities[v.Value];
                if (ent.Parent!=0)
                    ent.Parent = entIdRemap[ent.Parent];
                if (ent.Ideal!=0)
                    ent.Ideal= entIdRemap[ent.Ideal];
            }

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

        [Obsolete("Use State.ImportAsset to  ensure refernce messages are emitted.")]
        public Entity SpawnFromAsset(String Alias, String Asset, params object[] componentOverrides)
        {
            
            var model = AttributeCollectionDeserializer.FromAsset(Asset);
            return SpawnFromAttributeCollection(Alias, model, componentOverrides);
        }
        [Obsolete("Use State.ImportAsset to  ensure refernce messages are emitted.")]
        public Entity SpawnFromAsset(String Asset, params object[] componentOverrides)
        {
            var model = AttributeCollectionDeserializer.FromAsset(Asset);
            return SpawnFromAttributeCollection(model, componentOverrides);
        }

        public Entity SpawnFromAttributeCollection(String Alias, AttributeCollection model, params object[] componentOverrides)
        {
            List<object> componentList = AttributeCollectionToComponentSet(model);


            Entity spawn = new Entity();

            spawn.Id = Entities.Alias(Alias, spawn);
            //spawn.Id = Entities.Index(spawn);
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
        public Entity SpawnFromAttributeCollection(AttributeCollection model, params object[] componentOverrides)
        {
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

        public static object AttributeCollectionAs<T>(AttributeCollection src)
        {
            return AttributeCollectionAs(typeof(T), src);
        }

        public static object AttributeCollectionAs(Type t, AttributeCollection src)
        {
            var ret = Activator.CreateInstance(t);
            foreach(var v in t.GetProperties())
            {
                if (!v.CanWrite) continue;
                if (src[v.Name]==null)
                    continue;
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
        public static object AttributeAsObject(Type t, object src)
        {
            if (t == src.GetType())
                return src;
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
            if (t == typeof(RectangleF) && src.GetType() == typeof(Vector4))
            {
                Vector4 v = (Vector4)src;
                return new RectangleF(v.X, v.Y, v.Z, v.W);
            }
            if (t == typeof(Rectangle) && src.GetType() == typeof(Vector4))
            {
                Vector4 v = (Vector4)src;
                return new Rectangle((int)v.X, (int)v.Y, (int)v.Z, (int)v.W);
            }
            if (t == typeof(Point) && src.GetType() == typeof(Vector2))
            {
                Vector2 v = (Vector2)src;
                return new Point((int)v.X, (int)v.Y);
            }
            if (t.IsGenericType && t.GetGenericTypeDefinition()== typeof(Dictionary<,>))
            {
                Type[] arguments = t.GetGenericArguments();
                Type keyType = arguments[0];
                Type valueType = arguments[1];
                if (keyType==typeof(String))
                    return AttributeCollectionAsStringDictionary(keyType, valueType, (Dictionary<String, Object>)src);
                if (keyType==typeof(int))
                    return AttributeCollectionAsIntDictionary(keyType, valueType, (Dictionary<int, Object>)src);
                throw new Exception($"Unexpected Key Type {keyType.Name}");
                //continue;
            }
            return AttributeCollectionAs(t, (AttributeCollection)src);
        }
        private static object AttributeCollectionAsStringDictionary(Type kType, Type vTyle, Dictionary<String,object> dict)
        {
            var dType = typeof(Dictionary<,>).MakeGenericType(kType, vTyle);

            var args = new object[] { StringComparer.CurrentCultureIgnoreCase };


            var ret =   Activator.CreateInstance(dType, args);
            foreach (KeyValuePair<String,object> kvp in dict)
            {

                dType.GetMethod("Add")
                     //.MakeGenericMethod(kType, vTyle)
                     .Invoke(ret, new object[] { kvp.Key, AttributeCollectionAs(vTyle, (AttributeCollection)kvp.Value) });
                //ret.As  kvp.Key.As(kType)] = kvp.Value.As(vTyle);
            }
            return ret;
        }
        private static object AttributeCollectionAsIntDictionary(Type kType, Type vTyle, Dictionary<int, object> dict)
        {
            var dType = typeof(Dictionary<,>).MakeGenericType(kType, vTyle);
            var ret = Activator.CreateInstance(dType);
            foreach (KeyValuePair<int, object> kvp in dict)
            {

                dType.GetMethod("Add")
                     //.MakeGenericMethod(kType, vTyle)
                     .Invoke(ret, new object[] { kvp.Key, AttributeCollectionAs(vTyle, (AttributeCollection)kvp.Value) });
                //ret.As  kvp.Key.As(kType)] = kvp.Value.As(vTyle);
            }
            return ret;
        }
        public static object AttributeAsArrayOf(Type t, object[] src)
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
                if (t == typeof(RectangleF) && src.Length>0 && src[0].GetType()==typeof(Vector4))
                {
                    Vector4 v = (Vector4)src[i];
                    ret.SetValue(new RectangleF(v.X, v.Y, v.Z, v.W),i);
                    continue;
                }
                var val = AttributeCollectionAs(t, (AttributeCollection)src[i]);
                ret.SetValue(val, i);
            }
            //Activator.CreateInstance(t);
            return ret;
            //return ret.ToArray();
        }

        public static EntityComponentContext ReadFromStream(BinaryReader rdr)
        {
            EntityComponentContext ecc = new EntityComponentContext();
            var compMap = ReadAliasIdMapFromStream(rdr);
            //var entCount = rdr.ReadInt32();
            Dictionary<int, int> eIdMap = new Dictionary<int, int>();
            while(rdr.ReadBoolean())
            {
                InsertEntityFromStream(rdr, ecc, compMap, eIdMap);
            }
            foreach(Entity v in ecc.Entities.Values())
            {
                if (v == null) continue;
                if (v.Ideal!=0)
                    v.Ideal = eIdMap[v.Ideal];
                if (v.Parent!=0)
                    v.Parent = eIdMap[v.Parent];
            }
            return ecc;
        }
        private static void InsertEntityFromStream(BinaryReader rdr, EntityComponentContext ecc, Dictionary<int,string> cMap, Dictionary<int,int> entIdMap)
        {
            var alias = String.Empty;
            if (rdr.ReadBoolean())
                alias = rdr.ReadString();
            
            int ogEntID = rdr.ReadInt32();
            int ogEntIdeal = rdr.ReadInt32();
            int ogEntParent = rdr.ReadInt32();
            //int entCompCount = rdr.ReadInt32();
            List<object> componentList = new List<object>();
            while(rdr.ReadBoolean())
            {
                int compId = rdr.ReadInt32();
                Type tt = ComponentCache.ComponentAliasMap[cMap[compId]].OriginatingType;
                componentList.Add(rdr.ReadPrimitiveType(tt));
            }
            Entity newEnt;
            if (!string.IsNullOrEmpty(alias))
                newEnt = ecc.SpawnEntity(alias, componentList.ToArray());
            else
                newEnt = ecc.SpawnEntity(componentList.ToArray());
            newEnt.Ideal = ogEntIdeal;
            newEnt.Parent = ogEntParent;
            entIdMap.Add(ogEntID, newEnt.Id);
        }
        private static Dictionary<int,string> ReadAliasIdMapFromStream(BinaryReader rdr)
        {
            Dictionary<int, string> ret = new Dictionary<int, string>();

            
            while(rdr.ReadBoolean())
            {
                var val = rdr.ReadString();
                var key = rdr.ReadInt32();
                ret.Add(key, val);
            }
            return ret;
        }


        public static void WriteToStream(BinaryWriter wtr, EntityComponentContext ecc)
        {
            ecc.WriteToStream(wtr);
        }
        private void WriteToStream(BinaryWriter wtr)
        {
            //wtr.Write(ComponentCache.ComponentAliasMap.Count-1);                          //Write the number of types of components to expect
            foreach(string v in ComponentCache.ComponentAliasMap.Keys())                   //Foreach of the components to expect
            {
                wtr.Write(true);
                wtr.Write(v);                                                                  //name of component
                wtr.Write(ComponentCache.ComponentAliasMap[v].CTypeID);                        //mapped to Integer
            }
            wtr.Write(false);
            //wtr.Write(Entities.Count-1);                                                 //Write the number of Entities to expect (in total).
            HashSet<int> written = new HashSet<int>();
            foreach(var v in Entities.Keys())                                                 //Foreach entity that is named.
            {
                Entity ent = Entities[v];
                if (!ShouldWrite(ent))
                    continue;
                wtr.Write(true);
                wtr.Write(true);                                                              //Write flag that it is aliased
                wtr.Write(v);                                                                 //Write Alias
                
                written.Add(ent.Id);
                WriteToStream(wtr, ent);
            }
            foreach(Entity v in Entities.Values())
            {
                if (v == null) continue;
                if (!written.Add(v.Id))
                    continue;
                if (!ShouldWrite(v))
                    continue;
                wtr.Write(true);
                wtr.Write(false);
                WriteToStream(wtr, v);
            }
            wtr.Write(false);
        }

        private bool ShouldWrite(Entity e)
        {
            if (e == null) return false;
            foreach(var v in this.Components.GetComponentsByEntity(e))
            {
                if (!((ComponentCache.ComponentTypeData[v.GetType()].Flags & ComponentFlags.DoNotSerialize) == ComponentFlags.DoNotSerialize))
                    return true;
            }
            return false;
        }

        private void WriteToStream(BinaryWriter wtr, Entity ent)
        {
            wtr.Write(ent.Id);                                                           //Write Id
            wtr.Write(ent.Ideal);                                                        //Write Ideal
            wtr.Write(ent.Parent);                                                       //Write Parent
            var componentList = Components.GetComponentsByEntity(ent);
            //wtr.Write(componentList.Count());
            foreach(var component in componentList)
            {
                
                var compTypeDat = ComponentCache.ComponentTypeData[component.GetType()];
                if ((compTypeDat.Flags & ComponentFlags.DoNotSerialize) == ComponentFlags.DoNotSerialize)
                    continue;

                wtr.Write(true); 
                wtr.Write(compTypeDat.CTypeID);
                wtr.WritePrimative(component);
            }
            wtr.Write(false);
        }
        
        public IEnumerable<Tuple<String,String>> ExtractReferences()
        {
            foreach(Type compType in ComponentCache.ComponentTypeData.Keys)
            {
                if (typeof(IReferenceAssets).IsAssignableFrom(compType))
                {
                    foreach(var v in Components.GetEntitiesByComponentType(compType))
                    {
                        var ira = (IReferenceAssets)v.Value;
                        foreach(var r in ira.AssetReferences)
                        {
                            yield return new Tuple<string, string>(r.Item1, r.Item2);
                        }
                    }
                }
            }
        }
        
    }
}