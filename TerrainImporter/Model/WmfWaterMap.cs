using System;
using System.IO;
using L3dtFileManager;
using L3dtFileManager.Wmf;

namespace TerrainImporter.Model
{
    public class WmfWaterMap : WaterMap
    {
        private WmfFile file;

        public override int Width
        {
            get
            {
                return Convert.ToInt32(file.header.width);
            }
        }

        public override int Height
        {
            get
            {
                return Convert.ToInt32(file.header.height);
            }
        }

        public WmfWaterMap(string filename)
        {
            L3dtFileManager.L3dtFileManager hfzManager = new L3dtFileManager.L3dtFileManager();
            if (filename.EndsWith(".wmf"))
            {
                this.file = hfzManager.loadWmfFile(filename);
            }
            else
            {
                throw new Exception("Not a Wmf map file");
            }
        }

        public override void Dispose()
        {
            this.file = null;
        }

        public override bool PaintWater(int x, int y)
        {
            WmfPixelInfo pixel = file.getPixelAt((uint)x + 1, (uint)y + 1);
            byte waterType = pixel.waterTypeId;
            if (waterType == 10 || waterType == 30)
            {
                return true;
            }
            return false;
        }

        public override float GetWaterLevel(int x, int y)
        {
            WmfPixelInfo pixel = file.getPixelAt((uint)x + 1, (uint)y + 1);
            return pixel.data;
        }
    }
}
