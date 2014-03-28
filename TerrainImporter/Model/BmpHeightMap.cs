using System.Drawing;
using System.IO;

namespace TerrainImporter.Model
{
    public class BmpHeightMap : HeightMap
    {
        public Bitmap bmp;

        public override int Height
        {
            get
            {
                return this.bmp.Height;
            }
        }

        public override int Width
        {
            get
            {
                return this.bmp.Width;
            }
        }

        public BmpHeightMap(Stream inf)
        {
            this.bmp = new Bitmap(inf);
        }

        public override float GetHeight(int x, int y)
        {
            return this.bmp.GetPixel(x, this.bmp.Height - y - 1).GetBrightness();
        }
    }
}
