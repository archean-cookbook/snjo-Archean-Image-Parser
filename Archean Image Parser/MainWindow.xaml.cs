using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Archean_Image_Parser
{
    public partial class MainWindow : Window
    {
        Bitmap? bitmap = null;
        string loadFileName = "";
        List<System.Drawing.Color> palette = [];
        int brightnessRed = 100;
        int brightnessGreen = 100;
        int brightnessBlue = 100;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonLoadImage_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Open load image");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.PNG;*.BMP;*.JPG;*.GIF)|*.PNG;*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog();
            if (result != null && result == true)
            {
                Bitmap? bitmapFromFile = null;
                loadFileName = openFileDialog.FileName;
                Debug.WriteLine($"Load file: {loadFileName}");
                try
                {
                    bitmapFromFile = new Bitmap(loadFileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {loadFileName}.\nCheck that this is a valid image file.\n\n{ex}");
                }
                
                if (bitmapFromFile == null)
                {
                    return;
                }
                bitmap = CopyImage(bitmapFromFile);
                bitmapFromFile.Dispose(); // Releases the bitmap file so it can be saved to outside this program.

                SourceImageView.Source = CreateBitmapSourceFromGdiBitmap(bitmap);
                SourceImageView.Width = bitmap.Width;
                SourceImageView.Height = bitmap.Height;
            }
        }

        private enum ProcessingMode
        {
            horizontal,
            vertical,
            rect,
        }

        private void ProcessHorizontal_Click(object sender, RoutedEventArgs e)
        {
            ProcessImage(ProcessingMode.horizontal);
        }

        private void ProcessVertical_Click(object sender, RoutedEventArgs e)
        {
            ProcessImage(ProcessingMode.vertical);
        }

        private void ProcessRect_Click(object sender, RoutedEventArgs e)
        {
            ProcessImage(ProcessingMode.rect);
        }

        int GetBrightness(string color)
        {
            //int bright = 100;
            string text = "100";
            switch (color)
            {
                case "Red":
                    text = BrightnessAdjustRed.Text;
                    break;
                case "Green":
                    text = BrightnessAdjustGreen.Text;
                    break;
                case "Blue":
                    text = BrightnessAdjustBlue.Text;
                    break;
            }

                
            if (int.TryParse(text, out int bright))
            {
                bright = Math.Clamp(bright, 1, 100);
                Debug.WriteLine("Brightness set to " + bright);
            }
            else
            {
                bright = 100;
                Debug.WriteLine("Brightness set to default " + bright);
            }
            return bright;
        }

        private void ProcessImage(ProcessingMode processingMode)
        {
            brightnessRed = GetBrightness("Red");
            brightnessGreen = GetBrightness("Green");
            brightnessBlue = GetBrightness("Blue");
            string commands = string.Empty;
            int[,]? pixelGrid;
            palette.Clear();
            Debug.WriteLine("Process image");
            if (bitmap != null)
            {
                pixelGrid = new int[bitmap.Width,bitmap.Height];
                for (int y = 0; y < bitmap.Height; y++)
                { 
                    for (int x = 0;  x < bitmap.Width; x++)
                    {
                        System.Drawing.Color color = bitmap.GetPixel(x, y);
                        int match = -1;
                        int paletteNumber;
                        for (int i = 0; i < palette.Count; i++)
                        {
                            if (palette[i] == color)
                                match = i;
                        }
                        if (match == -1)
                        {
                            palette.Add(color);
                            paletteNumber = palette.Count - 1;
                        }
                        else
                        {
                            paletteNumber = match;
                        }
                        pixelGrid[x, y] = paletteNumber;
                        //Debug.WriteLine($"pixel {x} {y}: {color}");
                    }
                    
                    
                }
                TextBoxGrid.Text = MakePixelGrid(pixelGrid);
                commands = CreateCommands(pixelGrid, System.IO.Path.GetFileNameWithoutExtension(loadFileName),processingMode);
                // PrintPalette();
                
                // Debug.WriteLine(commands);
                TextBoxCommands.Text = commands;
            }
        }

        string MakePixelGrid(int[,] grid)
        {
            StringBuilder result = new();
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            //Debug.WriteLine($"grid x{width} y{height}");
            for (int y = 0; y < height; y++)
            {
                for (int x = 0;x < width; x++)
                {
                    int pcol = grid[x, y];
                    string pText = pcol.ToString();
                    if (palette[pcol].A == 0)
                        pText = " ";
                    // Debug.WriteLine($"x{x} y{y}");
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

        void PrintPalette()
        {
            for (int p = 0; p < palette.Count; p++)
            {
                System.Drawing.Color pColor = palette[p];
                uint archColor = (uint)pColor.A * 256 * 256 * 256;
                archColor += (uint)pColor.R * 256 * 256;
                archColor += (uint)pColor.G * 256;
                archColor += (uint)pColor.B;
                Debug.WriteLine($"Color {p}: A:{palette[p].A} R:{palette[p].R} G:{palette[p].G} B:{palette[p].B} archColor: {archColor}");
            }
            
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        uint PaletteToArchColor(int paletteNumber)
        {
            System.Drawing.Color pColor = palette[paletteNumber];
            uint archColor = (uint)pColor.A * 256 * 256 * 256;
            archColor += (uint)pColor.R * 256 * 256;
            archColor += (uint)pColor.G * 256;
            archColor += (uint)pColor.B;
            return archColor;
        }

        string PaletteToColorFunction(int paletteNumber)
        {
            System.Drawing.Color pColor = palette[paletteNumber];
            float brightR = (float)brightnessRed / 100f;
            float brightG = (float)brightnessGreen / 100f;
            float brightB = (float)brightnessBlue / 100f;
            int R = (int)(pColor.R * brightR);
            int G = (int)(pColor.G * brightG);
            int B = (int)(pColor.B * brightB);

            return $"color({R},{G},{B},{pColor.A})";
        }

        string CreateCommands(int[,] grid, string name,ProcessingMode processingMode)
        {
            StringBuilder commands = new();
            commands.AppendLine($"function @sprite_{name}($_screen:screen,$x:number,$y:number)");
            for (int p = 0; p < palette.Count; p++)
            {
                string archColor = PaletteToColorFunction(p);
                commands.AppendLine($"\tvar $_c{p} = {archColor}");
            }

            if (processingMode == ProcessingMode.horizontal)
            {
                commands.AppendLine(CreateDrawCommandsHorizontal(grid, name));
            }
            else if (processingMode == ProcessingMode.vertical)
            {
                commands.AppendLine(CreateDrawCommandsVertical(grid, name));
            }
            else if (processingMode == ProcessingMode.rect)
            {
                commands.AppendLine(CreateDrawCommandsRect(grid, name));
            }
            

            return commands.ToString();
        }

        int ColumnColorHeight(int[,] grid, int x, int y)
        {
            bool foundMismatch = false;
            int height = 0;
            int sourcePalette = grid[x, y];
            int gridHeight = grid.GetLength(1);
            while (!foundMismatch && y < gridHeight)
            {
                //Debug.WriteLine($"column x:{x} y:{y} source:{sourcePalette}");
                int foundPalette = grid[x, y];
                if (foundPalette != sourcePalette)
                {
                    foundMismatch = true;
                    break;
                }
                y++;
                height++;
            }
            //Debug.WriteLine($"column result h:{height}");
            return height;
        }

        (int width,int height) Chunk(int[,] grid, int x, int y)
        {
            int width = 0;
            int height = 0;
            int sourcePalette = grid[x, y];
            bool foundMismatch = false;
            int gridWidth = grid.GetLength(0);
            while (!foundMismatch && x < gridWidth)
            {
                //Debug.WriteLine($"   chunk x:{x} y:{y} w:{width} h:{height}  source:{sourcePalette}");
                int foundPalette = grid[x, y];
                if (foundPalette != sourcePalette)
                {
                    //Debug.WriteLine($"mismatch at x:{x} y:{y} w:{width} h:{height}");
                    foundMismatch = true;
                    break;
                }
                int h = ColumnColorHeight(grid, x, y);
                if (h < height)
                {
                    //Debug.WriteLine($"too short, aborting at x:{x} y:{y} w:{width} h:{height}");
                    foundMismatch = true;
                    break;
                }
                else
                {
                    height = h;
                }
                x++;
                width++;
            }
            //Debug.WriteLine($"chunk result w:{width} h:{height}\n");
            return (width, height);
        }

        void RemoveChunkedPixelsFromGrid(int[,] grid, int x, int y, int right, int bottom)
        {
            for (int sx = x; sx < right; sx++)
            {
                for (int sy = y; sy < bottom; sy++)
                {
                    grid[sx, sy] = -1;
                    //Debug.WriteLine($"blank {sx} {sy}");
                }
            }
        }

        string CreateDrawCommandsRect(int[,] grid, string name)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            StringBuilder commands = new();

            //int x = 0;
            // int y = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    
                    int paletteNum = grid[x, y];
                    //Debug.WriteLine($"starting chunks, x:{x} y:{y} paletteNum{paletteNum}");
                    
                    if (paletteNum < 0)
                    {
                        //Debug.WriteLine($"skip -1: {x} {y}");
                        continue;
                    }
                    int alpha = palette[paletteNum].A;
                    if (alpha == 0)
                    {
                        //Debug.WriteLine($"skip 0 alpha: {x} {y}");
                    }
                    else
                    {
                        (int w, int h) chunkDim = Chunk(grid, x, y);
                        RemoveChunkedPixelsFromGrid(grid, x, y, x + chunkDim.w, y + chunkDim.h);
                        //commands.AppendLine($"x:{x} y:{y} chunk:{chunkDim}");
                        if (chunkDim.w == 1 && chunkDim.h == 1)
                        {
                            commands.AppendLine($"\t$_screen.draw_point($x+{x},$y+{y},$_c{paletteNum})");
                        }
                        else if (chunkDim.w == 1) // vertical
                        {
                            commands.AppendLine($"\t$_screen.draw_line($x+{x},$y+{y},$x+{x},$y+{y + chunkDim.h},$_c{paletteNum}) ; line vertical h{chunkDim.h}");
                        }
                        else if (chunkDim.h == 1) // vertical
                        {
                            commands.AppendLine($"\t$_screen.draw_line($x+{x},$y+{y},$x+{x + chunkDim.w},$y+{y},$_c{paletteNum}) ; line horizontal w{chunkDim.w}");
                        }
                        else
                        {
                            commands.AppendLine($"\t$_screen.draw_rect($x+{x},$y+{y},$x+{x + chunkDim.w},$y+{y + chunkDim.h},0,$_c{paletteNum}) ; w{chunkDim.w} h{chunkDim.h}");
                        }
                        //Debug.WriteLine($"end chunks {chunkDim}");
                    }
                }
            }

            return commands.ToString();
        }

        string CreateDrawCommandsRectOLD(int[,] grid, string name)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            StringBuilder commands = new();

            for (int y = 0; y < height; y++) // row
            {
                Debug.WriteLine($"\nrow y{y} ------");
                commands.AppendLine($"; --- row {y} ---");
                for (int x = 0; x < width; x++) // column
                {
                    
                    int pixelSource = grid[x, y];
                    Debug.WriteLine($"\nloop: x:{x} y:{y} col:{pixelSource}");

                    int left = x;
                    int top = y;
                    int right = left;
                    int bottom = top;
                    int pixelHorizontal = -1;
                    int pixelVertical = -1;
                    int maxHeight = height;
                    int maxWidth = width;
                    for (right = left; right < width && bottom < height && right < maxWidth; right++)
                    {
                        Debug.WriteLine($"seek l:{left} t:{top} r:{right} b:{bottom}");
                        pixelHorizontal = grid[right, top];
                        
                        if (pixelHorizontal < 0) // for later, if blanked by previous rect
                        {
                            continue;
                        }

                        if (pixelHorizontal != pixelSource || right == width-1)
                        {
                            if (pixelSource >= 0)
                            {
                                Debug.WriteLine($"result: left {left} top {top}  right {right} bottom {bottom}, color {pixelSource}/{pixelHorizontal}/{pixelVertical}");
                                commands.AppendLine($"\t$_screen.draw_rect($x+{left},$y+{top},$x+{x + right},$y+{y + bottom},0,$_c{pixelSource}) ; w{right - left} h{bottom - top}");
                            }
                            x = right;
                            maxWidth = Math.Min(right,maxWidth);

                            for (int clearX = left; clearX <= right; clearX++)
                            {
                                for (int clearY = top; clearY <= bottom; clearY++)
                                {
                                    grid[clearX, clearY] = -1;
                                }
                            }

                            //break; // test
                        }
                        else
                        {
                            bool foundMismatch = false;
                            for (bottom = top; bottom < height && bottom < maxHeight; bottom++)
                            {
                                //Debug.WriteLine($">>> l:{left} t:{top} r:{right} b:{bottom}   {pixelVertical} != {pixelSource}");
                                pixelVertical = grid[right, bottom];
                                if (pixelVertical != pixelSource || bottom == height - 1)
                                {
                                    foundMismatch = true;
                                    maxHeight = Math.Min(bottom,maxHeight);
                                    //maxWidth = right;
                                    Debug.WriteLine($"mismatch at l:{left} t:{top} r:{right} b:{bottom}   {pixelVertical} != {pixelSource}");
                                    break;
                                    //break;
                                }
                            }   
                            if (!foundMismatch)
                            {
                                continue;
                            }
                        }
                        
                    }
                }
            }

            return commands.ToString();
        }

        string CreateDrawCommandsHorizontal(int[,] grid, string name)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            StringBuilder commands = new();
            
            for (int y = 0; y < height; y++)
            {
                commands.AppendLine($"; --- row {y} ---");
                for (int x = 0; x < width;)
                {
                    int pNow = grid[x, y];

                    // check for contiguous line
                    int chunkLength = 1;

                    for (int l = x+1; l < width; l++)
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
                        commands.AppendLine($"\t$_screen.draw_line($x+{x},$y+{y}, $x+{x+chunkLength},$y+{y}, $_c{pNow}) ; line {chunkLength} ");
                        x += chunkLength;
                    }
                    
                }
            }
            return commands.ToString();
        }

        string CreateDrawCommandsVertical(int[,] grid, string name)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            StringBuilder commands = new();

            for (int x = 0; x < width; x++)
            {
                commands.AppendLine($"; --- col {x} ---");
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
                        commands.AppendLine($"\t$_screen.draw_line($x+{x},$y+{y}, $x+{x},$y+{y+chunkHeight}, $_c{pNow}) ; line {chunkHeight} ");
                        y += chunkHeight;
                    }

                }
            }
            return commands.ToString();
        }

        public static Bitmap CopyImage(Bitmap img)
        {
            System.Drawing.Rectangle cropArea = new(0, 0, img.Width, img.Height);
            //https://www.codingdefined.com/2015/04/solved-bitmapclone-out-of-memory.html
            Bitmap bmp = new(cropArea.Width, cropArea.Height);

            using (Graphics gph = Graphics.FromImage(bmp))
            {
                gph.DrawImage(img, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), cropArea, GraphicsUnit.Pixel);
            }
            return bmp;
        }
    }
}