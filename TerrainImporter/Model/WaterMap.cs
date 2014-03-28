namespace TerrainImporter.Model
{
    public abstract class WaterMap
    {
        public abstract int Height { get; }

        public abstract int Width { get; }

        public abstract bool PaintWater(int x, int y);

        public abstract float GetWaterLevel(int x, int y);

        public virtual void Dispose()
        {
        }
    }
}
