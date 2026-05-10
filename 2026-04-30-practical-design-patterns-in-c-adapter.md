# Practical Design Patterns in C# – Adapter

In 2017 I inherited a digital signage player that had been written in .NET 4.0 in 2010 and had since accumulated the geological strata you'd expect of any seven-year-old line-of-business project: threads launched directly with `new Thread()`, `Invoke` calls bouncing off the UI dispatcher like pinballs, retry loops that polled until something either worked or didn't, and not a single `lock` statement anywhere in the synchronization-free void. The previous engineer had achieved concurrency the way medieval astronomers achieved planetary motion — by adding epicycles. I was not, at the time, a meaningfully better engineer than the one I was replacing. But I had read enough of the .NET 4.5 release notes to know that the entire mess could be erased by `await`.

The player ran on cheap fanless boxes mounted behind LCD screens in airport lounges and retail corners, the kind of hardware you forget exists until something falls off a wall. On every start-up it phoned home with a hardware inventory — CPU, RAM, disk, BIOS revision — so the central server could keep track of what was actually deployed in the field. The fleet drifted constantly. Boxes died, boxes got swapped, technicians plugged things into the wrong slot. The inventory existed so that when a support ticket arrived saying "the Düsseldorf one isn't booting," somebody could check whether the Düsseldorf one had quietly become a different machine three months earlier.

The data came from WMI — Windows Management Instrumentation — which is the operating system's answer to "how do I ask the kernel polite questions." WMI works. It has worked since Windows 2000. The problem is its API.

Here's how you ask WMI for the list of logical disks if you want the call to be asynchronous:

```csharp
public class WmiClient
{
    public void GetDiskData()
    {
        var management = new ManagementClass("Win32_LogicalDisk");
        var watcher = new ManagementOperationObserver();
        watcher.Completed += WatcherCompletedHandler;
        management.GetInstances(watcher);
    }

    private void WatcherCompletedHandler(object sender, CompletedEventArgs e)
    {
        // Whatever you actually wanted to do with the result lives here,
        // forty lines away from the call site, in a method that has no
        // way to return a value to the place that triggered it.
    }
}
```

This is the Event-based Asynchronous Pattern, or EAP. It was the official Microsoft-blessed answer to asynchrony from roughly 2005 to 2012, and you can recognize it on sight by the trio of `xxxAsync` method, `xxxCompleted` event, and the obligatory `CancelAsync`. There is nothing wrong with it that fifteen years of compiler improvements haven't fixed. The trouble is composition. Try sequencing three EAP calls — fetch disks, then fetch CPU, then fetch BIOS — and you end up with handlers nested inside handlers, each one dispatching the next, the eventual continuation buried so far inside a callback chain that you stop being able to reason about ownership of state.

The Gang of Four called this kind of thing a job for the Adapter pattern: take an existing interface that clients can't conveniently consume and wrap it in one that they can. They were thinking about cross-platform widget toolkits at the time, but the shape of the problem is the same. The EAP API is a perfectly good library. It just speaks a dialect that nobody on my team — or on Microsoft's compiler team, by 2012 — wanted to speak anymore.

What makes this particular adapter interesting is that it isn't *my* adapter. The conversion from EAP to TAP — Task-based Asynchronous Pattern — is a documented Microsoft recipe. There's a page on docs.microsoft.com that tells you exactly how to do it, and the heart of the recipe is one type: `TaskCompletionSource<T>`. You hand it out as a `Task`, you keep the `SetResult` / `SetException` / `SetCanceled` methods to yourself, and the asynchronous operation underneath gets to be as ugly as it needs to be without any of that ugliness leaking past the adapter.

Here's WMI again, this time wearing a suit:

```csharp
public class DiskInfoProvider
{
    public Task<PropertyDataCollection> CollectAsync()
    {
        var tcs = new TaskCompletionSource<PropertyDataCollection>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var watcher = new ManagementOperationObserver();

        CompletedEventHandler handler = null;
        handler = (sender, e) =>
        {
            try
            {
                switch (e.Status)
                {
                    case ManagementStatus.NoError:
                        tcs.SetResult(e.StatusObject.Properties);
                        break;
                    case ManagementStatus.CallCanceled:
                        tcs.SetCanceled();
                        break;
                    default:
                        tcs.SetException(
                            new ManagementException($"WMI call failed: {e.Status}"));
                        break;
                }
            }
            finally
            {
                watcher.Completed -= handler;
            }
        };

        watcher.Completed += handler;
        new ManagementClass("Win32_LogicalDisk").GetInstances(watcher);
        return tcs.Task;
    }
}
```

The call site collapses to the thing you wanted in the first place:

```csharp
var disks = await new DiskInfoProvider().CollectAsync();
```

Three lines of boilerplate at the call site, gone. The handler-and-state-juggling is inside the adapter, where it belongs. And — this is the part that turns out to matter — anywhere else in the codebase that wants to compose this call with another, it's now just a `Task` and obeys all the usual rules: `Task.WhenAll`, `try`/`catch`, `CancellationToken`, the lot.

There are exactly two ways to write this adapter wrong, and I have personally written it both ways.

The first is the handler leak. Notice the `handler = null` followed by `handler = (sender, e) => …` dance — that exists so the lambda can reference itself in order to unsubscribe. If you skip the `finally` block, every adapter call leaves a closure pinned to the watcher, which pins the `TaskCompletionSource`, which pins everything closed over by the caller. Run this in a long-lived process and your working set becomes a museum of completed disk inventories. I found out the hard way, by watching memory climb on a player that had been running for three weeks straight.

The second is forgetting that `TaskCompletionSource.SetResult` runs continuations *synchronously by default*. If the continuation is heavy — say, JSON-serializing an inventory payload before POSTing it over the wire — it executes on the WMI worker thread, blocks that thread, and the next WMI call queues up behind it. The fix is the constructor argument I quietly added above: `TaskCreationOptions.RunContinuationsAsynchronously`. The default is wrong for almost every adapter you will ever write, and Microsoft has acknowledged as much in the documentation without ever changing it.

I picked the inventory pipeline as the first thing to convert for reasons that had nothing to do with the pattern and everything to do with risk. It was self-contained — one class, one external dependency, no UI thread to placate. It ran exactly once per process lifetime, at startup, so I could test it by restarting the player. And the failure mode was generous: if the call broke, the server's record of that machine's hardware specs would go stale, and somebody six months later would be looking at a tech support ticket wondering why the box reported 4GB of RAM when it had clearly been upgraded to 8.

The player hardware never got upgraded. The boxes were cheap enough that when one stopped being adequate it was landfilled and replaced rather than improved, and the inventory data was, in some quiet sense, a record of the fleet's slow geological turnover. Stale records weren't a bug; they were the start of the conversation that ended with somebody driving to the airport with a new box in the trunk.

The Adapter pattern is, on paper, the least exciting of the structural patterns. It does one thing, that thing is renaming, and the GoF chapter on it is shorter than the one on Bridge for a reason. But EAP-to-TAP is what happens when adapter is taken seriously at the scale of an entire ecosystem: a generation of .NET APIs, all speaking a dialect the language no longer rewards, all rescuable with the same six lines of `TaskCompletionSource`. Most patterns you reach for once or twice in a career. This one you'll write again on Tuesday.
