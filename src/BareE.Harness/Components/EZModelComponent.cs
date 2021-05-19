using BareE.DataStructures;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Numerics;

namespace BareE.Harness.Components
{
    [Component("EZModel")]
    public class EZModel
    {
        public String Root;
        public String Skin;
        public String NormalMap;
        public String SpecularMap;
        public String EmmissiveMap;
        public bool HasNormalMap;
        public Dictionary<Vector3, Vector3> ClrMap;
        public bool isStatic;
        [JsonIgnore]
        public EZRend.EZModel Model { get; set; }

        public static EZModel CreateTextured(String root, string skin)
        {
            return new EZModel() { Root = root, Skin = skin };
        }
        public static EZModel CreateTextured(String root, string skin, String normalMap, String specularMap="", String emmisiveMap="")
        {
            return new EZModel() { Root = root, Skin = skin, NormalMap = normalMap, HasNormalMap = true , SpecularMap=specularMap, EmmissiveMap=emmisiveMap};
        }
        public static EZModel CreateTextureNormalMapped(String root, String skin)
        {
            return new EZModel() { Root = root, Skin = skin, HasNormalMap = true };
        }
        public static EZModel CreateColored(String root, Dictionary<Vector3, Vector3> clrMap)
        {
            return new EZModel() { Root = root, ClrMap = clrMap };
        }
        public static EZModel CreateStaticTextured(String root, string skin)
        {
            return new EZModel() { Root = root, Skin = skin, isStatic = true };
        }
        public static EZModel CreateStaticColored(String root, Dictionary<Vector3, Vector3> clrMap)
        {
            return new EZModel() { Root = root, ClrMap = clrMap, isStatic = true };
        }
    }
}
    


 
