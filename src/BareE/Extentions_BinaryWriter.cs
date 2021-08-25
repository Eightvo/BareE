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
            if (t == typeof(Enum))
            {
                return rdr.ReadInt32().As(t);
            }

            if (t.IsArray)
            {
                var elementType = t.GetElementType();
                int elements = rdr.ReadInt32();
                Array ret = Array.CreateInstance(elementType, elements);
                for (int i = 0; i < elements; i++)
                    ret.SetValue( rdr.ReadPrimitiveType(elementType),i);

                return ret;
            }
            //It is a complex time.
            var retob = Activator.CreateInstance(t);
            foreach (var v in t.GetProperties())
            {
                v.SetValue(retob, rdr.ReadPrimitiveType(v.PropertyType));
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

            //It is a complex time.
            foreach (var v in o.GetType().GetProperties())
            {
                wtr.WritePrimative(v.GetValue(o));
            }
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

    }
}
