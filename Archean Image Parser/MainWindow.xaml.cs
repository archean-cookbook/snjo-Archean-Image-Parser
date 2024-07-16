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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Archean_Image_Parser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bitmap? bitmap = null;
        string loadFileName = "";
        List<System.Drawing.Color> palette = [];
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonLoadImage_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Open load image");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool? result = openFileDialog.ShowDialog();
            if (result != null && result == true)
            {
                loadFileName = openFileDialog.FileName;
                Debug.WriteLine($"Load file: {loadFileName}");
                //Uri uri = new Uri(loadFileName);
                //bitmap = new BitmapImage(uri);
                bitmap = new Bitmap(loadFileName);
                SourceImageView.Source = CreateBitmapSourceFromGdiBitmap(bitmap);
                SourceImageView.Width = bitmap.Width;
                SourceImageView.Height = bitmap.Height;
            }
        }

        private void ButtonProcessImageH_Click(object sender, RoutedEventArgs e)
        {
            ProcessImage(true);
        }

        private void ButtonProcessImageV_Click(object sender, RoutedEventArgs e)
        {
            ProcessImage(false);
        }

        private void ProcessImage(bool horizontal)
        {
            string commands = string.Empty;
            int[,]? pixelGrid;
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
                commands = CreateCommands(pixelGrid, System.IO.Path.GetFileNameWithoutExtension(loadFileName),horizontal);
                // PrintPalette();
                
                // Debug.WriteLine(commands);
                TextBoxCommands.Text = commands;
            }
        }

        void PrintPixelGrid(int[,] grid)
        {
            Debug.WriteLine("---");
            Debug.WriteLine(MakePixelGrid(grid));
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
                    // Debug.WriteLine($"x{x} y{y}");
                    if (palette.Count > 10)
                    {
                        result.Append((grid[x, y]).ToString().PadLeft(4));
                    }
                    else
                    {
                        result.Append(grid[x, y]);
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
            return $"color({pColor.R},{pColor.G},{pColor.B},{pColor.A})";
        }

        string CreateCommands(int[,] grid, string name,bool horizontal)
        {
            StringBuilder commands = new();
            commands.AppendLine($"function @sprite_{name}($_screen:screen,$x:number,$y:number)");
            for (int p = 0; p < palette.Count; p++)
            {
                string archColor = PaletteToColorFunction(p);
                commands.AppendLine($"\tvar $_c{p} = {archColor}");
            }

            if (horizontal)
            {
                commands.AppendLine(CreateDrawCommandsHorizontal(grid, name));
            }
            else
            {
                commands.AppendLine(CreateDrawCommandsVertical(grid, name));
            }
            //commands.AppendLine(CreateDrawCommandsRect(grid, name));

            return commands.ToString();
        }

        string CreateDrawCommandsRect(int[,] grid, string name)
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
                        Debug.WriteLine($"l:{l} x:{x} y:{y} w:{width} h:{height}");
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
    }
}