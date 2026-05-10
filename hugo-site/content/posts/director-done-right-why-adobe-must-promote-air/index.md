---
title: "Director Done Right – Why Adobe Must Promote AIR"
date: 2010-03-03T05:39:08Z
slug: director-done-right-why-adobe-must-promote-air
aliases: ["/director-done-right-why-adobe-must-promote-air/"]
categories:
  - "Construction"
tags:
  - "actionscript"
wp_post_id: 48
---

When I leapt headfirst into multimedia development a decade ago, there was just one go-to product for all interactive needs – Macromedia Director. With origins in the vastly successful MacroMind VideoWorks application, Director had a strong pedigree in multimedia production. Regular updates of the product over more than a decade had converted the original VideoWorks into a development environment with an object-oriented programming language of its own called Lingo. It also supported external plug-ins for added features, ran on Windows and Macintosh computers and supported all media formats of the day – plain text, images, sound and video. The greatest plus point of Director was its ability to create a projector out of the Director file, which was an executable file that bundled the player and the binary file along with any plugins that were used. The projector was guaranteed to work on any system without requiring any additional libraries (except maybe audio and video codecs).

### A new age, a new champion

Then the whole web thing happened and Director could not keep pace any more. Macromedia shipped Xtras to support more media formats such as hypertext and Flash, and to make network requests. They also shipped the Shockwave browser plug-in to run a Director file in the browser. But it was no match for the Flash plug-in, which was a fraction of the download. This weighed heavily in the dial-up era back then. Chinks in the development environment began to take their toll. The biggest flaw of Director was that it bundled media and scripts in its working file into a single binary blob and saved it to disk. This meant that version control tools could not diff the scripts against previous revisions. Developers could not collaborate and work upon the same project without leaping through significant loops. The files would sometimes get damaged and become unreadable if there were power failures or other system breakdowns while the file was being saved. There was no easy way to create and reuse libraries. And the eccentric syntax of the Lingo language was a black mark on its developers, who could never gain any repute among people who used “real” programming languages with curly braces.

When Flash began to gain a foothold in the market because of its reach and ease of use, multimedia developers began to move on to the new platform in a steady trickle over the years, until Director became a niche production environment. The situation gave Macromedia a chance to right the problems of Director on a new platform, blank as a clean slate. And they capitalized upon it in a stellar manner. ActionScript quickly morphed from a simple scripting language with a handful of constructs, to a full-blown development environment that supported complex object hierarchies, most OOP principles, a rich API for various tasks ranging from media control to network access, all packaged inside a runtime that was tiny and already available on most of the internet-enabled computers. And it had curly braces.

### A web of entanglement

Yet, the biggest drawback of Flash was its sandbox that prevented developers from doing anything beyond the boundaries of the browser. Local file system access was out. Native nested windows were out. Drag and drop was not available. Launching native processes was out. Projector files were slightly more forgiving and allowed read-only local file access and limited spawning of new processes. But this was nowhere close to the unrestricted freedom offered by Director projectors. This in itself was not bad most of the time. But when more serious functionality was needed, ActionScript developers had to depend upon another programming language to get the job done. To be fair, Macromedia, and now Adobe have always done their best to make cross-language interop as easy as possible. We have come a long way from the getURL() calls required in ActionScript 1 to today’s ExternalInterface API. But a dependency meant having to support yet another codebase.

### Breaking free

It was heartening then to hear of the Adobe AIR announcement some years ago. Finally, Flash applications could be written to take advantage of local facilities of the system along with the existing media and internet APIs. It was now possible to read and write local files in a meaningful manner. Network utilities could be a lot more proactive and efficient. Applications could update themselves transparently and could support richer desktop paradigms such as taskbar icons and drag-and-drop interactions.

There still isn’t complete unfettered access to the system, the way Director projectors could. But this limitation will remain due to the different security scenario today. Being able to sell AIR as a secure replacement for web applications is highly desirable for Adobe in the face of HTML 5 and the advancing capabilities of browsers. But combined with all the other benefits that Flash offers, along with the reduced development time over other desktop development environments, and instant cross-platform compatibility, this is an insignificant bump.

The only drawback that AIR has over Director is that it requires a separate download of the AIR runtime. At 11 MB, it does not add a significant cost in today’s age. But it is a deal breaker for users without administrative rights. There really should be a way to package the runtime and the application package into a single executable.

### HTML is not the only future

There still are naysayers who predict that the Flash platform is dead. All that is needed is wider acceptance and support for HTML 5. But with support for HTML and JavaScript built into AIR (with WebKit, no less), Adobe already has that base covered. And the web is not the do-all and end-all of computing. While having a social networking application online makes sense, personal accounting or mail applications are better kept locally for security and accessibility reasons. And desktop applications have the unique ability to morph into hybrids that work locally as well as with remote services, which is better in some cases than having a web-only application. Most people already use a hybrid application – their mail client. New mails can be downloaded and read when connected online, while still being able to access downloaded emails when working offline.

Sure the W3C is working on writing specifications for similar functionality with HTML 5. But who said choice is bad? Given the sorry state of browser standardization even today, it is more likely that each browser publisher will implement the spec in some half-assed manner that causes subtle differences between them and require someone to invent a shim to fix those incompatibilities. And HTML 5 does not address all the features that AIR supports, including hardware accelerated 3D and filter effects, native menus, file extension registration, drag and drop, advanced sound and video APIs and windowing support beyond the most rudimentary.

In spite of all the advantages that it has, AIR has failed to make major inroads into desktop development. Blame lies squarely on Adobe for failing to promote it well enough. Other than ActionScript developer circles, there rarely is any mention or knowledge of the platform. They really ought to pick up some steam on that front and corner the market the way Director and Flash have done in their time.
