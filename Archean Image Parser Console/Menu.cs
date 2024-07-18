using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParseLib;

namespace Archean_Image_Parser_Console
{
    internal class Menu
    {
        Parser parser = new();
        string imageFileName = "";
        string? resultCommands = null;
        string? outputFile = null;
        int BrightnessRed = 60;
        int BrightnessGreen = 60;
        int BrightnessBlue = 60;
        Parser.ProcessingMode processingMode = Parser.ProcessingMode.rect;
        
        internal Menu()
        {

        }

        private static void ExitProgram(int ExitCode)
        {
            Environment.Exit(ExitCode);
        }

        private string SelectImageFile()
        {
            Console.Write("Enter image file name:");
            string? filename = Console.ReadLine();
            if (filename == null || filename.Length == 0)
            {
                Console.WriteLine("No file name set. Enter Q or Ctrl+C if you want to exit.");
            }
            else if (filename.ToLower() == "q")
            {
                ExitProgram((int)Parser.ErrorCodes.Quit);
            }
            else if (File.Exists(filename))
            {
                return filename;
            }
            else
            {
                Console.WriteLine($"File '{filename}' does not exist.");
            }
            return "";
        }

        private bool LoadImage(string filename)
        {
            if (parser.LoadImage(imageFileName))
            {
                Console.WriteLine();
                Console.WriteLine("File loaded");
                Console.WriteLine();
                return true;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("File not found or invalid.");
                return false;
            }
        }

        private void SelectBrightness()
        {
            Console.WriteLine("Set color channel brightness, 1-200, or press Enter to use recommend value 60");
            Console.Write("Red: ");
            if (int.TryParse(Console.ReadLine(), out int brightnessRed) == false)
            {
                Console.WriteLine("No number entered, using value 60");
                BrightnessRed = 60;
            }
            Console.Write("Green: ");
            if (int.TryParse(Console.ReadLine(), out int brightnessGreen) == false)
            {
                Console.WriteLine("No number entered, using value 60");
                BrightnessGreen = 60;
            }
            Console.Write("Blue: ");
            if (int.TryParse(Console.ReadLine(), out int brightnessBlue) == false)
            {
                Console.WriteLine("No number entered, using value 60");
                BrightnessBlue = 60;
            }
        }

        private void SelectProcessingMode()
        {
            Console.WriteLine("Select processing mode, or press enter/other key for Rectangle mode (recommended)");
            Console.Write("Rectangle, Horizontal or Vertical (R/H/V): ");
            ConsoleKey mode = Console.ReadKey().Key;
            Console.WriteLine();
            if (mode == ConsoleKey.H)
            {
                processingMode = Parser.ProcessingMode.horizontal;
            }
            else if (mode == ConsoleKey.V)
            {
                processingMode = Parser.ProcessingMode.vertical;
            }
            else
            {
                processingMode= Parser.ProcessingMode.rect;
            }
        }

        private void ProcessImage()
        {
            Console.WriteLine();
            Console.WriteLine("Processing image...");
            resultCommands = parser.ProcessImage(processingMode, BrightnessRed, BrightnessGreen, BrightnessBlue);
            if (resultCommands == null)
            {
                Console.WriteLine("Error processing image");
            }
        }

        private bool SelectOutputFile()
        {
            Console.WriteLine();
            Console.Write("File name: ");
            string? outfile = Console.ReadLine();
            if (outfile == null)
            {
                Console.WriteLine("Name entry cancelled, exiting");
            }
            else if (outfile.ToLower() == "q")
            {
                ExitProgram((int)Parser.ErrorCodes.Quit);
            }
            else if (outfile.Length == 0)
            {
                Console.WriteLine("No name entered, try again or type Q to quit.");
            }
            else
            {
                outputFile = outfile;
                return true;
            }
            return false;
        }

        private bool OutputToFile()
        {
            while (SelectOutputFile() == false);

            if (outputFile == null)
            {
                Console.WriteLine("Output file variable is null"); // shouldn't be reachable
                return false;
            }
            if (resultCommands == null)
            {
                Console.WriteLine("Command variable is null"); // shouldn't be reachable
                return false;
            }

            try
            {
                File.WriteAllText(outputFile, resultCommands);
                Console.WriteLine("File saved");
                return true;
            }
            catch
            {
                Console.WriteLine($"Error: could not save {outputFile}");
                return false;
            }  
        }

        private void OutputToConsole()
        {
            Console.WriteLine();
            Console.WriteLine(resultCommands);
            Console.WriteLine();
        }

        private bool SelectOutput()
        {
            Console.Write("File or Console (F/C): ");
            ConsoleKey mode = Console.ReadKey().Key;
            Console.WriteLine();
            if (mode == ConsoleKey.F)
            {
                OutputToFile();
                return true;
            }
            else if (mode == ConsoleKey.C)
            {
                OutputToConsole();
                return true;
            }
            else if (mode == ConsoleKey.Q)
            {
                ExitProgram((int)Parser.ErrorCodes.Quit);
            }
            else
            {
                Console.WriteLine("To exit, press Q");
            }
            return false;
        }

        internal bool MenuLoop()
        {
            //Console.Clear();
            while (imageFileName.Length == 0)
            {
                imageFileName = SelectImageFile();
            }
            if (LoadImage(imageFileName))
            {
                SelectProcessingMode();
                SelectBrightness();
                ProcessImage();
                Console.WriteLine();
                Console.WriteLine("Image processed. Select output");
                while (SelectOutput() == false);
                Console.WriteLine();
                ExitProgram((int)Parser.ErrorCodes.OK);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Try another file or type Q to quit.");
                imageFileName = "";
                Console.WriteLine();
            }
            
            return true;
        }
    }
}

