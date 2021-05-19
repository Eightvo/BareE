using System;

namespace BareE.DataStructures
{
    public class EntityComponentContext
    {
        public AliasMap<Entity> Entities;
        public ComponentCache Components;

        public EntityComponentContext() : this(new AliasMap<Entity>(), new ComponentCache()) { }
        public EntityComponentContext(AliasMap<Entity> _entities, ComponentCache _components)
        {
            Entities = _entities;
            Components = _components;
        }

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
        public Entity SpawnEntity(String Alias, params object[] componentList)
        {
            Entity spawn = new Entity();
            spawn.Id = Entities.Alias(Alias,spawn);
            foreach (var component in componentList)
            {
                Components.SetComponent(spawn, component);
            }
            return spawn;
        }
        public Entity SpawnChildEntity(Entity parent, params object[] componetLst)
        {
            var ent = SpawnEntity(componetLst);
            ent.Parent = parent.Id;
            return ent;
        }
        public Entity SpawnChildEntity(String Alias, Entity parent, params object[] componetLst)
        {
            var ent = SpawnEntity(Alias, componetLst);
            ent.Parent = parent.Id;
            return ent;
        }
    }
}
