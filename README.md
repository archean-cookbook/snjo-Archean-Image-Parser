Requires .net 8

Parses an image and outputs a xenoncode draw function for Archean computers.
The image will be processed into a grid where each number is a color value from a generated palette.
This grid is then used to simplify the draw calls to lines and rectangles if there's a contiguous set of pixels with the same palette color.

If you have fewer unique colors in your image, fewer color palette variables are created, and there will be more opportunities to create simple shapes out of a chunk of pixels.
When converting an existing image like a photo with a wide range of colors, try converting it to an indexed color image to simplify it.

The finished function draws the points, lines and rects to a screen. If you use one hidden screen as your sprite storage, you can then copy that sprite region to your main screen on demand.
That way you only have to use the heavy generated function once for each image, and use the sprite library screen -> main screen copy when you need to draw it.
