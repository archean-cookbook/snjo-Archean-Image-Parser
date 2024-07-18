// See https://aka.ms/new-console-template for more information

using Archean_Image_Parser_Console;

if (args.Length > 0)
{
    ProcessArguments processArguments = new ProcessArguments(args);
}
else
{
    Menu menu = new Menu();
    Console.WriteLine("Starting in menu mode. For automatic mode, use arguments (try -h or help)");
    Console.WriteLine();
    while(menu.MenuLoop());
}