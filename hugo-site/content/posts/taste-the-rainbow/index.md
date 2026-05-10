---
title: "Taste the Rainbow"
date: 2026-05-10T14:30:10+05:30
slug: taste-the-rainbow
categories: []
tags: []
---

Every morning, the CEO would open the product, stare at his screen, and file the same bug report. The colours were wrong again.

Not wrong in a subtle, squint-and-you-might-notice way. Wrong in a yellow-was-now-violet way. Borders that were supposed to be brand blue had drifted to teal. Backgrounds that were set to warm grey had shifted to something faintly pink. The colours weren't just wrong. They were *moving*. He reported it on Monday. He reported it on Tuesday. By Wednesday it had become a ritual — open the app, wince, file the ticket. The developers could reproduce it without fail, but they were incapable of fixing it.

## One Colour, Seven Ways

The product was a content management system for digital signage. Operators would design their display layouts through a styling page — picking colours for text, borders, backgrounds, gradients, and other visual elements. Each element had its own colour picker, powered by a JavaScript library called jscolor. A busy styling page could have dozens of pickers, each backed by a text input field.

jscolor has an API. You call a method, you get a value. The documentation is not hidden behind a paywall. It is not classified. It is not written in Sumerian.

Nobody read it.

Instead, the developers extracted colour values by scooping them directly out of the text input field. jscolor populates the field with a hex string for display, so that's what they grabbed. But they couldn't agree on how. Some prefixed it with a hash: `#ffcc00` straight off the text input. Some stored it raw: `ffcc00`, stripping off the # character. Some, for reasons that died with their authors, split the hex into comma-separated pairs: `FF,CC,00`.

Three formats for the same six characters of data. All stored in `varchar(10)` columns, all parsed by a giant JavaScript function with dozens of conditionals and error checking. This was the good period.

Then gradients arrived.

An operator could now pick two colours — start and end — for gradient fills. Rather than add a second column, someone concatenated both values into a single string: `#ffcc00,#ff00cc`. The column was widened to accommodate this innovation. The function became twice as long to handle two data points.

All this time, alpha values had been stored in separate companion columns, ranging between 0.0 to 1.0. Then someone realized that jscolor could generate colour pickers with integrated transparency selection. The colour value would become 2 characters wider, but still short enough to fit in the database column. They patted themselves on the back for their foresight in choosing `varchar(10)`. Newer colour fields began to use the 32-bit hex representation. Older ones still stored transparency in separate fields.

Consistency is overrated.

Then the external consultants showed up.

The company brought in a team of front-end specialists to help port the application from ASP.NET WebForms to MVC. These were supposed to be the adults in the room. They took one look at jscolor, and realised that it supported CSS function notation. `rgba(255, 255, 255, 0.8)`. Stripped down to bare digits before storage: `255,255,255,1.0` in a `varchar(15)`. Barely made it!

Seven encoding formats. Three teams. One concept. All cohabiting in the same database columns, parsed by whichever JavaScript happened to be on the page that loaded them.

## The Colour Wheel of Misfortune

Inconsistency alone wouldn't have been fatal. You can write defensive parsers. You can handle multiple formats. It's ugly, but it works.

The parsing was lossy. That was fatal.

When a 32-bit hex value like `#ffcc00cc` was read by code that expected six characters after the hash, it would extract `ffcc00`. Bye-bye transparency. But a different parser on a different page might strip the hash first and read six characters from the end — `cc00ff`. Yellow just became violet.

Now here's where it stopped being a bug and started being performance art.

If the operator didn't notice the shift — and on a page with dozens of colour pickers, why would they — they'd hit Save. The corrupted value would be written back to the database. Next time anyone loaded that record, the parser would transform the already-wrong colour into something else. Each save-load cycle pushed the value further from its origin.

The colours were degenerative. The system was tie-dyeing itself, one round trip at a time.

The CEO wasn't reporting the same bug every morning. He was watching a different colour die every morning. The product was evolving its own palette, and nobody had asked it to.

## Slow as Molasses

Here is the function that sat at the heart of it, straight out of the archives. I don't know why I saved it. Some kind of perverse masochism.

```javascript
function HexToRGBCode(hex, IsAlpha, Trans) {
    if (IsAlpha) {
        return 'rgba(' + hexToRgb(hex).r + ',' + hexToRgb(hex).g + ',' 
            + hexToRgb(hex).b + ',' + Trans + ')';
    } else {
        return 'rgb(' + hexToRgb(hex).r + ',' + hexToRgb(hex).g + ',' 
            + hexToRgb(hex).b + ')';
    }
}

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}
```

The answer was *inside the function* and nobody saw it. The integers were right there, briefly alive between the regex match and the string concatenation, before being smashed back into a string and sent on their way.

This function ran hundreds of times across the page — every element that needed a colour value would invoke it. Hundreds of regex executions, hundreds of string concatenations, and three `parseInt` calls per invocation that were immediately thrown away. In the JavaScript engines of the mid-2010s, this was not free. Opening the styling page was an audit-worthy event.

## parseInt('ffcc00', 16)

That's it. That's the fix. JavaScript has a built-in conversion from hex strings to integers. Store the integer. `16763904` doesn't drift. It can store alpha. It doesn't care about hashes or commas or alpha channels. It is a number. It stays a number.

But that would have required someone to stop and ask: what is a colour, actually? Not "what does the text field say?" but "what does the value *mean*?" A colour is a number. It has been a number since the framebuffer was invented. Storing it as a string and parsing it back on every page load is like writing down a phone number in Roman numerals and converting it every time you dial.

Nobody on the original team stopped to ask. The consultants didn't either — they just brought their own string format to a database already drowning in string formats. Each team solved the immediate problem in front of them. Get the value out of the picker. Put it in the database. Ship it. Next ticket.

The `varchar` column was a bucket that accepted anything. So anything is what it got.

## Still Running

This was never fixed.

The styling system was too deeply wired into the product — too many pages, too many customers, too many stored values that would all need migrating. The cost of rearchitecting it was always higher than the cost of living with it. The CEO's morning bug reports eventually tapered off. Not because the problem was solved. Because everyone adjusted their expectations.

As far as I know, the colours are still drifting. Somewhere out there, a digital signage display is showing a shade of purple that started its life as yellow, took a detour through teal on a Tuesday, and arrived at its current hue through a journey that no single person could reconstruct.

Seven formats. Three teams. Zero integers.
