using ParseLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archean_Image_Parser_Console
{
    internal class ProcessArguments
    {
        string imageFileName = "";
        string outputFile = "";
        int[] Brightness = {60,60,60};
        Parser.ProcessingMode processingMode = Parser.ProcessingMode.rect;

        enum ColorChannel
        {
            Red,
            Green,
            Blue,
            Alpha
        }

        public ProcessArguments(string[] args)
        {
            Parser parser = new Parser();
            
            if (args.Length == 1)
            {
                if (args[0] == "h" || args[0] == "-h" || args[0] == "--h" || args[0] == "/?" || args[0] == "/h" || args[0] == "help")
                {
                    HelpInfo();
                    Environment.Exit((int)Parser.ErrorCodes.Quit);
                }
                else
                {
                    Console.WriteLine("Too few arguments");
                    Helphint();
                }
                Environment.Exit((int)Parser.ErrorCodes.TooFewArguments);
            }

            imageFileName = args[0];
            outputFile = args[1];

            if (args.Length == 2)
            {
                // just input and output
            }
            else if (args.Length == 3)
            {
                string pMode = args[2];

                SetProcessingMode(pMode);
            }
            else if (args.Length >= 5)
            {

                for (int i = 0; i <= 2; i++)
                {
                    //Console.WriteLine($"Args length {args.Length}, i:{i}, brightness");
                    if (int.TryParse(args[i + 2], out var value))
                    {
                        Brightness[i] = value;
                        Console.WriteLine($"Brightness {(ColorChannel)i} set to {value}");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid brightness value in argument {i + 1} ({(ColorChannel)i}), expected integer, got {args[i + 2]}");
                        Environment.Exit((int)Parser.ErrorCodes.InvalidArguments);
                        Helphint();
                    }
                }

                if (args.Length >= 6)
                {
                    string pMode = args[5];

                    SetProcessingMode(pMode);
                }
            }
            else
            {
                Console.WriteLine("Wrong number of arguments, expecting one of these:");
                Console.WriteLine("2: image output");
                Console.WriteLine("3: image output mode");
                Console.WriteLine("5: image output R G B");
                Console.WriteLine("6: image output R G B mode");
                Helphint();

                Environment.Exit((int)Parser.ErrorCodes.InvalidArguments);
            }




            if (File.Exists(imageFileName) == false)
            {
                Console.WriteLine($"File not found: {imageFileName}");
                Environment.Exit((int)Parser.ErrorCodes.FileNotFound);
            }

            if (parser.LoadImage(imageFileName))
            {
                Console.WriteLine($"File loaded: {imageFileName}");
            }
            else
            {
                Console.WriteLine($"File not found or invalid: {imageFileName}");
                Helphint();
                Environment.Exit((int)Parser.ErrorCodes.FileError);
            }

            string? resultCommands = parser.ProcessImage(processingMode, Brightness[0], Brightness[1], Brightness[2]);
            
            
            if (resultCommands != null)
            {
                if (outputFile == "=")
                {
                    outputFile = Path.GetFileNameWithoutExtension(imageFileName) + ".xc";
                    Console.WriteLine($"Setting output file name to {outputFile}");
                }

                if (outputFile.ToLower() == "console")
                {
                    OutputToConsole(resultCommands);
                    Environment.Exit((int)Parser.ErrorCodes.OK);
                }
                if (outputFile == "=")
                {
                    outputFile = Path.GetFileNameWithoutExtension(imageFileName) + ".xc";
                    Console.WriteLine($"Setting output file name to {outputFile}");
                }
                else
                {
                    OutputToFile(outputFile, resultCommands);
                    Environment.Exit((int)Parser.ErrorCodes.OK);
                }
            }
            else
            {
                Console.WriteLine("Error processing image.");
                Helphint();
                Environment.Exit((int)Parser.ErrorCodes.ProcessingError);
            }
        }

        private static void Helphint()
        {
            Console.WriteLine("For help use argument -h or help");
        }

        private void SetProcessingMode(string pMode)
        {
            if (pMode.ToLower() == "horizontal" || pMode.ToLower() == "h")
            {
                processingMode = Parser.ProcessingMode.horizontal;
            }
            else if (pMode.ToLower() == "vertical" || pMode.ToLower() == "v")
            {
                processingMode = Parser.ProcessingMode.vertical;
            }
            else if (pMode.ToLower() == "grid" || pMode.ToLower() == "g")
            {
                processingMode = Parser.ProcessingMode.grid;
            }
            else
            {
                processingMode = Parser.ProcessingMode.rect;
            }
            Console.WriteLine($"Processing mode argument: {processingMode.ToString()}");
        }

        private void OutputToFile(string outputFilename, string resultCommands)
        {
            try
            {
                File.WriteAllText(outputFile, resultCommands);
                Console.WriteLine($"File saved: {outputFile}");
            }
            catch
            {
                Console.WriteLine($"Error: could not save {outputFile}");
            }
        }

        private void OutputToConsole(string resultCommands)
        {
            Console.WriteLine();
            Console.WriteLine(resultCommands);
            Console.WriteLine();
        }

        private void HelpInfo()
        {
            ConsoleColor prevColor = Console.ForegroundColor;
            ConsoleColor normal = ConsoleColor.Gray;
            ConsoleColor heading = ConsoleColor.Yellow;
            ConsoleColor highlight = ConsoleColor.Cyan;


            Console.WriteLine();
            Console.ForegroundColor = heading;
            Console.WriteLine("Archean image parser by Andreas Aakvik Gogstad (snjo)");
            Console.ForegroundColor = normal;
            Console.WriteLine("https://github.com/archean-cookbook/snjo-Archean-Image-Parser");
            Console.WriteLine("Processes an image file into a function with XenonCode draw commands for the game Archean");
            Console.WriteLine();
            Console.WriteLine("To use menu based mode, start the program with no arguments.");
            Console.WriteLine();
            Console.ForegroundColor = heading;
            Console.WriteLine("Command line arguments:");
            Console.ForegroundColor = highlight;
            Console.WriteLine("ArcheanImageParser imagefile outputfile [mode]");
            Console.WriteLine("ArcheanImageParser imagefile outputfile [red green blue] [mode]");
            Console.ForegroundColor = normal;
            Console.WriteLine("The output file argument can be replaced by = to use the same name as imagefile + .xc");
            Console.WriteLine();
            Console.ForegroundColor = heading;
            Console.WriteLine("Examples:");
            Console.ForegroundColor = highlight;
            Console.WriteLine("ArcheanImageParser test.png out.xc");
            Console.WriteLine("ArcheanImageParser test.png =");
            Console.WriteLine("ArcheanImageParser test.png out.xc v");
            Console.WriteLine("ArcheanImageParser test.png out.xc 80 60 60 horizontal");
            Console.ForegroundColor = normal;
            Console.WriteLine();
            Console.WriteLine("If colors are omitted, the default value of 60 is used for all channels (60% brightness)");
            Console.WriteLine();
            Console.ForegroundColor = heading;
            Console.WriteLine("Modes:");
            Console.ForegroundColor = highlight;
            Console.Write("rect        ");
            Console.ForegroundColor = normal;
            Console.WriteLine("Rectangle mode, outputting an efficient mix of shapes for the shortest code");
            Console.ForegroundColor = highlight;
            Console.Write("horizontal  ");
            Console.ForegroundColor = normal;
            Console.WriteLine("(or h) Outputs only horizontal lines and points");
            Console.ForegroundColor = highlight;
            Console.Write("vertical    ");
            Console.ForegroundColor = normal;
            Console.WriteLine("(or v) Outputs only vertical lines and points");
            Console.ForegroundColor = highlight;
            Console.Write("grid        ");
            Console.ForegroundColor = normal;
            Console.WriteLine("Outputs the grid of pixels found in the image, numbers represent palette swatches.");
            Console.WriteLine("            Does not output a draw function. This is used for building alternative algorithms.");
            Console.WriteLine();
            Console.ForegroundColor = heading;
            Console.WriteLine("Available image formats");
            Console.ForegroundColor = normal;
            Console.WriteLine("PNG, BMP, JPEG, GIF, TIFF, TGA, PBM, QOI, WEBP");
            Console.WriteLine();
            Console.ForegroundColor = heading;
            Console.WriteLine("Error codes:");
            Console.ForegroundColor = normal;
            Console.WriteLine("OK = 0, Quit = 1, FileNotFound = 2, FileError = 3, ImageError = 4");
            Console.WriteLine("ProcessingError = 5, OutputFileError = 6, TooFewArguments = 7, InvalidArguments = 8");
            Console.WriteLine();
            Console.ForegroundColor = prevColor;
        }
    }
}
