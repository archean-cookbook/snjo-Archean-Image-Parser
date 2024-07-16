## Archean image parser
Parses an image and outputs a xenoncode draw function for Archean computers.

## Use
- Load an image. I recommend a small image with areas of identical color if you want the best performance out of the function in game.
- Click Process as Rectangles (suitable for most uses)
- Copy the function from the textbox to the right into an archean computer
- Call on the generated function in your normal program.
- If the image is too bright or too dark, use the Brightness values in the program and generate the code again to update the color palette. By default the values are darker than the source image to combat overly bright colors on Archean computers

## Details
The image will be processed into a grid where each number is a color value from a generated palette.
This grid is then used to simplify the draw calls to lines and rectangles if there's a contiguous set of pixels with the same palette color.

If you have fewer unique colors in your image, fewer color palette variables are created, and there will be more opportunities to create simple shapes out of a chunk of pixels.
When converting an existing image like a photo with a wide range of colors, try converting it to an indexed color image to simplify it.

The finished function draws the points, lines and rects to a screen. If you use one hidden screen as your sprite storage, you can then copy that sprite region to your main screen on demand.
That way you only have to use the heavy generated function once for each image, and use the sprite library screen -> main screen copy when you need to draw it.

## Pixel grid

The left text box outputs all the pixels in the image as numbers in a grid. Use this data if you're writing your own image processing code and just want the pixel data from an image.
Spaces are transparent values.
