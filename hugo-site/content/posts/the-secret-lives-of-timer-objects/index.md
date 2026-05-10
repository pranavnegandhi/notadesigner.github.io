---
title: "The Secret Lives of Timer Objects"
date: 2010-12-27T10:48:09Z
slug: the-secret-lives-of-timer-objects
aliases: ["/the-secret-lives-of-timer-objects/", "/100/"]
categories:
  - "Technique"
tags:
  - "actionscript"
  - "memory-leaks"
wp_post_id: 100
---

One of the better side-effects of working in memory-managed languages is that you don’t have to bother with manually cleaning up references. This removes a huge chunk of busywork for developers who have one less thing to get their heads around. Allocate an object, use it, and forget about it. The garbage collector will get around to it eventually and clean it out.

What could be simpler?

### Great Power, Great Responsibility

However, with ActionScript 3, the bar has been raised substantially. This new incarnation adds a slew of APIs that increase its expressiveness and capabilities. Yes siree! ActionScript is no longer a toy language whose primary arsenal is gotoAndPlay(). There’s an extensive library of native classes that can do stuff like drawing on screen, playing audio, fetching text and binary data from all sorts of data sources, or even launching and communicating with other applications.

Take that, Java!

Several of these features mean that its legacy garbage collection techniques have to be replaced with less aggressive methods to identify unused memory. This in turn requires more developer intervention than before to identify which references are no longer required and which ones must be left untouched. The new AS3 garbage collector uses reference counting and mark sweeping (both techniques are covered by Grant Skinner here). And while things are still better than completely manual memory management, building complex or long-running applications requires that the developer have at least a passing understanding of how memory is allocated, references stored, passed around and cleared, and the potential for memory leaks in the midst of all this.

This is where the typical ActionScript programmer stumbles, mainly because people programming in AS3 often do not have a formal background in computer science or programming and have usually learned the language on their own through books or online tutorials. It is not uncommon to find ActionScript developers for whom this is their first taste of programming.

### Our Subject for Today

One subtle pitfall is the Timer object which is used to run code on a specific time sequence. This class consolidates the setTimeout and setInterval methods. The API is pretty straightforward – create an instance of the class with its delay and repeat count properties, set up listeners which are triggered at the end of each delay or after the number of delays specified by the repeat count are completed, and finally, call the start() method to begin the countdown.

A Timer object is different from other types of objects because the garbage collector does not clear it unless it is stopped, even if there are no active references to it.

Let’s repeat that.

**A Timer object is not cleared by the garbage collector as long as it is running, even when its references are set to null or the variable goes out of scope, and it continues to fire TIMER events as long as it is active**.

The only way to clear the object from memory and stop events from being fired is to stop it using the stop() or reset() methods.

The following piece of code illustrates the permanence of Timer objects which haven’t been stopped.

```actionscript
package 
{
    import flash.display.Sprite;
    import flash.events.Event;
    import flash.events.TimerEvent;
    import flash.utils.Timer;

    public class Main extends Sprite 
    {
        public function Main():void 
        {
             var t:Timer; // Create timer as a local variable

             t = new Timer(1000);
             t.addEventListener(TimerEvent.TIMER, this.m_tick);
             t.start(); // Reference ends here; Timer continues to exist and trigger events
        }

        private function m_tick(e:TimerEvent):void
        {
             trace("tick");
        }
    }
}
```

This may look like a design flaw in the language to the casual passer-by and elicit a very obvious question.

“Everything else is cleared automatically. Why leave behind timers?”

Some thought on the subject will make you realize that the language designers at Adobe weren’t off their rockers after all. This behaviour is by design. In the example code shown above, ‘t’ is a local variable inside the constructor. When the function ends, the variable goes out of scope and there are no more references to the Timer. If the garbage collector cleared it out, it would not fire any events at all. This would essentially mean that a developer could only assign Timer objects to those variables that remain in scope for as long as it is supposed to run, such as member variables inside a class.

Conversely, a Timer instance which remains active with no way for it to be accessed and stopped is terrible. The object continues to maintain references to listeners and triggers events on them regularly. Listeners can never be garbage collected either because their reference count never goes down to zero. And finally, each active timer occupies a slice of CPU time, bringing the processor down to its knees gradually.

The Timer object always passes a reference to itself in the target property of the TimerEvents it triggers. The listener can use this reference to stop the Timer. When the listener function ends, the event object goes out of scope and is cleared, taking the reference to the Timer object along with it and in turn, making it available for garbage collection.

Here is an example that illustrates how this memory leak can inadvertently occur.

```actionscript
package 
{
    import flash.events.TimerEvent;
    import flash.utils.Timer;

    public class Slideshow
    {
        private var m_timer:Timer;

        private var m_isActive:Boolean;

        /**
         * Example how not to use a timer
         */
        public function Start():void
        {
             this.m_timer = new Timer(5000); // Create and start a new timer
             this.m_timer.addEventListener(TimerEvent.TIMER, this.m_next);
             this.m_timer.start();
        }

        private function Pause(e:TimerEvent):void
        {
             this.m_isActive = false;
        }

        private function m_next(e:TimerEvent):void
        {
             if (!this.m_isActive) return;

             // Code to move to next slide
        }
    }
}
```

In the example above, the programmer using this slideshow component is expected to call removeChild() to remove it from the stage. However, because the component does not stop the Timer when it is removed, it will continue to fire the TIMER event for as long as the application is run, and also prevent the memory used by the component from being garbage collected by holding a reference to its m\_next method. If multiple instances of the slideshow object are created and disposed using removeChild(), their timers will continue to fire and none of the components will actually be cleared from memory.
