---
title: "Improved Application Performance with .NET 7"
date: 2023-01-25T07:27:17Z
slug: improved-application-performance-with-net-7
aliases: ["/improved-application-performance-with-net-7/"]
images: ["og-image.jpg"]
categories:
  - "Demonstration"
  - "Software Engineering"
tags:
  - ".net"
  - "c#"
  - "performance"
featured_image: featured.jpg
wp_post_id: 3059
---

Speed has been a big selling point for .NET 7. Stephen Toub has written [an extremely lengthy and detailed article back in August 2022 about all the performance improvements that went into it](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/). And it's ridiculously easy to take advantage of the speed boost in your own applications. In most cases, it only requires recompiling your project to target the new runtime. Developers who don't make the switch are leaving a lot of performance gains on the table. I took some effort to measure the speed improvements by running some benchmarks on my own projects.

Shades is a .NET port of the Python module of the same name, for creating generative art. The module was originally authored by Ben Rutter. The library operates by decomposing any drawing operation, no matter how complex, into a series of single-pixel modifications, executed one at a time, across the entire bitmap. To draw a line, the library first calculates every pixel that makes the line, then iterates through that list to compute its colour value, and making a discrete set pixel call for it. Drawing primitive shapes using solid colours is delegated to the much faster SkiaSharp library. But all the more advanced effects which have no equivalent operation in SkiaSharp have to be done with custom routines.

So of course it is slow. And anything that makes it go faster is a welcome improvement.

For this exercise, I chose the PixelsInsideEdge method, which operates on the coordinates that make up the outer edge of a shape, and identifies the pixels that fall within that boundary. It uses a simplified implementation of a ray-casting algorithm to determine whether a point is within the shape boundary or falls outside of it.

```csharp
public ICollection<SKPoint> PixelsInsideEdge(ICollection<SKPoint> edgePixels)
{
    /// Contains a list of distinct values along the X axis, extracted from edgePixels.
    /// Each unique value along the X axis corresponds to a list of unique values along
    /// the Y axis that intersect with the X coordinate.
    var xs = new SortedDictionary<int, SortedSet<int>>();
    int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
    foreach (var e in edgePixels)
    {
        var ex = Convert.ToInt32(e.X);
        var ey = Convert.ToInt32(e.Y);
        maxX = Math.Max(maxX, ex);
        minX = Math.Min(minX, ex);
        maxY = Math.Max(maxY, ey);
        minY = Math.Min(minY, ey);

        if (xs.TryGetValue(ex, out var points))
        {
            points.Add(ey);
        }
        else
        {
            xs[ex] = new SortedSet<int>() { ey };
        }
    }

    /// Contains a list of points that make up the inside of the shape formed by the
    /// bounds of edgePixels.
    var innerPixels = new List<SKPoint>();
    for (var x = minX; x <= maxX; x++)
    {
        var ys = xs[x];

        /// Find the lowest values along the Y axis, i.e. values
        /// that make up the lower edge of the shape.
        var temp = new SortedSet<int>();
        foreach (var y in ys)
        {
            if (!ys.TryGetValue(y - 1, out var _))
            {
                temp.Add(y);
            }
        }

        var rayCount = 0;
        for (var y = temp.Min; y <= temp.Max; y++)
        {
            if (temp.TryGetValue(y, out var _))
            {
                rayCount++;
            }

            if (rayCount % 2 == 1)
            {
                innerPixels.Add(new SKPoint(x, y));
            }
        }
    }

    innerPixels.AddRange(edgePixels);

    return innerPixels;
}
```

It was executed on a circular shape having a radius of 2048 pixels. This method identified 12869 points enclosed within its boundary. The location of the shape on the canvas had no effect. Negative coordinates were also calculated and stored in the result. The benchmark only ran the geometric computations. It did not perform any modifications on the image itself.

```csharp
[Benchmark]
[ArgumentsSource(nameof(LargeDataSource))]
public ICollection<SKPoint> PixelsInsideEdgeCollection(SKPoint[] Edges) => shade.PixelsInsideEdge(Edges);

public static IEnumerable<SKPoint[]> LargeDataSource()
{
    var shade = new BlockShade(SKColor.Empty);

    yield return shade.GetCircleEdge(new SKPoint(0, 0), 2048.0f).ToArray();
}
```

The results of this benchmark are shown below.

```powershell
dotnet run -c Release --filter PixelsInsideEdgeCollection --runtimes \
net5.0 net6.0 net7.0

// * Summary *

BenchmarkDotNet=v0.13.4, OS=Windows 11 (10.0.22621.1105)
AMD Ryzen 5 5600H with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
[Host] : .NET 5.0.17 (5.0.1722.21314), X64 RyuJIT AVX2
Job-SOHORL : .NET 5.0.17 (5.0.1722.21314), X64 RyuJIT AVX2
Job-PPCKCW : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT AVX2
Job-EBVIPG : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2

|           Method |  Runtime |          Edges |     Mean | Ratio |
|----------------- |--------- |--------------- |---------:|------:|
| PixelsInsideEdge | .NET 5.0 | SKPoint[12869] | 217.7 ms |  1.00 |
| PixelsInsideEdge | .NET 6.0 | SKPoint[12869] | 213.2 ms |  0.98 |
| PixelsInsideEdge | .NET 7.0 | SKPoint[12869] | 161.9 ms |  0.75 |
```

But what does this mean for the performance of the application as a whole? To test this, I ran Doom Fire, which replicates the effect from the title screen of the classic video game. I generated a 5 second animated sequence of images in sizes of 128 × 128, 256 × 128, 512 × 128 and 1024 × 128 pixels.

{{< video src="fire-128.mp4" poster="fire-256-poster.gif" >}}

{{< video src="fire-256.mp4" poster="fire-256-poster.gif" >}}

{{< video src="fire-512.mp4" poster="fire-256-poster.gif" caption="These three videos show the output of the effect at 128 × 128 pixels (top left), 256 × 128 pixels (top centre), and 512 × 128 pixels (above)." >}}

The height of the image was restricted to minimise the amount of empty pixels being pushed during the test. Time elapsed was measured with an instance of `System.Diagnostics.Stopwatch`. Each test was first run 5 times to warm up the JIT and allow the progressive compiler to thoroughly kick in and optimise the code. The results of these warm-up tests were discarded. Then 3 further iterations were run, and the fastest result was considered for comparison.

```
|  Runtime |           Size |    Time |
|--------- |---------------:|--------:|
| .NET 5.0 |   128 x 128 px | 13.59 s |
| .NET 5.0 |   256 x 128 px | 13.74 s |
| .NET 5.0 |   512 x 128 px | 25.67 s |
| .NET 5.0 |  1024 x 128 px | 49.17 s |
```

```
|  Runtime |           Size |    Time |
|--------- |---------------:|--------:|
| .NET 7.0 |   128 x 128 px | 13.35 s |
| .NET 7.0 |   256 x 128 px | 13.52 s |
| .NET 7.0 |   512 x 128 px | 23.23 s |
| .NET 7.0 |  1024 x 128 px | 44.53 s |
```

This wasn't quite as stark an improvement as a standalone microbenchmark. A single method being executed in quick succession has vastly different performance characteristics from a complete application. This benchmark also saved the frame to disk, which adds an I/O penalty. As expected, smaller data sets showed minimal variation. The numbers started diverging as the magnitude of data increased.

### Conclusion

Based on the standard benchmarks, .NET 5 and 6 show similar performance. With .NET 5 now out of support, and .NET 7 being earmarked as a short-term support release, developers are likely to upgrade to .NET 6 instead of .NET 7. And .NET 6 has an additional 6 months of support over .NET 7. There are many customers who would accept the tradeoff of slower runtime execution for longer official support. If your application is performance sensitive, it's a no-brainer that you must move to .NET 7. But even otherwise, if you can risk the shorter support cycle (and .NET 8 will certainly be released by then), it may be a worthwhile investment to consider targeting your application for .NET 7 instead.
