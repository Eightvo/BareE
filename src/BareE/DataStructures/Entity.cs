namespace BareE.DataStructures
{
    /// <summary>
    /// Used to collate components.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// An entity has all components associated with it's Ideal unless that componet has been specifically masked
        /// </summary>
        public int Ideal;
        /// <summary>
        /// Identifier for an Entity. An Entity ID is only static within an Entity Component Context.
        /// </summary>
        public int Id;
        /// <summary>
        /// Components are not derived from Parents. Parents represent a heirachy of Entities in the game scene.
        /// </summary>
        public int Parent;
    }
}