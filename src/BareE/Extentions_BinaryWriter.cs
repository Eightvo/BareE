using BareE.DataStructures;

using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BareE
{

    public static partial class Extentions
    {
        private static Dictionary<byte, Type> primitiveTypeMap = new Dictionary<byte, Type>()
        {
              {0b0001,typeof(bool)},
              {0b0010,typeof(char)},
              {0b0011,typeof(int)},
              {0b0100,typeof(uint)},
              {0b0101,typeof(long)},
              {0b0110,typeof(float)},
              {0b0111,typeof(double)},
              {0b1000,typeof(string)},
              {0b1001,typeof(Vector2)},
              {0b1010,typeof(Vector3)},
              {0b1011,typeof(Vector4)},
             {0b1100,typeof(AttributeCollection)},
             {0b1101,typeof(Object)},
             {0b1110,typeof(RectangleF)},
        };
        private static Dictionary<byte, Func<BinaryReader, object>> primitiveTypeReaders = new Dictionary<byte, Func<BinaryReader, object>>()
        {
              {0b0001,new Func<BinaryReader, object>((b)=>{ return b.ReadBoolean(); }) } ,
              {0b0010,new Func<BinaryReader, object>((b)=>{ return b.ReadChar(); }) } ,
              {0b0011,new Func<BinaryReader, object>((b)=>{ return b.ReadInt32(); }) } ,
              {0b0100,new Func<BinaryReader, object>((b)=>{ return b.ReadUInt32(); }) } ,
              {0b0101,new Func<BinaryReader, object>((b)=>{ return b.ReadInt64(); }) } ,
              {0b0110,new Func<BinaryReader, object>((b)=>{ return b.ReadSingle(); }) } ,
              {0b0111,new Func<BinaryReader, object>((b)=>{ return b.ReadDouble(); }) } ,
              {0b1000,new Func<BinaryReader, object>((b)=>{ return b.ReadString(); }) } ,
              {0b1001,new Func<BinaryReader, object>((b)=>{ return b.ReadVector2(); }) } ,
              {0b1010,new Func<BinaryReader, object>((b)=>{ return b.ReadVector3(); }) } ,
              {0b1011,new Func<BinaryReader, object>((b)=>{ return b.ReadVector4(); })},
              {0b1100,new Func<BinaryReader, object>((b)=>{ return b.ReadAttributeCollection(); })},
              {0b1101,new Func<BinaryReader, object>((b)=>{ return b.ReadAttributeCollection(); })},
              {0b1110,new Func<BinaryReader, object>((b)=>{ return b.ReadRectangleF(); }) },
        };
        public static object ReadPrimitiveType(this BinaryReader rdr, Type t)
        {
            if (t == typeof(bool))
                return rdr.ReadBoolean();

            if (t == typeof(byte))
                return rdr.ReadByte();

            if (t == typeof(Char))
                return rdr.ReadChar();

            if (t == typeof(short))
                return rdr.ReadInt16();

            if (t == typeof(ushort))
            {
                return rdr.ReadUInt16();
                //return;
            }

            if (t == typeof(int))
            {
                return rdr.ReadInt32();
                //return;
            }

            if (t == typeof(uint))
            {
                return rdr.ReadUInt32();
                //return;
            }

            if (t == typeof(long))
            {
                return rdr.ReadInt64();
                //return;
            }

            if (t == typeof(ulong))
            {
                return rdr.ReadUInt64();

            }

            if (t == typeof(String))
            {
                return rdr.ReadString();

            }

            if (t == typeof(Decimal))
            {
                return rdr.ReadDecimal();

            }
            if (t == typeof(Double))
            {
                return rdr.ReadDouble();

            }
            if (t == typeof(float))
            {
                return rdr.ReadSingle();
            }




            if (t == typeof(Vector2))
            {
                return rdr.ReadVector2();
            }


            if (t == typeof(Vector3))
            {
                return rdr.ReadVector3();

            }

            if (t==typeof(Vector4))
            {
                return rdr.ReadVector4();
            }
            if (t==typeof(RectangleF))
            {
                return rdr.ReadRectangleF();
            }
            if (t == typeof(Enum))
            {
                return rdr.ReadInt32().As(t);
            }
            if (IsSameOrSubclass(typeof(AttributeCollection), t))
            {
                return rdr.ReadAttributeCollection();
            }
            if (t.IsArray)
            {
                var elementType = t.GetElementType();
                int elements = rdr.ReadInt32();
                Array ret = Array.CreateInstance(elementType, elements);
                for (int i = 0; i < elements; i++)
                {
                    if (elementType != typeof(Object))
                        ret.SetValue(rdr.ReadPrimitiveType(elementType), i);
                    else
                        ret.SetValue(rdr.ReadPrimitiveType(typeof(AttributeCollection)), i);
                }

                return ret;
            }
            //It is a complex time.
            var retob = Activator.CreateInstance(t);
            foreach (var v in t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (v.IsDefined(typeof(Newtonsoft.Json.JsonIgnoreAttribute), true))
                    continue;
                v.SetValue(retob, rdr.ReadPrimitiveType(v.PropertyType));
            }
            foreach (var v in t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (v.IsDefined(typeof(Newtonsoft.Json.JsonIgnoreAttribute), true))
                    continue;
                v.SetValue(retob, rdr.ReadPrimitiveType(v.FieldType));
            }
            return retob;
            throw new Exception();
        }
        public static void WritePrimative(this BinaryWriter wtr, object o)
        {
            //The only nullable object that should be passed to this function is a string

            if (o == null)
                o = String.Empty;
            if (o.GetType() == typeof(bool))
            {
                wtr.Write((bool)o);
                return;
            }

            if (o.GetType() == typeof(byte))
            {
                wtr.Write((byte)o);
                return;
            }

            if (o.GetType() == typeof(Char))
            {
                wtr.Write((char)o);
                return;
            }

            if (o.GetType() == typeof(short))
            {
                wtr.Write((short)o);
                return;
            }

            if (o.GetType() == typeof(ushort))
            {
                wtr.Write((ushort)o);
                return;
            }

            if (o.GetType() == typeof(int))
            {
                wtr.Write((int)o);
                return;
            }

            if (o.GetType() == typeof(uint))
            {
                wtr.Write((uint)o);
                return;
            }

            if (o.GetType() == typeof(long))
            {
                wtr.Write((long)o);
                return;
            }

            if (o.GetType() == typeof(ulong))
            {
                wtr.Write((ulong)o);
                return;
            }

            if (o.GetType() == typeof(String))
            {
                wtr.Write((String)o);
                return;
            }

            if (o.GetType() == typeof(Decimal))
            {
                wtr.Write((Decimal)o);
                return;
            }
            if (o.GetType() == typeof(Double))
            {
                wtr.Write((double)o);
                return;
            }
            if (o.GetType() == typeof(float))
            {
                wtr.Write((float)o);
                return;
            }

            if (o.GetType() == typeof(Vector2))
            {
                wtr.WriteVector2((Vector2)o);
                return;
            }

            if (o.GetType() == typeof(Vector3))
            {
                wtr.WriteVector3((Vector3)o);
                return;
            }

            if (o.GetType()==typeof(Vector4))
            {
                wtr.WriteVector4((Vector4)o);
                return;
            }
            if (o.GetType()==typeof(RectangleF))
            {
                wtr.WriteRectangleF((RectangleF)o);
                return;
            }
            if (o.GetType() == typeof(Enum))
            {
                wtr.Write((int)o);
                return;
            }

            if (o.GetType().IsArray)
            {
                var arry = o as Array;
                wtr.Write(arry.Length);
                for (int i = 0; i < arry.Length; i++)
                    wtr.WritePrimative(arry.GetValue(i));
                return;
            }
            if (IsSameOrSubclass(typeof(BareE.DataStructures.AttributeCollectionBase), o.GetType()))
            {
                WriteAttributeCollection(wtr, (AttributeCollectionBase)o);
                return;
            }
            //It is a complex time.
            foreach (var v in o.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (v.IsDefined(typeof(Newtonsoft.Json.JsonIgnoreAttribute), true))
                    continue;
                if (!v.CanWrite)
                    continue;
                object objVal = v.GetValue(o);
                wtr.WritePrimative(objVal);
            }
            foreach (var v in o.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (v.IsDefined(typeof(Newtonsoft.Json.JsonIgnoreAttribute), true))
                    continue;
                object objVal = v.GetValue(o);
                wtr.WritePrimative(objVal);
            }
        }

        private static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }

        public static T As<T>(this object o)
        {
            if (o == null || o == DBNull.Value) return default(T);
            return (T)Convert.ChangeType(o, typeof(T));
        }

        public static object As(this object o, Type type)
        {
            if (o == null || o == DBNull.Value) return null;
            return Convert.ChangeType(o, type);
        }


        public static void WriteAttributeCollection(this BinaryWriter wtr, AttributeCollectionBase col)
        {
            foreach(var v in col.Attributes)
            {
                if (v.Value == null) continue;
                wtr.Write(true);//There is another attribute.
                wtr.Write(v.AttributeName);//Name of the attribute

                KeyValuePair<byte,Type> k = primitiveTypeMap.Where(x => x.Value == v.Type).FirstOrDefault();

                if (k.Value == null)
                {
                    if (IsSameOrSubclass(typeof(AttributeCollectionBase), v.Type))
                        wtr.Write(0b1100);
                    else
                    {
                        if (v.Type.IsArray)
                        {
                            wtr.Write((byte)0b1111);
                            var elementTypeId = primitiveTypeMap.Where(x => x.Value == v.Type.GetElementType()).FirstOrDefault();
                            if (k.Value == null)
                            {
                                if (IsSameOrSubclass(typeof(AttributeCollection), v.Type.GetElementType()))
                                    wtr.Write((byte)0b1100);
                                else
                                {
                                    if (v.Type.GetElementType()==typeof(Object))
                                    {
                                        wtr.Write((byte)0b1101);
                                    }else
                                    throw new NotImplementedException($"{v.AttributeName} Array of {v.Type}");
                                }
                                    
                            }
                            else
                            {
                                wtr.Write(k.Key);
                            }
                        }else
                            throw new NotImplementedException($"{v.AttributeName} {v.Type}");
                    }
                }
                else
                {
                    wtr.Write(k.Key);//Write The primitive Type
                }
                wtr.WritePrimative(v.Value);

            }
            wtr.Write(false);
        }

        public static AttributeCollection ReadAttributeCollection(this BinaryReader rdr)
        {
            AttributeCollection col = new AttributeCollection();
            while(rdr.ReadBoolean())
            {
                var attributeName = rdr.ReadString();
                var primitiveType = rdr.ReadByte();

                object val;
                if (primitiveType!=0b1111)
                    val  = primitiveTypeReaders[primitiveType].Invoke(rdr);
                else
                {
                    var elementType = rdr.ReadByte();
                    var instance = Array.CreateInstance(primitiveTypeMap[elementType],1);
                    val = rdr.ReadPrimitiveType(instance.GetType());
                }
                col[attributeName] = val;
            }
            return col;
        }

        public static Vector2 ReadVector2(this BinaryReader rdr)
        {
            Vector2 vi = new Vector2();
            vi.X = rdr.ReadSingle();
            vi.Y = rdr.ReadSingle();
            return vi;
        }
        public static void WriteVector2(this BinaryWriter wtr, Vector2 vi)
        {
            wtr.Write(vi.X);
            wtr.Write(vi.Y);
        }

        public static Vector3 ReadVector3(this BinaryReader rdr)
        {
            Vector3 vi = new Vector3();
            vi.X = rdr.ReadSingle();
            vi.Y = rdr.ReadSingle();
            vi.Z = rdr.ReadSingle();
            return vi;
        }
        public static void WriteVector3(this BinaryWriter wtr, Vector3 vi)
        {
            wtr.Write(vi.X);
            wtr.Write(vi.Y);
            wtr.Write(vi.Z);
        }
        public static Vector4 ReadVector4(this BinaryReader rdr)
        {
            Vector4 vi = new Vector4();
            vi.X = rdr.ReadSingle();
            vi.Y = rdr.ReadSingle();
            vi.Z = rdr.ReadSingle();
            vi.W = rdr.ReadSingle();
            return vi;
        }
        public static void WriteVector4(this BinaryWriter wtr, Vector4 vi)
        {
            wtr.Write(vi.X);
            wtr.Write(vi.Y);
            wtr.Write(vi.Z);
            wtr.Write(vi.W);
        }
        public static RectangleF ReadRectangleF(this BinaryReader rdr)
        {
            return new RectangleF(rdr.ReadSingle(), rdr.ReadSingle(), rdr.ReadSingle(), rdr.ReadSingle());
            //wtr.Write(vi.X);
            //wtr.Write(vi.Y);
            //wtr.Write(vi.Z);
            //wtr.Write(vi.W);
        }
        public static void WriteRectangleF(this BinaryWriter wtr, RectangleF vi)
        {
            wtr.Write(vi.X);
            wtr.Write(vi.Y);
            wtr.Write(vi.Width);
            wtr.Write(vi.Height);
        }

    }
}
