
using System;

namespace LilScrapper.Components
{
    [Flags]
    public enum SpritePhysicsAttribute
    {
        /// <summary>
        /// Triggers a collision but does not alter movement path
        /// </summary>
        Immaterial = 1,
        /// <summary>
        /// Act as though edges are 'sloped'
        /// </summary>
        Sloped = 2,
        /// <summary>
        /// Speed up velocity... figured it could be used for paths.
        /// </summary>
        Smooth = 4,

        /// <summary>
        /// Slowdown velocity
        /// </summary>
        Sticky = 8,

        Bouncy = 16,

        /// <summary>
        /// Indicates that this unit can not be moved by collisions.
        /// </summary>
        Immobile = 32,


        Flying = 64,

        WaterBound = 128,
        Waterphobic = 256
    }
}
