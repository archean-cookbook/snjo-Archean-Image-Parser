// See https://aka.ms/new-console-template for more information

using Archean_Image_Parser_Console;

if (args.Length > 0)
{
    ProcessArguments processArguments = new ProcessArguments(args);
}
else
{
    Menu menu = new Menu();
    while(menu.MenuLoop());
}