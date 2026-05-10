---
title: "Porting a Windows Forms Application to .NET – Part 2"
date: 2022-12-28T17:11:02Z
slug: porting-a-windows-forms-application-to-net-6-part-2
aliases: ["/porting-a-windows-forms-application-to-net-6-part-2/"]
images: ["featured.jpg"]
categories:
  - "Construction"
  - "Software Engineering"
tags:
  - "c#"
featured_image: featured.jpg
wp_post_id: 2042
---

[Previously](/porting-a-windows-forms-application-to-net-6-part-1/), I described the legacy of the Vertika Player, the bottlenecks in its initial development, some elementary efforts to refactor the code, and a major roadblock that came from the deprecation of the Flash Player.

Once we decided to move to .NET, work began in full earnest. Enthusiasm was running high, and nothing seemed impossible. Don't mind the old code. It was trash. We'd rewrite everything from scratch! Heck, we were so smart, we could do it twice over. But this fervour lasted all of 15 minutes before we folded up and rolled back all our changes.

I'm exaggerating, of course. We did write a new application shell from scratch, using newer APIs like the `Microsoft.Extensions.Hosting.IHost` interface and its dependency injection framework for some of the most fundamental classes that were needed. But there was immense pressure to get the product out of the door. Remember that the Flash Player uninstaller was well and truly active now, and support staff were working overtime to keep up with restoring it every time it got nuked. After a couple of weeks of this effort, the enormity of the exercise hit us and we fell back to copying files wholesale from the old code-base. On the brighter side, in spite of rewriting only a small portion of the code, the groundwork had been laid for more significant breakthroughs in the near future.

The singular monolith had been deconstructed into separate projects based on functionality, such as the primary executable, model classes, and the web API host. Over the following months, we added more projects to isolate critical portions of data synchronisation, the network download library (eventually replaced by [the Downloader library, written by Behzad Khosravifar](https://github.com/bezzad/Downloader)) and background services.

### A SOAP-y Muddle

There's a significant chunk of the application functionality that depends on communicating with a remote SOAP service (stop sniggering; this stuff is from 15 years ago). Visual Studio continues to provide tools to automatically generate a SOAP client from a service. But the output does not maintain API compatibility with the client generated with earlier versions of the tool. Among the methods missing from the new client are the synchronous variants of the service calls, which, unfortunately, were a mainstay in the earlier application code.

That's right. Microsoft used to ship tools that allowed developers to make synchronous network calls.

But this is all gone now. And I had a problem on my hands.

```csharp
public void ClickHandler(object sender, MouseEvent e)
{
    var users = serviceClient.GetUsers();
}
```

```
CS1061	'ServiceClient' does not contain a definition for 'GetUsers' and no accessible extension method 'GetUsers' accepting a first argument of type 'object' could be found (are you missing a using directive or an assembly reference?)
```

Welp!

Calling asynchronous code from previously written synchronous methods was not going to be easy. Web service calls were tightly integrated in many classes that were still oversized even after aggressive refactoring. Stephen Cleary's AsyncEx library came to our rescue. Asynchronous method invocations were wrapped inside calls to `AsyncContext.Run()`, which we liberally abused to build a synchronous abstraction over the TAP interface.

```csharp
public void ClickHandler(object sender, MouseEvent e)
{
    var users = AsyncContext.Run(serviceClient.GetUsersAsync());
}
```

This was much better than the alternative of calling `Wait()` or `Result` directly on the task. In addition to blocking what could potentially be the UI thread, it would also wrap any exceptions thrown during the invocation into an additional `AggregateException`. And anybody who's dealt with `InnerException` hell knows how bad that can be.

The second API incompatibility was in the collection types returned from the service. The earlier service client returned a `System.Data.DataSet` type for collections. This was changed to a new type called `ArrayOfXElement`. Fortunately, this was an easy fix with a simple extension method to convert the `ArrayOfXElement` into a `DataSet`.

### Wrapping Up and Rolling Out

The hour of reckoning arrived about 5 months later, when we finally removed references to the Flash Player ActiveX control from the project, replacing them with the WebView2 control. The minuscule amount of effort required of this step belies the enormity of its significance. Flash had been our rendering mainstay for over a decade. All those thousands of man-hours invested into the application were gone in a blink of an eye, replaced with a still-wet-behind-the-ears port to Blazor. This was the first time in the history of the company that legacy code was discarded entirely, and rewritten from scratch on an empty slate.

The new product was deployed to several test sites for a month to ensure that everything worked as expected. And other than a few basic layout errors, there were no problems that we encountered. The porting exercise was a success, and offered a major lifeline to the business.
