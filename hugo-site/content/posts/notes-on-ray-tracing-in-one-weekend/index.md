---
title: "Notes On Ray Tracing in One Weekend"
date: 2023-06-29T03:22:46Z
slug: notes-on-ray-tracing-in-one-weekend
aliases: ["/notes-on-ray-tracing-in-one-weekend/"]
images: ["og-image.jpg"]
categories:
  - "Construction"
  - "Software Engineering"
tags:
  - "c#"
featured_image: featured.jpg
wp_post_id: 3285
---

Peter Shirley's [Ray Tracing in One Weekend](https://raytracing.github.io/books/RayTracingInOneWeekend.html) had been on my reading list since a long time. Based on brief incursions into its first few chapters, I was aware of the intensive hands-on approach that the book takes to explain its subject matter.

In addition to the mathematics, this also became a solid opportunity to hone my skills with C# because of several forays into implementing concepts from first-principles. I also encountered some language-specific peculiarities which are uncommon in web development, or as a result of translating the C++ code from the book to C#.

This blog post contains an ongoing list of lessons learned during this exercise.

### Console output can be redirected to a file

Now this I knew from earlier, but I'm adding it here for sake of completeness. Anything that the program prints to the console (through `Console.Write()` or `Console.WriteLine()`) can be saved to a file by using the > operator in PowerShell (or most other shells).

```powershell
MyApp.exe > output.log
```

The commonly used redirection operators are >, >> and N>, which usually present in all shells, across platforms. PowerShell supports additional techniques to pipe output to files using the Out-File and Tee-File cmdlets.

### The C# equivalent to std:cerr is Console.Error

The use of stderr is common in several environments because of its convenience and ubiquity. I've seen it less often in colloquial C# for runtime logging, where the `ILogger` interface has far more traction.

The `Console.Error` is a `TextWriter` instance that prints to a console by default. And it can be changed to point to any other stream, such as a file or a network endpoint.

```
Console.Error.WriteLine("The Thing Broke");
```

But it's also possible to use the > operator in PowerShell to redirect the standard error output to any other destination, which is far more convenient and flexible.

### The unary minus (-) operator negates a value

Easy-peasy. Standard mathematics rules apply. The language has its own rules though, which must be adhered to.

1. The operator takes only 1 parameter (hence, unary).
2. The parameter must be the same type that contains the operator declaration.

### The unary \* operator does not multiply a number by itself

This was my "Doh!" moment. It's a pointer dereference operator, of course. I had a temporary brain fade when I read this in the book at first. But the documentation set things right.

### Compound assignment operators cannot be explicitly overloaded

All compound assignment operators are implicitly overloaded by overloading their corresponding binary operator. These operators are +=, -=, \*=, /=, %=, &=, |=, ^=, <<=, >>= and >>>=.

### The in modifier on a parameter is a poor man's substitute for const

Applying the `in` modifier on a parameter makes it a read only reference. The compiler does not prevent the invocation of mutating methods on an in parameter, but the instance is not modified.

```csharp
public class Apple
{
  public int Size;

  public void Bite()
  {
    Size -= 1;
  }
}
```

```csharp
public void Program
{
  public static void Main()
  {
    var fruit = new Apple();
    Mutate();
  }

  private static void Mutate(in Apple fruit)
  {
    // fruit.Size--; // Won't compile
    fruit.Bite(); // Compiles and runs on a copy of fruit
  }
}
```

When the `Bite()` method is called, it transparently operates on a copy of the original object. The value of `Size` is never altered in the original instance. This can introduce subtle bugs for the unwary programmer. The `const` keyword in C++ comes with much stronger guarantees of immutability by completely preventing the invocation of mutable methods.

And remember, property getters and setters in C# are methods under the hood.

### C# 10 adds support for global aliases

Declaring an alias allows the programmer to step around class name conflicts when using unalterable code (such as third-party libraries). Earlier versions of the language required that the alias be declared separately in each file where it was to be used. This is no longer necessary. The `global` modifier makes the alias available across the entire project.

```csharp
global using NativeScrollBar = System.Windows.Forms.HScrollBar;
global using LibraryScrollBar = Vendor.Library.HScrollBar;

var a = new NativeScrollBar();
var b = new LibraryScrollBar();
```

This feature comes in handy when declaring the `Color` and `Point3` types as aliases for the `Vec3` type during the course of the book.

### Method inlining is not guaranteed

Inline expansion is an optimisation technique that copies the body of the function to the call site. The compiler will almost always do a better job at selecting methods to inline. Besides, C# does not have forced inlining. The JIT heuristics will always be the final authority to determine whether a method should be inlined or not, even when the programmer has explicitly decorated a method with the `MethodImplAttribute`. So it's best to leave it out from your code.

### Convert.ToInt32() is different from static\_cast<int>

This was a significant stumbling block that had a material change on the colour output. In C++, the statement `static_cast<int>(255.999)` truncates the decimal digit, returning the value 255. The `Convert.ToInt32()` method in C# rounds to the nearest integer. So `Convert.ToInt32(255.999)` results in 256, which is outside the 8-bit range of allowed values for each RGB channel. This can give rise to some weird outputs, ranging from wildly incorrect colours, to mosquito noise patterns.

`Math.Truncate()` could have been used instead, because it truncates the decimal portion of the number and returns only the integral part. But the return type of this method is still a decimal, which has to be cast to an `int`.

Eventually, the solution turned out to be simpler than anticipated. Since casting to `int` also discards the decimal portion, the call to `Math.Truncate()` can be excluded, and the type can be directly cast to an `int`.

```
(int)255.999; // Returns 255
```
