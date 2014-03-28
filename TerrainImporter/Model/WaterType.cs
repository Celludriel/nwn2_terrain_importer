namespace TerrainImporter.Model
{
    internal enum WaterType : ushort
    {
        None = (ushort)0,
        Ocean = (ushort)10,
        OceanShore = (ushort)11,
        Lake = (ushort)30,
        LakeShore = (ushort)31,
        Watertable = (ushort)90,
    }
}
