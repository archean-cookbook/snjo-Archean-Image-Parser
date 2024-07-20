using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archean_Image_Parser_Class_Library
{
    internal class PixelChunk
    {
        public readonly int x;
        public readonly int y;
        public readonly int width;
        public readonly int height;
        public readonly int palette;
        public int size
        {
            get { return width * height; }
        }
        public int smallestSide
        {
            get { return Math.Min(width, height); }
        }
        public int weightedSize
        {
            get { return (size/4) + smallestSide; }
        }

        public PixelChunk(int x, int y, int width, int height, int palette)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.palette = palette;
        }
    }
}
