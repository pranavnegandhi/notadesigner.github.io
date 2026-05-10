---
title: Runtimes Die. Seams Don't.
date: 2026-04-27
---

Adobe killed Flash on January 1, 2021. They didn't deprecate it or issue a stern warning; they shipped a binary that refused to play SWF content past a hardcoded date — a self-destruct timer compiled into the runtime years in advance — and then pushed a forced uninstaller through Windows Update for good measure.

Adobe didn't break up with Flash. Adobe took out a contract on it.

I know this because that morning I was running a digital signage fleet — hundreds of screens across retail, banking, corporate lobbies and hospitality — and every single one of them ran on Flash.

This was a known event. Adobe had announced the end-of-life in 2017. Four years of warning. Management had spent three of them hemming, half of one hawing, and the last six months in committee. Then Adobe's compiled killswitch did what management couldn't: it made the decision.

Happy new year.

The next 72 hours bought us six months. I had been hoarding Flash Player builds for the better part of a decade, and the pathology was about to pay rent. Somewhere in that archive was version 32.0.0.344; the build released just before the one Adobe had shipped with the time-bomb compiled in. Install the prior build, and Flash worked.

Getting it onto hundreds of devices that had already auto-updated themselves into uselessness was the next problem, and the solution does not survive a security audit. We built one machine on a copy of Windows 7 (which had also EOL'd by then) whose licensing was illegitimate enough that Windows Update refused to talk to it — usually a problem, that week a feature — froze it as a disk image, and operations spent the next several days slapping the image onto customer hardware in the field. The runtime Adobe had erased from the planet was running again, illegally, on illegal Windows, in retail floors, bank branches and hotel lobbies.

Tier-1 cities were tedious but tractable. The remote sites were the real bill. A meaningful chunk of our customer estate belonged to banks pushing into tier-2 and tier-3 towns to introduce ATM cards and netbanking to communities that genuinely preferred cash. They were prime ground for promotional signage. Those sites had no permanent internet — fifteen minutes a day, borrowed off the branch manager's personal hotspot in the morning, and that was the whole window. There was no remote anything. Operations executives got on buses and trains, USB sticks in their pockets, and went to each site in person to reload a computer that had stopped working because Adobe had decided, six years and one binary update earlier, that today was the day Flash would die.

You are not supposed to be able to roll back a runtime that has erased itself from the planet. With sufficient hoarding and a forgiving relationship with software licensing, it turns out you can. We bought the runway, and we burned it porting.

Here is the part that surprises people. The port itself was boring.

## Why Flash, and why it didn't matter that we picked Flash

A reasonable question: why was a 2010s digital signage product running on Flash in the first place?

Because in 2010, when the product was conceptualised, Flash was the right answer. HTML5 video at the time was a clown car. Codecs disagreed, browsers disagreed, and audio had its own separate flavour of disagreement. Autoplay policies had not yet been weaponised but were already actively unhelpful. Flash, by contrast, played video — smoothly, anywhere it was installed, on a single codepath.

The 2010 me made one decision that would matter ten years later, though I had no idea at the time. I refused to write the player in plain Flash. I insisted on Flex.

If you've never used Flex: it was Adobe's attempt to drag Flash into the world of grown-up application development, with components, data binding, MXML and a proper compilation toolchain. Structurally it looked like the ancestor of every web framework you've used since — React, Angular, Blazor, Vue — built on the now-familiar primitives of components, props, events and a tree.

Flex itself was already on its way to abandonment by 2011. Adobe donated it to Apache, which is the corporate equivalent of leaving your dog at a shelter. But the Flex programming model — components in a hierarchy, not symbols on a timeline — turned out to be the lifeboat.

Here is the thing nobody tells you about scene graphs: they don't translate. A retained-mode display list with timeline-driven animation has no analogue in any modern web framework, and if your 2010 Flash player was built the Flash way — with MovieClips, timelines and stage hierarchies — your 2021 migration is a rewrite from scratch, while Adobe is throwing a grenade through your window.

If your 2010 player was a Flex component tree, your 2021 migration is mostly rote translation. An `<mx:VBox>` is a `<div>`. A Flex component is a Blazor component. Data flows down, events bubble up, the mental model survives unchanged.

Component hierarchies travel. Scene graphs don't.

## The contract nobody was supposed to look at

The signage player did not run alone. It was embedded inside a WinForms host process that handled the unglamorous side of the product, such as scheduling, content sync, device management and watchdog. The Flash player was a guest in that process, hosted via the Flash ActiveX control.

Host and guest needed to talk. We did not use the Flash ActiveX's official scripting bridge, because that path was, charitably, a lie agreed upon. Instead, the WinForms host opened a TCP socket on localhost. The Flash player connected to it. They exchanged hand-rolled XML messages.

```xml
<play id="42" asset="welcome.mp4" />
<ack  id="42" />
<ended id="42" reason="complete" />
```

This was, on every objective metric, ugly: hand-rolled framing, a bespoke schema, and an XML parser doing real work in a content-rendering loop. If you saw it in a code review you would file a ticket.

It was also the entire reason the migration was a swap and not a rewrite.

The WinForms host did not care what was on the other end of that socket. It cared about the contract. Send `<play id="42"/>`, expect `<ack id="42"/>`. The runtime was not a dependency of the host; it was a peer that spoke a protocol. That distinction is everything.

When Flash died, the replacement could no longer step up to keep its half of the contract. Blazor WASM could not connect to a raw socket. But it could communicate over HTTP. So along with the ActiveX, the socket connection was also replaced with a WebApi application hosted within the WinForms application as a background service. New transport, new language, new runtime.

If there is one architectural lesson in this whole story, it is this: **the runtime is the smallest replaceable unit only if you build a seam around it.** No seam, no swap. We had a seam, and the seam was ugly, but beautiful seams are for retrospectives. Ugly seams are for January 1.

## The forced-modernisation bonus

There is a particular flavour of luck where two unrelated disasters arrive on the same day and cancel each other out.

.NET 5 had shipped on November 10, 2020 — eight weeks before Adobe's killswitch fired. The first unified .NET release after the Framework / Core schism. We were on .NET Framework 4.8 because management had been dodging the Core migration with the same tactical sophistication they had brought to the Flash question.

Now we needed Kestrel inside our WinForms host to serve the Web API the new Blazor player would call. Kestrel did not run on Framework. Kestrel ran on Core, and now on .NET 5.

The Flash forcing function turned out to be the .NET forcing function. We swapped the runtime, the framework, the host integration and the IPC transport in one go. There was no other version of this project where management would have signed off on any of those individually.

Adobe did the hard part. Adobe decided.

## What 90% mechanical actually means

People hear "we ported the Flash app to Blazor" and assume catastrophe. The actual port was boring. Ninety percent of the codebase translated by hand into C# without drama. ActionScript 3 and C# are siblings — both descend from the same ECMAScript-4 / Java family tree, both class-based, both strongly typed. Most of the day's work was renaming `:Number` to `double` and substituting syntax.

The 10% that did not translate cleanly was where the interesting questions lived.

`enterFrame` was the obvious one. AS3's `enterFrame` event fires every frame at the stage's frame rate, and a small but stubborn part of the codebase relied on it for tweens and timing. Blazor has no `enterFrame`. The web has `requestAnimationFrame`, which fires at the display's refresh rate, which is not the same thing and not always 60Hz. We rewrote those niches against `requestAnimationFrame` and stopped pretending we had frame-accurate timing. We did not, in any meaningful sense, ever have it on Flash either.

H.264 video was the part that turned out to have been solved in 2015. The content team had been mastering everything in H.264 for half a decade on engineering's instructions, which at the time felt like pedantry and turned out to be a content-library life-saver. WebView2 played the same files Flash had. Sales agreed to live with playback that was close-enough but not Flash-smooth, on the explicit understanding that it would improve as browsers improved. They were right; it did.

The boring port was a feature, not a coincidence. It was the dividend on architectural decisions made a decade earlier by people who had no idea Adobe was going to fire the gun.

## What Flash actually did better

I am not going to pretend this was a no-loss migration. Five years on, with the rose-tint kicking in, here is what I honestly miss.

**Intuitive Graphics API.** The AS3 graphics API was simple and direct. Create a display object that sits in the scene graph, grab its graphics object, and start plotting away. The runtime supported this natively on every platform it ran on. Nor did it require additional libraries. Flex came in clutch with its component architecture. But runtime vector graphics elevated the visuals above cookie-cutter defaults.

```actionscript
var mySprite:Sprite = new Sprite();
addChild(mySprite);

// Access the graphics property directly
var g:Graphics = mySprite.graphics;

// 1. Set the visual style
g.lineStyle(4, 0xFF0000, 1.0); // 4px thick, Red, 100% alpha
g.beginFill(0x00FF00, 0.5);    // Green, 50% opacity

// 2. Start drawing
g.moveTo(50, 50);             // Jump to start point
g.lineTo(50, 150);            // Vertical line
g.lineTo(150, 150);           // Horizontal line
g.lineTo(150, 120);           // Up
g.lineTo(80, 120);            // Left
g.lineTo(80, 50);             // Up
g.lineTo(50, 50);             // Close the path back to start

// 3. Render the fill
g.endFill();
```

**Asset Bundling.** Assets like images, audio, fonts and animation clips could be embedded right there along with the binary. .NET applications support this behaviour too. But doing so for the web in Blazor is MSBuild's problem, then `dotnet publish`'s problem, then yours.

**ActionScript was less chatty than C#.** A lot of what is now boilerplate in our codebase was hidden under the AS3 runtime; I never debugged an `HttpClient` leak in AS3, and I have debugged several since. The runtime made certain classes of mistake unavailable to you, which is a kind of API design we have largely forgotten how to do.

The rest of Flash I do not miss. The IDE was a slow horror, the compiler errors were creative writing, the deployment story was a bin folder full of `.swf` files and prayer. But the runtime was ergonomic in a way modern stacks have lost track of, and it is worth saying out loud before the entire generation that wrote AS3 retires and the experience evaporates.
