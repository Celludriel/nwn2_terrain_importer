﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using L3dtFileManager;
using L3dtFileManager.Hfz;

namespace TerrainImporter.Model
{
    internal class Hf2HeightMap : HeightMap
    {
        private HfzFile file;

        public Hf2HeightMap(string filename)
        {
            L3dtFileManager.L3dtFileManager hfzManager = new L3dtFileManager.L3dtFileManager();
            if (filename.EndsWith(".hf2.gz") || filename.EndsWith(".hfz"))
            {
                this.file = hfzManager.loadHfzFile(filename, FileFormat.COMPRESSED);
            }
            else if (filename.EndsWith(".hf2"))
            {
                this.file = hfzManager.loadHfzFile(filename, FileFormat.UNCOMPRESSED);
            }
            else
            {
                throw new Exception("Not a HF2 map file");
            }
        }

        public override int Width
        {
            get
            {
                return Convert.ToInt32(this.file.header.nx);
            }
        }

        public override int Height
        {
            get
            {
                return Convert.ToInt32(this.file.header.ny);
            }
        }

        public float VerticalScale
        {
            get
            {
                return 0.0f;
            }
        }

        public float VerticalOffset
        {
            get
            {
                return 0.0f;
            }
        }

        public float getMaximumHeight()
        {
            return this.file.getMaxHeight();
        }

        public float getMinimumHeight()
        {
            return this.file.getMinHeight();
        }

        public override void Dispose()
        {
            this.file = null;
        }

        public override float GetHeight(int x, int y)
        {
            return file.getPixelAt((uint)x + 1, (uint)y + 1);
        }
    }
}
