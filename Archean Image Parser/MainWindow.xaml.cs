using ParseLib;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace Archean_Image_Parser
{
    public partial class MainWindow : Window
    {
        Parser parser = new();
        string? commandsOut = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonLoadImage_Click(object sender, RoutedEventArgs e)
        {
            // Load an image file via dialog
            Debug.WriteLine("Open load image");
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Image Files(*.PNG;*.BMP;*.JPG;*.GIF)|*.PNG;*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"
            };
            bool? result = openFileDialog.ShowDialog();


            if (result != null && result == true)
            {


                if (parser.LoadImage(openFileDialog.FileName))
                {

                    BitmapImage thumbnail = new BitmapImage();
                    thumbnail.BeginInit();
                    thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                    thumbnail.UriSource = new Uri(openFileDialog.FileName);
                    thumbnail.EndInit();
                    SourceImageView.Source = thumbnail;
                }
            }
        }

        private void ProcessHorizontal_Click(object sender, RoutedEventArgs e)
        {
            commandsOut = parser.ProcessImage(Parser.ProcessingMode.horizontal, GetBrightness("Red"), GetBrightness("Green"), GetBrightness("Blue"));
            TextBoxCommands.Text = commandsOut;
        }

        private void ProcessVertical_Click(object sender, RoutedEventArgs e)
        {
            commandsOut = parser.ProcessImage(Parser.ProcessingMode.vertical, GetBrightness("Red"), GetBrightness("Green"), GetBrightness("Blue"));
            TextBoxCommands.Text = commandsOut;
        }

        private void ProcessRect_Click(object sender, RoutedEventArgs e)
        {
            commandsOut = parser.ProcessImage(Parser.ProcessingMode.rect, GetBrightness("Red"), GetBrightness("Green"), GetBrightness("Blue"));
            TextBoxCommands.Text = commandsOut;
        }
        
        private void ProcessRanked_Click(object sender, RoutedEventArgs e)
        {
            commandsOut = parser.ProcessImage(Parser.ProcessingMode.ranked, GetBrightness("Red"), GetBrightness("Green"), GetBrightness("Blue"));
            TextBoxCommands.Text = commandsOut;
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new()
            {
                Filter = "Xenon files (*.xc)|*.xc|Text files *.txt)|*.txt|All files (*.*)|*.*"
            };
            bool? result = saveDialog.ShowDialog();
            if (result == null)
            {
                Debug.WriteLine("save dialog result null");
                return;
            }    
            if(result == true)
            {
                Debug.WriteLine("save dialog OK");
                string filename = saveDialog.FileName;
                try
                {
                    File.WriteAllText(filename, commandsOut);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving to file:\r\n\r\n"+ex.ToString());
                }
            }
            else
            {
                Debug.WriteLine("save dialog cancelled");
            }
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (commandsOut != null && commandsOut.Length > 0)
            {
                try
                {
                    Clipboard.SetText(commandsOut);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error writing to clipboard:\r\n" + ex.ToString());
                }
            }
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            ArgumentNullException.ThrowIfNull(bitmap);

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

        int GetBrightness(string color)
        {
            // fetch brightness textbox values from UI
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
                bright = Math.Clamp(bright, 1, 200);
                Debug.WriteLine("Brightness set to " + bright);
            }
            else
            {
                bright = 100;
                Debug.WriteLine("Brightness set to default " + bright);
            }
            return bright;
        }
    }
}