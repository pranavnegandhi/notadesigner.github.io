---
title: "Breaking Free from Your API"
date: 2009-06-05T05:35:13Z
slug: breaking-free-from-your-api
aliases: ["/breaking-free-from-your-api/"]
categories:
  - "Demonstration"
tags:
  - "actionscript"
wp_post_id: 44
---

I have been noticing off-late that as a UI developer, I dabble less in what can be considered programming and more often in HTML and CSS and making them work across the bazillion browsers out there. The closest I get to what can be considered writing lines of code is where the interaction requires a few lines of JavaScript to transition a control in or out of the screen or some Ajax operation to send data across the wire or update the view. And even such programming is greatly abstracted away, thanks to frameworks such as jQuery. With the browser taking care of everything, from drawing widgets, to layout, to handling user interaction, there is little left to do other than send the page layout to the browser. Sadly, that is the lot of most UI developers who have been reduced to pixel-pushers in this advent of web-based applications.

Even the folks who develop RIAs on platforms such as Flash and Silverlight do not feel the need to get into performance oriented code beyond optimizing network requests.

Don’t worry about widgets. Don’t fiddle with the whatchamacallit. Don’t! Don’t! It is all done! Just fetch me another recordset, sonny. And make it snappy!

And though this kind of work does pay the bills, every once in a while someone runs into a problem that cannot be addressed through an API call. When that happens, the first reaction of the average development team is to clutch at their hair and run around in circles screaming “Omigod! Omigod! Omigod!” And though someone on the team usually manages to whack something out over a weekend, it often isn’t the best solution because nobody has done this before! Whereas programmers of yore that I worked with never flinched at getting their hands dirty with low-level stuff, even up to writing custom extensions to their platform in a language like C, many of today’s programmers work on enough abstractions to make the underlying hardware and operating system entirely irrelevant. To them, the browser or virtual machine is the machine.

### Where Does One Draw the Line?

Let me lead you through a programming exercise using ActionScript that demonstrates the joy of extending missing API calls. We are not inventing new algorithms here, but implementing a simple, well-defined solution for drawing lines on raster displays using the slope-intercept formula. Then we are coming up with a better implementation, using a better algorithm, trading performance and accuracy to reach a balanced result in the end.

{{< flash src="line.swf" width="400" height="400" >}}

### The Shape of a Line

For those who do not remember high-school geometry and algebra, a line can be specified by providing its start and end coordinates, using the slope-intercept formula to determine the path between these two points. This information used to be enough to display lines on vector displays of yore, which could draw all sorts of mathematically defined shapes. But with the advent of raster displays which only allow discrete pixels to be turned on, lines and other shapes are now displayed by calculating the pixels which approximate the ideal line as closely as possible.

The slope intercept formula goes as follows –

y = (y1 - y0) / (x1 - x0) \* x + b

This is further condensed into –

y = m \* x + b

Here m is the slope of the line, which is the ratio of rise against the run. The value b represents the y-intercept, which is the location where the line crosses the origin on the y-axis. So to compute the y-coordinate on any given x-coordinate on that line, you multiply the slope of the line with the x-coordinate and add the y-intercept to the result. To extend this into drawing lines, you would run a loop incrementing a value between the start and end x-coordinates and computing the y-coordinate with each iteration.

Here is an implementation.

```actionscript
public static function lineSimple(bmdCanvas:BitmapData, x0:int, y0:int, x1:int, y1:int, lColor:uint)
{
	/**
	 * Naive implementation of slope-intercept formula
	 */

	var m:Number;
	var b:Number;
	var dx:Number, dy:Number;

	dx = x1 - x0;
	dy = y1 - y0;

	if (dx != 0)
	{
		m = dy / dx;
		b = y0 - m * x0;
		dx = (x1 > x0) ? 1 : -1;

		while (x0 != x1)
		{
			x0 += dx;
			y0 = m * x0 + b;

			bmdCanvas.setPixel32(x0, y0, lColor);
		}
	}
}
```

This is a very straight-forward implementation and leaves a lot to be desired. But let us take it line by line to understand it.

The first 2 lines calculate the difference between the start and end points in both axes. Then it divides dy by dx after checking for a potential divide by zero error and then calculates the slope and the y-intercept of the line. It also changes the value of dx to 1 if the x-coordinate of the second point is greater than the x-coordinate of the first point, or -1 if it is less than.

We now enter the loop which continues until x0 becomes greater than or equal to x1. Inside this loop it increments the value of x0 by dx (1 or -1) and calculates the value of y0 using the slope-intercept formula. It then places a pixel at the newly calculated coordinate and continues the loop.

This is a very simple implementation and just barely does the job. In fact, it does not even cover all possible cases that a line-drawing function might run into. For example, this will not work correctly if the slope of the line is greater than 1 (in other words, if the line is taller than it is wider). As the loop iterates over values in the x-axis, it plots exactly one pixel in every column while possibly plotting more than one pixel in the same row. But when the line becomes steep, there are fewer columns than rows. Since the algorithm iterates over the x-axis and computes the position in the y-axis, the distance between consecutive pixels on the y-axis increases as the line becomes steeper and makes for a discontinuous line.

### Resolution for Slopes Greater than 1 or Negative Slopes

To resolve this issue, with come up with a second iteration of our line drawing function. This function first calculates the slope of the line and if it is less than 1 then it continues using the previously demonstrated algorithm. But if the slope is greater than 1 or less than -1 then it increments the value of the y-axis in the loop and computes the position in the x-axis. The complete implementation is shown below.

```actionscript
public static function lineImproved(bmdCanvas:BitmapData, x0:int, y0:int, x1:int, y1:int, lColor:uint)
{
	/**
	 * Advanced implementation of slope-intercept formula
	 * to handle slopes greater than 1 or less than -1
	 */

	var m:Number;
	var b:Number;
	var dx:Number, dy:Number;

	dx = x1 - x0;
	dy = y1 - y0;

	if (Math.abs(dx) > Math.abs(dy))
	{
		/**
		 * Slope is < 1 or > -1
		 * Increment horizontally
		 */

		m = dy / dx;
		b = y0 - m * x0;
		dx = (x1 > x0) ? 1 : -1;

		while (x0 != x1)
		{
			x0 += dx;
			y0 = m * x0 + b;

			bmdCanvas.setPixel32(x0, y0, lColor);
		}
	}
	else if (dy != 0)
	{
		/*
		 * Slope is > 1 or < -1
		 * Increment vertically
		 */

		m = dx / dy;
		b = x0 - m * y0;
		dy = (dy < 0) ? -1 : 1;

		while (y0 != y1)
		{
			y0 += dy;
			x0 = m * y0 + b;

			bmdCanvas.setPixel32(x0, y0, lColor);
		}
	}
}
```

This program is essentially the same as the previous one but functionally complete to render lines correctly for all possible cases. But there is plenty of room for optimization. Firstly, it uses floating point numbers for what is eventually to be plotted on screen as pixels, which are in whole numbers. Secondly, it computes the value of the next pixel using the slope-intercept formula. This can be optimized away by pre-calculating this equation once out of the loop and then incrementing the value of the row or column in the loop.

### Optimization of Inner Loops

```actionscript
public static function lineDDA(bmdCanvas:BitmapData, x0:int, y0:int, x1:int, y1:int, lColor:uint)
{
	/**
	 * Line drawn using DDA algorithm
	 * Optimizations are made by replacing the repetitive
	 * calculations of the slope-intercept formula in
	 * the loop with 2 division operations outside it
	 * and instead incrementing the values of x and y
	 * variables with these pre-computed numbers
	 * inside the loop
	 */

	var dx:Number, dy:Number;
	var steps:Number;
	var m:Number;
	var x:Number, y:Number;
	var xinc:Number, yinc:Number;

	// Determine the direction of incrementation based on the slope
	dx = x1 - x0;
	dy = y1 - y0;
	if (Math.abs(dx) > Math.abs(dy))
		steps = Math.abs(dx);
	else
		steps = Math.abs(dy);

	// Calculate the incrementation for each axes
	xinc = dx / steps;
	yinc = dy / steps;

	x = x0;
	y = y0;

	for (var i:Number = 1; i < steps; i++)
	{
		x += xinc;
		y += yinc;
		bmdCanvas.setPixel32(x, y, lColor);
	}
}
```

This algorithm does not use the slope-intercept formula at all. Instead, it calculates the difference between the start and end points and increments the position on both axes in the loop. Whereas calculating the y-coordinate with the slope-intercept formula requires the use of one multiplication and one addition, incrementing the y-coordinate only requires a single addition operation, which makes it blazingly fast.

### Further Speed Gains

Floating point operations are desirable for plotting on vector graphics terminals because they are capable for drawing a continuous line. But when using raster displays, which only allow plotting discrete points to approximate a true line, floating point data becomes redundant. Thus, if we can find a way to compute a line using only integers, we can speed up our algorithm, while not losing any substantial information because the pixels used to approximate the line are whole numbers anyway.

Bresenham’s Line Drawing Algorithm uses integers to plot a line on the screen. An implementation is shown below.

```actionscript
public static function lineBresenham2(bmdCanvas:BitmapData, x0:int, y0:int, x1:int, y1:int, lColor:uint)
{
	var dx:int;
	var dy:int;
	var stepx:int
	var stepy:int;
	var counter:int

	// Calculate the difference between the start and end coordinates
	dx = x1 - x0;
	dy = y1 - y0;

	// Determine the direction of the increment in both axes
	if (dx < 0)
	{
		dx = -dx;
		stepx = -1;
	}
	else
	{
		stepx = 1;
	}

	if (dy < 0)
	{
		dy = -dy;
		stepy = -1;
	}
	else
	{
		stepy = 1;
	}

	bmdCanvas.setPixel32(x0, y0, lColor);

	dx <<= 1;
	dy <<= 1;

	if (dx > dy)
	{
		/** Line is wider than it is taller */

		/**
		 * Counter is used to track the number of times
		 * dx is divisible by dy. The value of dy is
		 * repeatedly subtracted from it on every iteration
		 * until it becomes less than or equal to 0.
		 * When that happens, the y-coordinate is incremented
		 * by stepy and counter is reset to dx.
		 */
		counter = dx;

		while (x0 != x1)
		{
			/**
			 * Increment the y-coordinate by stepy when
			 * the counter becomes less than 0
			 */
			if (counter <= 0)
			{
				y0 += stepy;
				counter += dx;
			}

			// Increment the x-coordinate on every iteration
			x0 += stepx;

			// Reduce the counter by dy
			counter -= dy;

			bmdCanvas.setPixel32(x0, y0, lColor);
		}
	}
	else
	{
		/** Line is taller than it is wider */

		counter = dy >> 1;

		while (y0 != y1)
		{
			if (counter <= 0)
			{
				x0 += stepx;
				counter += dy;
			}

			y0 += stepy;
			counter -= dx;

			bmdCanvas.setPixel32(x0, y0, lColor);
		}
	}
}
```

Division here is implemented as a series of continuous subtractions. The initial value of the variable fraction is set to dx. Assuming that the line is to be plotted from 0,0 to 100, 10, the values of dx and dy are 100 and 10, respectively. Thus, fraction is assigned the value 100. The variables x0 and y0 are the starting coordinates of the line. With each iteration of the loop, x0 is increased by 1 and the value of dy subtracted from fraction. When the fraction becomes less than or equal to 0, the y-coordinate is incremented by 1 and the counter reset to dx. The variable dy can be subtracted from counter dx / dy times before it becomes less than or equal to 0, making it a brute-force form of division.
