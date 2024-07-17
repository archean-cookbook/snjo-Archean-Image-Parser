using ParseLib;
Parser parser = new();

// See https://aka.ms/new-console-template for more information
Console.Write("Enter image file name:");
string? filename = Console.ReadLine();

if (filename == null)
{
    Console.WriteLine("Name entry cancelled, exiting");
}
else
{
    if (parser.LoadImage(filename))
    {
        Console.WriteLine("File loaded");
        Console.WriteLine("Set color channel brightness, 1-200 (recommend 60)");
        Console.Write("Red: ");
        if (int.TryParse(Console.ReadLine(), out int brightnessRed) == false)
        {
            Console.WriteLine("Invalid entry, using value 60");
            brightnessRed = 60;
        }
        Console.Write("Green: ");
        if (int.TryParse(Console.ReadLine(), out int brightnessGreen) == false)
        {
            Console.WriteLine("Invalid entry, using value 60");
            brightnessGreen = 60;
        }
        Console.WriteLine("Blue: ");
        if (int.TryParse(Console.ReadLine(), out int brightnessBlue) == false)
        {
            Console.WriteLine("Invalid entry, using value 60");
            brightnessBlue = 60;
        }
        Console.WriteLine("Processing image...");
        string? result = parser.ProcessImage(Parser.ProcessingMode.rect, brightnessRed, brightnessGreen, brightnessBlue);
        if (result == null)
        {
            Console.WriteLine("Error processing image");
        }
        else
        {
            Console.WriteLine("Image processed. Select output");
            Console.Write("File or Console (F/C): ");
            ConsoleKey mode = Console.ReadKey().Key;
            if (mode == ConsoleKey.F)
            {
                Console.WriteLine();
                Console.Write("File name: ");
                string? outfile = Console.ReadLine();
                if (outfile == null)
                {
                    Console.WriteLine("Name entry cancelled, exiting");
                }
                else
                {
                    //if (Directory.Exists(Path.GetDirectoryName(filename)))
                    //{
                    try
                    {
                        File.WriteAllText(outfile, result);
                        Console.WriteLine("File saved");
                    }
                    catch
                    { 
                        Console.WriteLine($"Error: could not save {outfile}");
                        Console.WriteLine($"Assumed Directory {Path.GetDirectoryName(filename)}");
                    }
                        
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Directory doesn't exist");
                    //}
                }
            }
            else if (mode == ConsoleKey.C)
            {
                Console.WriteLine();
                Console.WriteLine(result);
                Console.WriteLine();
            }

        }
    }
    else
    {
        Console.WriteLine("File not found or invalid, Restart to retry");
    }

}
