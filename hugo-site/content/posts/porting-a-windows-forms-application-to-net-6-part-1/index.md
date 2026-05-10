---
title: "Porting a Windows Forms Application to .NET – Part 1"
date: 2022-12-24T10:26:16Z
slug: porting-a-windows-forms-application-to-net-6-part-1
aliases: ["/porting-a-windows-forms-application-to-net-6-part-1/"]
images: ["featured.jpg"]
categories:
  - "Construction"
  - "Software Engineering"
tags:
  - "c#"
featured_image: featured.jpg
wp_post_id: 1992
---

In a time when every dinky software company is writing massive systems that run in The Cloud™, it's a source of astonishment for some to hear about my continued work on a desktop application. It's not quite the same effort as synchronising 5K posts per second with millions of followers. But it does have its high notes nevertheless.

### Understanding the Legacy

Like many products from early-stage startups, the first iteration of the Vertika Player was built as a proof-of-concept. The objective was to fetch text, video and images from a web service and play it on a large screen. But the underlying Windows Forms framework wasn't really geared towards rendering multimedia content. So a separate application was written in ActionScript, which was hosted inside the Windows Forms application through the Flash Player ActiveX control.

Even during those days, web development was topping the list of desired programming skills. Nobody on the original team had significant experience engineering a desktop application. As a result, it came to be that most of the code got piled into the main window class, a couple of static utility classes, and a single god class containing approximately 200 public fields to maintain application state. The latter was referenced hither and yon all over the project. It was awful. The project was so fragile that even changing a simple network port number was fraught with risk of breakages in parts of the application that had nothing to do with networking.

Apparently, none of the programmers understood the significance of the UI thread, or even basic threading in general. Resource contentions were handled with a hail Mary running inside an infinite loop.

```csharp
while (!result)
{
    try
    {
        xmlDoc.Save(FilePath);
        result = true;
    }
    catch (Exception exception)
    {
        log.Error("An error occurred while saving to disk");
    }
}
```

There were some hard to replicate bugs that reared their heads at times. Support incidents came up occasionally about missing content, or false alarms were raised when the telemetry and monitoring thread went awry. A particularly egregious bug was the cause for occasional runaway file downloads that never stopped, appending the same sequence of bytes in a file ad infinitum. To the end user, the file would seem to grow continuously until it filled the entire hard disk.

But in spite of all these issues, the application worked as expected more often than it didn't. It generated steady revenue from paying customers who were mostly pleased with its performance. Support staff had formulated scripts for almost every scenario that they were likely to encounter. They could identify symptoms and were able to get in and resolve the problem quickly.

### Ringing in the New

Teams changed, until there was nobody left who had worked on the first iteration of the product.

New features were gradually added over time. But architectural issues continued to fester, while more lines of code were added to already expansive classes. At one time, the main window class exceeded 5,000 lines of code. It wasn't a pleasant experience maintaining this project. But the one thing that we all agreed upon was that we would never discard the project and start from scratch. We bided our time and waited for an opportunity to refactor the application.

We got our foot in when customers started reporting stutter during playback after minor update. Telemetry reports had to include a screen shot of the running application. But due to data caps at many locations, captured images used to be down-sampled to postage stamp sizes before uploading. Network capacities had recently been improved, and screen shot dimensions were increased accordingly.

Herein lay the catch. A 100 pixel square image is about 2 KB in size. But a 500 pixel square accounts for a 25x increase in data. The code to upload the image was being executed on the UI thread, blocking it while the network request was completing. This was imperceptible when the file sizes were small. But it became immediately noticeable once the images sizes were increased. It was glaringly obvious during playback of video because of periodic frame freeze that occurred while an image was uploaded.

We chose the expeditious fix for the time being. Instead of directly calling the upload method, it was wrapped into a `WaitCallback` delegate and queued for later execution through `ThreadPool.QueueUserWorkItem()`. The UI thread was freed up again, solving the playback stutters. But we made dozens of commits in the project while working our way to these few lines of change. This was the chance we were looking for. We had to get the cat out of the bag.

All our early modifications were mechanical and low-risk. Auto-format the code. Reduce the API surface area by hiding unnecessary public members. Remove dead and commented lines of code.

This exercise gave us the exposure to the codebase that was necessary to make further modifications. Further refactoring was driven by some of the SOLID principles. If we found a class that was doing too many things, we'd try to split it into different types with individual responsibilities. Interfaces were scarcely in use so far. So we began to define some new ones, being careful to contain their scope. But dependency inversion wasn't quite as high on our list of priorities. .NET Framework didn't ship with built-in DI features, and adding a third-party library for the job would impede our path to upgrade to .NET Core later down the line.

Over time, the quality of the codebase improved, and our knowledge of its internal working increased. There were few changes which were visible to the end user. In general, it was just a little more robust, faster to launch and run, and the UI looked a bit tidier.

### The Demise of Flash

Come January 2021, disaster struck. Flash had been on its last legs since a long time. But nobody expected Adobe to pull the rug from under us by forcefully removing the plug-in from all computers. This bricked every deployed instance of the application. Nothing could be displayed on any screen without the Flash Player.

<figure>
  <img src="flash-disabled.png" alt="">
  <figcaption>He's dead, Jim.</figcaption>
</figure>

We scrambled to find workarounds, such as installing older versions of Flash Player that did not contain the the self-destruct switch, blocking Windows Updates, and even deploying older versions of Windows that shipped with working versions of Flash. These were all temporary solutions. The juggernaut was unstoppable, and we were fielding support calls every week that involved a missing Flash Player. In addition to the technical complexities, Adobe had also revoked the license to install and run the Flash Player. Continuing to use it was exposing the company to legal ramifications that nobody wanted to deal with. We needed an alternative for Flash as soon as possible.

### Blazing Guns of Glory

Our salvation came in the form of the recently released Blazor framework from Microsoft. Built on the C# language, it borrowed a lot of concepts from ASP.NET and MVC. A crack team of developers was assembled, with the singular motive of porting the entire Flash application into Blazor. This was a significant diversion from our ground rule to never throw away everything and start from scratch.

Blazor generates a web application that runs in a browser. Embed a browser control into a Windows Forms project, and voila! It works just the same as the Flash application. The only significant difference was the communication protocol across process boundaries. Flash used to expose a TCP sockets API to communicate with the host process. Since web browsers don't expose any such interface, we had to fall back upon standard HTTP. The hosting process would expose a web API that would be accessible on localhost over the HTTP protocol.

The last item was a significant pivot point. Hosting a HTTP API would require a web server, for which there were limited options that worked within the .NET Framework ecosystem. HttpListener was far too low-level. Cassini wasn't as tightly integrated. GenHttp came with its own learning curve. In the long-term, it was going to be far better to switch from .NET Framework to .NET 5. And it shipped with its own built-in web server that supported running ASP.NET Web API out of the box.

The writing was on the wall. We would be porting to .NET 5.
