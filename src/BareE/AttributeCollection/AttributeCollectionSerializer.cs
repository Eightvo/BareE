using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{
    public struct SerializationSettings
    {
        public bool indent;
    }
    public static class AttributeCollectionSerializer
    {

        static void Indent(int indent, System.IO.StreamWriter wtr)
        {
              for (int i = 0; i < indent; i++)
                   wtr.Write("  ");
        }
        static bool needsQuote(String inp)
        {
            foreach (var v in inp)
                if (Char.IsWhiteSpace(v) ||
                     v == '.'
                    || v == '/'
                    || v == '\\'
                    || v == '#')
                    return true;
            return false;
        }

        public static void Write(object o, String filename, SerializationSettings settings=default)
        {
            using(var wtr=new System.IO.StreamWriter(filename))
            {
                Write(o, wtr, settings);
            }
        }
        public static void Write(object o, System.IO.StreamWriter wtr,SerializationSettings settings=default)
        {
            DoWrite(o, wtr, settings, 0);
        }

        static void DoWrite(object o, System.IO.StreamWriter wtr, SerializationSettings settings, int indent)
        {
            if (o == null) { wtr.Write("null"); return; }
            if (o.GetType().IsPrimitive)
            {
                wtr.Write(o.ToString());
                return;
            }
            if (o.GetType() == typeof(String))
            {
                var oStr = $"{o}";
                if (needsQuote(oStr))
                    wtr.Write($"\"{oStr}\"");
                else wtr.Write($"{oStr}");
                return;
            }
            if (o.GetType().IsEnum)
            {
                wtr.Write(Enum.GetName(o.GetType(), o));
                return;
            }
            if (o.GetType() == typeof(Vector2))
            {
                var v = (Vector2)o;
                wtr.Write($"<{v.X}, {v.Y}>");
                return;
            }
            if (o.GetType() == typeof(Vector3))
            {
                var v = (Vector3)o;
                wtr.Write($"<{v.X}, {v.Y}, {v.Z}>");
                return;
            }
            if (o.GetType() == typeof(Vector4))
            {
                var v = (Vector4)o;
                wtr.Write($"<{v.X}, {v.Y}, {v.Z}, {v.W}>");
                return;
            }
            if (o.GetType() == typeof(RectangleF))
            {
                var v = (RectangleF)o;
                wtr.Write($"<{v.X}, {v.Y}, {v.Width}, {v.Height}>");
                return;
            }

            if (o.GetType().IsArray)
            {
                wtr.WriteLine($"[");
                var oL = (object[])o;
                for (int i = 0; i < oL.Length; i++)
                {
                    var subObj = oL[i];
                    DoWrite(subObj, wtr,settings, indent + 1);
                    if (i < oL.Length - 1)
                    {
                        wtr.Write(",");
                        if (settings.indent)
                            wtr.WriteLine();
                    }
                }
                if (settings.indent)
                {
                    wtr.WriteLine();
                    Indent(indent, wtr);
                }
                wtr.Write($"]");
                return;
            }
            if (o.GetType().IsAssignableTo(typeof(AttributeCollection)))
            {
                var ac = (AttributeCollection)o;
                if (settings.indent)
                    Indent(indent, wtr);
                wtr.WriteLine("{");
                for (int i = 0; i < ac.Attributes.Count; i++)
                {
                    var AttributeName = ac.Attributes[i];
                    Indent(indent, wtr);
                    wtr.Write($"{AttributeName.AttributeName} : ");
                    DoWrite(ac[AttributeName.AttributeName], wtr,settings, indent + 1);
                    if (i < ac.Attributes.Count - 1)
                    {
                        wtr.Write(",");
                        if (settings.indent)
                            wtr.WriteLine();
                    }
                }
                if (settings.indent)
                {
                    wtr.WriteLine();
                    Indent(indent, wtr);
                }
                wtr.Write("}");
                return;
            }

        }
    }
}
