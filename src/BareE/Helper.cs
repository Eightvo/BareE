using System;
using System.Numerics;

public static class Helper
{
    public static String EncodeColor(Vector4 input)
    {
        var r = ((int)(input.X * 255)).ToString("x").PadLeft(2, '0');
        var g = ((int)(input.Y * 255)).ToString("x").PadLeft(2, '0');
        var b = ((int)(input.Z * 255)).ToString("x").PadLeft(2, '0');
        var a = ((int)(input.W * 255)).ToString("x").PadLeft(2, '0');
        return $"#{r}{g}{b}{a}";
    }
    public static String EncodeColor(Vector3 input)
    {
        var r = ((int)(input.X * 255)).ToString("x").PadLeft(2, '0');
        var g = ((int)(input.Y * 255)).ToString("x").PadLeft(2, '0');
        var b = ((int)(input.Z * 255)).ToString("x").PadLeft(2, '0');
        return $"#{r}{g}{b}";
    }
    public static Vector4 DecodeColor(String input)
    {
        if (input.StartsWith('#'))
        {
            return DecodeHexColor(input);
        }

        switch (input.ToLower())
        {
            case "white": return new Vector4(1, 1, 1, 1);
            case "black": return new Vector4(0, 0, 0, 1);
            case "red": return new Vector4(1, 0, 0, 1);
            case "green": return new Vector4(0, 1, 0, 1);
            case "blue": return new Vector4(0, 0, 1, 1);
            case "none": return new Vector4(0, 0, 0, 0);

        }
        throw new Exception($"Unknown Color {input}");
    }
    public static Vector4 DecodeHexColor(String input)
    {
        if (input.Length != 7)
            if (input.Length != 9)
                throw new Exception($"Invalid Hex color {input}");

        var rS = input.Substring(1, 2);
        var gS = input.Substring(3, 2);
        var bS = input.Substring(5, 2);
        var aS = input.Length == 9 ? input.Substring(7, 2) : "FF";
        var rV = int.Parse(rS, System.Globalization.NumberStyles.HexNumber);
        var gV = int.Parse(gS, System.Globalization.NumberStyles.HexNumber);
        var bV = int.Parse(bS, System.Globalization.NumberStyles.HexNumber);
        var aV = int.Parse(aS, System.Globalization.NumberStyles.HexNumber);
        return new Vector4(rV / 255.0f, gV / 255.0f, bV / 255.0f, aV / 255.0f);
    }
}
