---
title: "What’s in a Name?"
date: 2016-04-14T04:36:47Z
slug: whats-in-a-name
aliases: ["/whats-in-a-name/"]
categories:
  - "Construction"
wp_post_id: 860
---

Naming has been acknowledged in many texts as a difficult aspect of practical computer programming. The problem with assigning appropriate names for entities is that it is not quite as cut and dry as most other aspects of the craft. The rules are not very objective. Choosing a good name requires applying a certain amount of heuristics drawn out of experience and intuition.

Good names are easy to overlook because they stay out of the way. But a smart programmer can learn much about the kind of naming schemes to avoid simply by having to maintain code with poor quality naming conventions. Badly named variables can make this task so difficult that there might be some merit in the idea of exposing programmers to such a maintenance exercise specifically to teach them this valuable skill.

We look over a simple example of how poor naming conventions inhibit the design, and conversely, how a well chosen name can make short work of extending the application in the future.

```actionscript
Wheel buildWheel(int spokeCount);
```

This signature declares a method that takes an integer spoke count as input and returns a Wheel instance. Fred calls the method in the following manner.

```actionscript
Wheel front = buildWheel(32);
```

This gets flagged by his pair programmer Dave for using a magic number. So Fred changes the code.

```actionscript
const int THIRTY_TWO = 32;

Wheel front = buildWheel(THIRTY_TWO);
```

It isn't the most elegant snippet they have encountered. But both Fred & Dave agree that this code no longer uses a magic number. So they commit the code and go home.

Two years later, the specifications change. The client can now build lightweight wheels with just 16 spokes. Fred is no longer on the team. Susan, the maintenance programmer, takes up the task and inspects the code. She finds the line where the number of spokes is declared and edits it.

```actionscript
const int THIRTY_TWO = 16;

Wheel front = buildWheel(THIRTY_TWO);
```

Technically, the code is fixed. But it has deteriorated a bit more in quality and maintainability.

A few months later, the client sees resurgent demand from their customers for high-spoke count wheels due to their durability and higher resilience to handle bad surfaces. So they wish to make the number of spokes a customisable option that can either be 32 or 16. Two weeks into the modification they also release a new 12-spoke wheel. The code now looks like this.

```actionscript
const int THIRTY_TWO = 16;
const int THIRTY_TWO_TRUE = 32;
const int TWELVE = 12;

Wheel front;
switch (wheelType)
{
    case DURABLE:
        front = buildWheel(THIRTY_TWO2);
        break;
    case LIGHTWEIGHT:
        front = buildWheel(THIRTY_TWO);
        break;
    case ULTRA_LIGHTWEIGHT:
    default:
        front = buildWheel(TWELVE);
        break;
}
```

This is a very minor, localized problem in the code. It has relatively negligible impact on the performance of the application. Dozens of minor warts such as this fester in any sizeable code base. Programmers are human too, and make mistakes sometimes. Maybe the deadline inched too close and they had to roll out *right that minute*. The key is to identify and fix those code smells as soon as they're encountered. Queuing them up for a grand refactoring project risks turning these minor issues into full-blown architectural challenges.

Things often get trickier when such names are used in giant mudballs with extremely wide scope, such as singleton objects or global buckets. Nobody wants to read the code surrounding all 28 references to the constant THIRTY\_TWO, without which making the change would be extremely hazardous. So THIRTY\_TWO continues to live on in the code base, forever doomed to contain just half of what it denotes.

This type of problem can be nipped in the bud very easily by simply spending a moment thinking about the names of objects. In this case, the method signature itself provides a hint for the correct name of the parameter - spokeCount. Then Fred's iteration would have looked like this.

```actionscript
const int SPOKE_COUNT = 32;
 ...
```

When Susan comes in to add two new spoke count options, she realizes that the constant SPOKE\_COUNT already exists. This nudges her into defining two more constants - LOW\_SPOKE\_COUNT, MID\_SPOKE\_COUNT. If Susan uses a reasonably modern IDE, it would also allows her to rename the old constant to HIGH\_SPOKE\_COUNT.

```actionscript
const int HIGH_SPOKE_COUNT = 32;
const int MID_SPOKE_COUNT = 16;
const int LOW_SPOKE_COUNT = 12;
```

Suddenly, by simply choosing a name that better ***reflects the purpose of the entity*** rather than its value, the application programmer has made its intended usage clearer, and nudged future maintenance programmers towards using similar appropriate names to new code that they write.
