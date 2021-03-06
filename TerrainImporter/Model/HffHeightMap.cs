﻿using System;
using System.IO;
using L3dtFileManager.Hff;

namespace TerrainImporter.Model
{
    internal class HffHeightMap : HeightMap
    {
        private HffFile file;

        public override int Width
        {
            get
            {
                return (int)file.header.width;
            }
        }

        public override int Height
        {
            get
            {
                return (int)file.header.height;
            }
        }

        public float VerticalScale
        {
            get
            {
                return file.header.verticalScale;
            }
        }

        public float VerticalOffset
        {
            get
            {
                return file.header.verticalOffset;
            }
        }

        public HffHeightMap(string filename)
        {
            L3dtFileManager.L3dtFileManager hfzManager = new L3dtFileManager.L3dtFileManager();
            if (filename.EndsWith(".hff"))
            {
                this.file = hfzManager.loadHffFile(filename);
            }
            else
            {
                throw new Exception("Not a Hff map file");
            }
        }

        public override void Dispose()
        {
            this.file = null;
        }

        public override float GetHeight(int x, int y)
        {
            return file.getPixelAt((uint)x+1,(uint)y+1).data;
        }
    }
}