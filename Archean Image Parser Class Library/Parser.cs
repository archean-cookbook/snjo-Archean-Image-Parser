using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.Text;


namespace ParseLib
{
    public class Parser
    {
        public Image<Rgba32>? bitmap = null;
        public string loadFileName = "";
        readonly List<Rgba32> palette = new List<Rgba32>();
        int[,]? pixelGrid;

        int BrightnessRed = 100;
        int BrightnessGreen = 100;
        int BrightnessBlue = 100;

        public enum ProcessingMode
        {
            horizontal,
            vertical,
            rect,
            grid,
        }

        public enum ErrorCodes
        {
            OK = 0,
            Quit = 1,
            FileNotFound = 2,
            FileError = 3,
            ImageError = 4,
            ProcessingError = 5,
            OutputFileError = 6,
            TooFewArguments = 7,
            InvalidArguments = 8,
        }

        public bool LoadImage(string fileName, bool includeException = false)
        {
            Image<Rgba32>? bitmapFromFile;
            loadFileName = fileName;
            Debug.WriteLine($"Load file: {loadFileName}");
            try
            {
                //bitmapFromFile = new Bitmap(loadFileName);
                bitmapFromFile = Image.Load<Rgba32>(loadFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {loadFileName}");
                Console.WriteLine($"Check that this is a valid image file.");
                if (ex is SixLabors.ImageSharp.UnknownImageFormatException)
                {
                    Console.WriteLine("Available image formats:\r\n - BMP\r\n - QOI\r\n - JPEG\r\n - Webp\r\n - PNG\r\n - TGA\r\n - TIFF\r\n - GIF\r\n - PBM");
                }
                if (includeException)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                }
                return false;
            }


            //bitmap = Parser.CopyImage(bitmapFromFile); // Make a copy of the bitmap so it's not tied to the file.
            bitmap = bitmapFromFile;
            //bitmapFromFile.Dispose(); // Releases the bitmap file so it can be saved to outside this program.
            return true;
        }

        public string? ProcessImage(ProcessingMode processingMode, int brightnessRed, int brightnessGreen, int brightnessBlue)
        {
            // brighness values are used to reduce over-bright images on game screens.
            BrightnessRed = brightnessRed;
            BrightnessGreen = brightnessGreen;
            BrightnessBlue = brightnessBlue;
            string commands;
            //int[,]? pixelGrid;
            palette.Clear();

            // Build color palette and pixel grid
            if (bitmap != null)
            {
                pixelGrid = new int[bitmap.Width, bitmap.Height];
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        //System.Drawing.Color color = bitmap.GetPixel(x, y);

                        Rgba32 color = bitmap[x, y];


                        int match = -1;
                        int paletteNumber;
                        for (int i = 0; i < palette.Count; i++)
                        {
                            if (palette[i] == color)
                                match = i;
                        }
                        if (match == -1) // new color found
                        {
                            palette.Add(color);
                            paletteNumber = palette.Count - 1;
                        }
                        else
                        {
                            paletteNumber = match; // using existing color
                        }
                        pixelGrid[x, y] = paletteNumber;
                    }
                }
                // draw the pixel grid to UI textbox
                //TextBoxGrid.Text = MakePixelGrid(pixelGrid);
                // add the draw functions
                commands = CreateCommands(pixelGrid, System.IO.Path.GetFileNameWithoutExtension(loadFileName), processingMode);
                //TextBoxCommands.Text = commands;
                return commands;
            }
            return null;
        }

        public string MakePixelGrid(int[,] grid)
        {
            if (pixelGrid == null)
            {
                return "";
            }
            StringBuilder result = new();
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pcol = grid[x, y];
                    string pText = pcol.ToString();
                    if (palette[pcol].A == 0)
                        pText = ".";
                    if (palette.Count > 10)
                    {
                        result.Append(pText.PadLeft(4));
                    }
                    else
                    {
                        result.Append(pText);
                    }
                }
                result.AppendLine();
            }
            return result.ToString();
        }



        //uint PaletteToArchColor(int paletteNumber)
        //{
        //    System.Drawing.Color pColor = palette[paletteNumber];
        //    uint archColor = (uint)pColor.A * 256 * 256 * 256;
        //    archColor += (uint)pColor.R * 256 * 256;
        //    archColor += (uint)pColor.G * 256;
        //    archColor += (uint)pColor.B;
        //    return archColor;
        //}

        string PaletteToColorFunction(int paletteNumber)
        {
            Rgba32 pColor = palette[paletteNumber];
            float brightR = (float)BrightnessRed / 100f;
            float brightG = (float)BrightnessGreen / 100f;
            float brightB = (float)BrightnessBlue / 100f;
            int R = Math.Clamp((int)(pColor.R * brightR), 0, 255);
            int G = Math.Clamp((int)(pColor.G * brightG), 0, 255);
            int B = Math.Clamp((int)(pColor.B * brightB), 0, 255);

            return $"color({R},{G},{B},{pColor.A})";
        }

        string CreateCommands(int[,] grid, string name, ProcessingMode processingMode)
        {
            StringBuilder commands = new();
            // function name
            commands.AppendLine($"function @sprite_{name}($_screen:screen,$x:number,$y:number)");

            // create palette color variables
            for (int p = 0; p < palette.Count; p++)
            {
                string archColor = PaletteToColorFunction(p);
                commands.AppendLine($"\tvar $_c{p} = {archColor}");
            }

            // parsing modes, outputs draw statements
            if (processingMode == ProcessingMode.horizontal)
            {
                commands.AppendLine(CreateDrawCommandsHorizontal(grid));//, name));
            }
            else if (processingMode == ProcessingMode.vertical)
            {
                commands.AppendLine(CreateDrawCommandsVertical(grid));//, name));
            }
            else if (processingMode == ProcessingMode.rect)
            {
                commands.AppendLine(CreateDrawCommandsRect(grid)); //,name)
            }
            else if (processingMode == ProcessingMode.grid)
            {
                commands.AppendLine();
                commands.AppendLine(MakePixelGrid(grid));
            }

            return commands.ToString();
        }

        static (int width, int height) Chunk(int[,] grid, int x, int y)
        {
            // check for an area with identically colored pixels in a rectangle
            int width = 0;
            int height = 0;
            int sourcePalette = grid[x, y];
            //bool foundMismatch = false;
            int gridWidth = grid.GetLength(0);
            //while (!foundMismatch && x < gridWidth)
            while (x < gridWidth)
            {
                int foundPalette = grid[x, y];
                if (foundPalette != sourcePalette)
                {
                    //foundMismatch = true;
                    break;
                }
                int h = Parser.ColumnColorHeight(grid, x, y);
                if (width == 0)
                {
                    height = h;
                }
                if (h < height)
                {
                    //foundMismatch = true;
                    break;
                }
                else
                {
                    height = Math.Min(h, height);
                }
                x++;
                width++;
            }
            return (width, height);
        }

        static void RemoveChunkedPixelsFromGrid(int[,] grid, int x, int y, int right, int bottom)
        {
            // sets a grid pixel to -1, discarded when included in a chunk, so it's ignored by other later chunks
            for (int sx = x; sx < right; sx++)
            {
                for (int sy = y; sy < bottom; sy++)
                {
                    grid[sx, sy] = -1;
                }
            }
        }

        string CreateDrawCommandsRect(int[,] grid)//, string name)
        {
            // loops through entire image to find rects or lines with the same color to combine
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            StringBuilder commands = new();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    int paletteNum = grid[x, y];

                    if (paletteNum < 0)
                    {
                        continue;
                    }
                    int alpha = palette[paletteNum].A;
                    if (alpha == 0)
                    {
                        // skip
                    }
                    else
                    {
                        (int w, int h) = Chunk(grid, x, y);
                        RemoveChunkedPixelsFromGrid(grid, x, y, x + w, y + h);
                        if (w == 1 && h == 1)
                        {
                            commands.AppendLine($"\t$_screen.draw_point($x+{x},$y+{y},$_c{paletteNum})");
                        }
                        else if (w == 1) // vertical
                        {
                            commands.AppendLine($"\t$_screen.draw_line($x+{x},$y+{y},$x+{x},$y+{y + h},$_c{paletteNum}) ; line vertical h{h}");
                        }
                        else if (h == 1) // vertical
                        {
                            commands.AppendLine($"\t$_screen.draw_line($x+{x},$y+{y},$x+{x + w},$y+{y},$_c{paletteNum}) ; line horizontal w{w}");
                        }
                        else
                        {
                            commands.AppendLine($"\t$_screen.draw_rect($x+{x},$y+{y},$x+{x + w},$y+{y + h},0,$_c{paletteNum}) ; w{w} h{h}");
                        }
                    }
                }
            }

            return commands.ToString();
        }

        static string CreateDrawCommandsHorizontal(int[,] grid)//, string name)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            StringBuilder commands = new();

            for (int y = 0; y < height; y++)
            {
                // commands.AppendLine($"; --- row {y} ---");
                for (int x = 0; x < width;)
                {
                    int pNow = grid[x, y];

                    // check for contiguous line
                    int chunkLength = 1;

                    for (int l = x + 1; l < width; l++)
                    {
                        int pNext = grid[l, y];

                        if (pNext == pNow)
                        {
                            chunkLength++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (chunkLength < 2)
                    {
                        commands.AppendLine($"\t$_screen.draw_point($x+{x},$y+{y},$_c{pNow})");
                        x++;
                    }
                    else
                    {
                        commands.AppendLine($"\t$_screen.draw_line($x+{x},$y+{y}, $x+{x + chunkLength},$y+{y}, $_c{pNow}) ; line {chunkLength} ");
                        x += chunkLength;
                    }

                }
            }
            return commands.ToString();
        }

        static string CreateDrawCommandsVertical(int[,] grid)//, string name)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            StringBuilder commands = new();

            for (int x = 0; x < width; x++)
            {
                // commands.AppendLine($"; --- col {x} ---");
                for (int y = 0; y < height;)
                {
                    int pNow = grid[x, y];

                    // check for contiguous line
                    int chunkHeight = 1;

                    for (int l = y + 1; l < height; l++)
                    {
                        int pNext = grid[x, l];

                        if (pNext == pNow)
                        {
                            chunkHeight++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (chunkHeight < 2)
                    {
                        commands.AppendLine($"\t$_screen.draw_point($x+{x},$y+{y},$_c{pNow})");
                        y++;
                    }
                    else
                    {
                        commands.AppendLine($"\t$_screen.draw_line($x+{x},$y+{y}, $x+{x},$y+{y + chunkHeight}, $_c{pNow}) ; line {chunkHeight} ");
                        y += chunkHeight;
                    }

                }
            }
            return commands.ToString();
        }


        public static int ColumnColorHeight(int[,] grid, int x, int y)
        {
            // check for length of unbroken identically colored pixels in a column

            int height = 0;
            int sourcePalette = grid[x, y];
            int gridHeight = grid.GetLength(1);
            
            while (y < gridHeight)
            {
                int foundPalette = grid[x, y];
                if (foundPalette != sourcePalette)
                {
                    break;
                }
                y++;
                height++;
            }
            return height;
        }
    }
}
