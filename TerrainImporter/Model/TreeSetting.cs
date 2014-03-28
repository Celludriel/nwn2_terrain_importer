using NWN2Toolset.NWN2.Data.Blueprints;

namespace TerrainImporter.Model
{
  public class TreeSetting
  {
    public float[,] Scale = new float[3, 2];
    public NWN2TreeBlueprint Tree;
    public byte Weight;
  }
}
