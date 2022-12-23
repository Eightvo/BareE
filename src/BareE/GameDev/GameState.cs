using BareE.DataStructures;
using BareE.Messages;

using System.Collections.Generic;

namespace BareE.GameDev
{
    public class GameState
    {
        public GameClock Clock;
        public InputHandler Input;
        public MessageQueue Messages;
        public EntityComponentContext ECC;

        public GameState()
        {
            Clock = new GameClock();
            Input = new InputHandler();
            Messages = new MessageQueue();
            ECC = new EntityComponentContext();
        }

        private void EmitMetaDefinitions(object[] metadefs)
        {
            foreach(var meta in metadefs)
            {
                var metaAC = (AttributeCollection)meta;
                var metaType = metaAC.DataAs<string>("Type");
                if (string.IsNullOrEmpty(metaType))
                    Messages.EmitMsg<EmitText>(EmitText.Warning("Meta type not specified"));
                Messages.EmitMsg<EmitMeta>(new EmitMeta() { Type = metaType,  MetaDefinition = metaAC });
            }
        }
        private void EmitReferences(object[] refDefs)
        {
            foreach(var r in refDefs)
            {
                var rAC = (AttributeCollection)r;
                var rType = rAC.DataAs<string>("Type");
                if (string.IsNullOrEmpty(rType))
                    Messages.EmitMsg<EmitText>(EmitText.Warning("Reference Type not specified"));
                Messages.EmitMsg<EmitAsset>(new EmitAsset() { Type = rType, ReferenceDefinition = rAC });
            }
        }
        static List<string> refNameList = new List<string>() { "Reference", "REferences", "Ref", "Refs" };
        public Entity ImportAsset(string alias, string Defenition, params object[] componentOverrides)
        {
            AttributeCollection def = AttributeCollectionDeserializer.FromAsset(Defenition);
            if (def.HasAttribute("Meta"))
            {
                EmitMetaDefinitions(def.DataAs<object[]>("Meta"));
            }
            foreach (var refname in refNameList)
            {
                if (def.HasAttribute(refname))
                {
                    EmitReferences(def.DataAs<object[]>(refname));
                    break;
                }
            }
            return ECC.SpawnFromAttributeCollection(alias, def, componentOverrides);
        }
        public Entity ImportAsset(string Defenition, params object[] componentOverrides)
        {
            AttributeCollection def = AttributeCollectionDeserializer.FromAsset(Defenition);
            if (def.HasAttribute("Meta"))
            {
                EmitMetaDefinitions(def.DataAs<object[]>("Meta"));
            }
            foreach (var refname in refNameList)
            {
                if (def.HasAttribute(refname))
                {
                    EmitReferences(def.DataAs<object[]>(refname));
                    break;
                }
            }
            return ECC.SpawnFromAttributeCollection(def, componentOverrides);
        }
    }
}