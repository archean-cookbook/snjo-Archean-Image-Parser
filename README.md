Requires .net 8

Parses an image and outputs a xenoncode draw function for Archean computers.
The image will be processed into a grid where each number is a color value from a generated palette.
If you have fewer colors in your image, fewer color palette variables are created.

The finished function draws the points to a screen. If you use one hidden screen as your sprite storage, you can then copy that sprite region to your main screen on demand.
That way you only have to use the heavy generated function once for each image, and use the sprite library screen -> main screen copy when you need to draw it.
