---
title: "Practical Design Patterns in C# – State"
date: 2018-06-11T04:43:00Z
slug: practical-design-patterns-in-c-state
aliases: ["/practical-design-patterns-in-c-state/", "/practical-design-patterns-in-c-strategy/"]
images: ["featured.jpg"]
categories:
  - "Construction"
tags:
  - "c#"
  - "practical-design-patterns"
  - "state"
featured_image: featured.jpg
wp_post_id: 1030
---

The purpose of the state design pattern is to allow an object to alter its behaviour when its internal state changes.

The example of a logging function below is a model candidate for conversion into a state implementation.

The requirements of this function are as follows.

1. Write the log entry to the end of a text file by default.
2. The file size is unlimited by default, denoted by setting the value of the maximum file size to 0.
3. If the value of the maximum file size is greater than 0 then the log file is archived when it reaches this size, and the entry is written to a new empty log file.
4. If the recycle attribute is enabled and the maximum file size is set to a value greater than 0, then the oldest entry in the log file is erased to make space for the new entry instead of archiving the entire log file.

The implementation of the actual mechanism for writing to disk is not relevant in this example. Rather, the focus is on creating a framework that enables selecting the correct mechanism based on the preferences set.

```csharp
public void Log(string message)
{
    if (LimitSize)
    {
        if (file.Length < MAX_SIZE)
        {
            // Write to the end of the log file
        }
        else
        {
            if (Recycle)
            {
                // Clear room at the top of the log file
            }
            else
            {
                // Create a new log file with current time stamp
            }

            // Write to the end of the log file
        }
    }
    else
    {
        // Write to the end of the log file
    }
}
```

The implementation shown above is very na├»ve, tightly coupled, rigid and brittle. However, it is also a fairly common occurrence in most code bases due to its straightforward approach to addressing the problem.

The complicated logical structure is difficult to decipher, debug and extend. The tasks of determining file size limits, recovering disk space, creating a new file and writing to the file are all discrete from each other and should have no dependency between themselves. But they are all interwoven in this approach and add a huge cognitive overhead in order to understand the execution path.

There are multiple conditions being evaluated before the log entry is made. The programmer has to simulate the computer’s own execution path for each statement before even arriving at the important lines related to writing to disk. A couple of blocks are duplicated code. It would be nice if the execution path could be streamlined and duplicate code removed.

### Restructured Code 

An eager approach to selecting the preferred mechanism breaks up the conditional statements into manageable chunks. It is logically no different from having the same code in the Log method. But moving it into the mutator makes it easier to understand by adding context into the logic rather than handing it all in a single giant function.

```csharp
public int MaxFileSize
{
    get
    {
        return _maxFileSize;
    }

    set
    {
        _maxFileSize = value;

        if (0 == MaxFileSize)
        {
            // Activate the unlimited logger
            return;
        }

        if (Recycle)
        {
            // Activate the recycling logger
        }
        else
        {
            // Activate the rotating logger
        }
    }
}

public bool Recycle
{
    get
    {
        return _recycle;
    }

    set
    {
        _recycle = value;

        if (0 == MaxFileSize)
        {
            // Recycling is not applicable when file sizes are unlimited
            return;
        }

        if (Recycle)
        {
            // Activate the recycling logger
        }
        else
        {
            // Activate the rotating logger
        }
    }
}
```

The comments indicate that you’re still missing some code that actually performs the operation. This is the spot where the behaviour is selected and applied based on the state of the object. We use C# delegates to alter the behaviour of the object at runtime.

```csharp
public delegate void LogDelegate(Level level, string statement)

public LogDelegate Log
{
    get;
    private set;
}
```

The Logger class contains private methods that implement the same signature as the Log delegate.

```csharp
private void AppendAndLog(Level level, string message)
{
}

private void RotateAndLog(Level level, string message)
{
}

private void RecycleAndLog(Level level, string message)
{
}
```

When the condition evaluation is completed and the logging method has to be selected, a new instance of the Log delegate is created which points to the correct logging method.

```csharp
if (Recycle)
{
    Log = new LogDelegate(RecycleAndLog);
}
else
{
    Log = new LogDelegate(RotateAndLog);
}
```

This separates the selection of the correct logging technique to use from the implementation of the technique itself. The logging method implementations can be changed at will to fix bugs or extend their features without adding risk to breaking the rest of the code.
