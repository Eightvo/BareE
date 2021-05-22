using System;

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
    }
}