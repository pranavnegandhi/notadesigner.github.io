---
title: "Asynchronous AIR Native Extensions"
date: 2015-02-11T09:17:33Z
slug: asynchronous-air-native-extensions
aliases: ["/asynchronous-air-native-extensions/"]
categories:
  - "Technique"
tags:
  - "actionscript native-extensions"
wp_post_id: 826
---

What could be worse than attempting to build an image processing application in ActionScript? While the language has its positives, handling large data sets quickly is not one of them. This was the dilemma we were up against in a recent assignment. And the images themselves were expected to be hot off the camera sensors. At a time when even tiny point-and-shoot cameras regularly reach 12 megapixels, our target audiences of photographers and studios were expected to hit us with a minimum of 24 megapixels.

That works out to 72 megabytes of data per image. Now multiply that by a few hundred images and you know why we were very concerned. The task in question was not very complicated - just resizing images. But the volume of data ensured that even this mundane operation took forever to complete, without any feedback to the user because ActionScript runs on a single thread.

We flirted a bit with ActionScript Workers, but they were an incomplete solution. Our UI thread became available again, but the processing still remained unacceptably slow.

Our fallback came in the form of Adobe's Native Extension API, that allows ActionScript applications running in the Adobe Integrated Runtime to access libraries written in C or similar languages.

Well, that's that then. This task was easy enough. Bang out some functions to resize and encode images and make a call from ActionScript. Since it was native, it would be fast and the user would never even notice the pause.

Unfortunately, it didn't turn out that straightforward.

While this operation became faster by an order of magnitude, it still stuttered when loading really high resolution images. And when multiplied by a hundred or so photos to be batch processed, the frequent freezing of the UI was very obvious.

So it was back to the drawing board.

### Asynchronous Processing

The native extension API offers the ability to dispatch events from the native code back into the runtime. This has been designed precisely for such situations. The extension spawns a new thread that handles the processing and returns control back to the runtime on the UI thread. After the computations are complete, the native extension dispatches an event that the runtime listens for.

To illustrate, let us implement a simple native extension that runs a timer and notifies the runtime when the allocated time is up. For simplicity, this timer will run for ten seconds. It can be made to run for arbitrary durations by passing the interval value as a parameter to the native side. We'll call this extension the Ten Second Timer. Its ActionScript class definition is as follows.

```actionscript
namespace com.notadesigner.utils
{
    public class TenSecondTimer extends flash.events.EventDispatcher
    {
        public function start():void {}

        private function context_statusHandler(event:Event):void
    }
}
```

This class extends EventDispatcher. Collectively, the client, TenSecondTimer and the native code set up a chain. The client listens for events from TenSecondTimer, which in turn subscribes for events from the native code. When the native code dispatches an event, TenSecondTimer creates its own Event instance and dispatches it. The client thus receives an indirect notification from the native code through the TenSecondTimer class.

```actionscript
this.m_context = ExtensionContext.createExtensionContext("com.notadesigner.TenSecondDelay", "");
this.m_context.addEventListener(StatusEvent.STATUS, this.context_statusHandler);
this.m_context.call("start");
```

On the native side, the start function is implemented with the function signature required of all native API functions.

```cpp
FREObject start(FREContext, void*, uint32_t, FREObject[]);
```

When this function is invoked by the runtime, it spawns a new thread (using the pthread API in this case) and immediately returns control back to the runtime. A reference to the waitForDuration function is passed to the pthread\_create function. The newly created thread executes that function.

```cpp
FREObject start(FREContext context, void* functionData, uint32_t argc, FREObject argv[])
{
    pthread_t thread;
    pthread_create(&thread, NULL, (void *)&waitForDuration, NULL);

    return NULL;
}
```

The waitForDuration function calls the usleep API that suspends the thread for 10 seconds. The CPU wakes this thread again after this duration has elapsed, and the function dispatches an event through FREDispatchStatusEventAsync.

```cpp
void* waitForDuration(void* arg)
{
    usleep(10000000);

    FREDispatchStatusEventAsync(_context, (const uint8_t*) "complete", (const uint8_t*) "done");

    return NULL;
}
```

In order to notify the runtime, the native coded needs to maintain a reference to the context. The context is passed as a parameter by the native extension API to the context initializer function. This function must store the reference somewhere that is accessible to the thread. A global variable works nicely in a pinch.

The runtime then kicks back into action, and passes on the event notification to the TenSecondTimer class. The context\_statusHandler method is triggered, which in turn dispatches a complete event for the client to handle.

```actionscript
private function context_statusHandler(event:StatusEvent):void
{
    var event2:Event = new Event(Event.COMPLETE);
    this.dispatchEvent(event2);
}
```

This pattern of triggering actions in the runtime from the native code can be used for a variety of other tasks that require asynchronous execution. The function invoked by the thread can perform whatever task may be required of it in place of the usleep call.

In our case, we implemented the native extension method to resize images asynchronously. Since loading the original image was taking the most amount of time, it went straight into the separate thread. The thread also took care of resizing the image after it was loaded and saving the resized file back to disk.
