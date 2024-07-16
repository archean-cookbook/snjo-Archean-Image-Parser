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

## Horizontal and Vertical processing
These modes only output straight lines and dots, no rectangles.
This is useful if you're creating a function with stretching capabilities, such as making a narrow sliver of a UI element that can be stretched to fill the bounds.

    Example, change this:

    function @sprite_buttonMiddle($_screen:screen,$x:number,$y:number)
	    var $_c0 = color(21,37,87,255)
    	var $_c1 = color(77,126,144,255)
    	var $_c2 = color(51,84,96,255)
    	$_screen.draw_line($x+0,$y+0, $x+2,$y+0, $_c0) ; line 2 
    	$_screen.draw_line($x+0,$y+1, $x+2,$y+1, $_c1) ; line 2 
    	$_screen.draw_line($x+0,$y+2, $x+2,$y+2, $_c2) ; line 2 

    To this:
    
    function @sprite_buttonMiddle($_screen:screen,$x:number,$y:number,$width:number) ; <<< add width argument
	    var $_c0 = color(21,37,87,255)
    	var $_c1 = color(77,126,144,255)
    	var $_c2 = color(51,84,96,255)
    	$_screen.draw_line($x+0,$y+0, $x+2+$width,$y+0, $_c0) ; <<< add $width to the second X variable to have the sprite stretch out to any size
    	$_screen.draw_line($x+0,$y+1, $x+2+$width,$y+1, $_c1)
    	$_screen.draw_line($x+0,$y+2, $x+2+$width,$y+2, $_c2)
        

## Pixel grid

The left text box outputs all the pixels in the image as numbers in a grid. Use this data if you're writing your own image processing code and just want the pixel data from an image.
Spaces are transparent values.
