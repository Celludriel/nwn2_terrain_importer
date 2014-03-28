namespace TerrainImporter.Model
{
    public abstract class HeightMap
    {
        public abstract int Height { get; }

        public abstract int Width { get; }

        public abstract float GetHeight(int x, int y);

        public virtual void Dispose()
        {
        }
    }
}
