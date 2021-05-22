namespace BareE.Transvoxel
{
    internal struct cellConstructionData
    {
        private uint[] createdVerticies;

        public void SaveVertex(int cellVertexIndex, uint globalVertexIndex)
        {
            (createdVerticies ?? new uint[4])[cellVertexIndex] = globalVertexIndex;
        }

        public uint retreiveVertex(int cellVertexIndex)
        {
            return (createdVerticies ?? new uint[4])[cellVertexIndex];
        }
    }
}