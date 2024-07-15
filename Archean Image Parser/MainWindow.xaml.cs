using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
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

        private void ButtonProcessImage_Click(object sender, RoutedEventArgs e)
        {
            string commands = string.Empty;
            Debug.WriteLine("Process image");
            if (bitmap != null)
            {
                int[,] pixelGrid = new int[bitmap.Width,bitmap.Height];
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
                    
                    commands = (CreateDrawCommands(pixelGrid,System.IO.Path.GetFileNameWithoutExtension(loadFileName)));
                }
                // PrintPalette();
                TextBoxGrid.Text = MakePixelGrid(pixelGrid);
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
            return $"color({pColor.A},{pColor.R},{pColor.G},{pColor.B},)";
        }

        string CreateDrawCommands(int[,] grid, string name)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            StringBuilder commands = new();
            commands.AppendLine($"function @sprite_{name}($_screen:screen,$x:number,$y:number)");
            for (int p = 0; p < palette.Count; p++)
            {
                string archColor = PaletteToColorFunction(p);
                commands.AppendLine($"\tvar $_c{p} = {archColor}");
            }
            for (int y = 0; y < height; y++)
            {
                commands.AppendLine($"--- row {y} ---");
                for (int x = 0; x < width;)
                {
                    int pNow = grid[x, y];

                    // check for contiguous line
                    int chunkLength = 1;
                    int chunkHeight = 1;
                    bool growX = true;
                    bool growY = true;
                    //l = x + 1
                    //while (growX || growY)
                    for (int l = x+1; l < width; l++)
                    {
                        int pNext = grid[l, y];

                        if (pNext == pNow)
                        {
                            chunkLength++;
                        }
                        else // debug
                        {
                            if (y < 2) // debug
                                Debug.WriteLine($"line ends");
                            break;
                        }

                        if (y < 2) // debug
                        {
                            Debug.WriteLine($"l{l} x{x} y{y} pnow{pNow} pnext{pNext} length{chunkLength}");
                        }
                    }

                    if (chunkLength < 2)
                    {
                        commands.AppendLine($"\t$_screen.draw_point($x+{x},$y+{y},$_c{pNow})");
                        x++;
                    }
                    else
                    {
                        commands.AppendLine($"\t$_screen.draw_line($x+{x},$y+{y}, $x+{x+chunkLength},$y+{y} $_c{pNow}) ; line {chunkLength} ");
                        x += chunkLength;
                    }
                    
                }
            }
            return commands.ToString();
        }
    }
}