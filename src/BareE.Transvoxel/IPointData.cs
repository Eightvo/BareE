namespace BareE.Transvoxel
{
    public interface IPointData
    {
        /// <summary>
        /// The sample Value indicates whether the sample is below or above the isosurface.
        /// According to the .pdf, the value 0 is not special and can be treated as either in empty space or in the surface.
        /// A positive value indicates the sample is above the surface in empty space and not within an object.
        /// A Negative value indicates the sample is below the isosurface.
        /// According to the .pdf, the value 0 is not special and can be treated as either in empty space or in the surface.
        /// TransvoxelDODN implements the algrorithm such that 0 indicates the sample is in empty space.
        /// </summary>
        float SampleValue { get; }

        float CreateEmptySpaceSamplevalue();
    }
}