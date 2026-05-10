---
title: "How to Write Unmaintainable Code – ASP.NET Redux"
date: 2018-03-19T01:06:00Z
slug: how-to-write-unmaintainable-code-asp-net-redux
aliases: ["/how-to-write-unmaintainable-code-asp-net-redux/"]
images: ["featured.jpg"]
categories:
  - "Technique"
tags:
  - "asp.net"
  - "c#"
  - "coding-horror"
featured_image: featured.jpg
wp_post_id: 1027
---

No matter how far technology progresses, it seems that we still remain bound to the past by an innate ability of writing poorly structured programs. To me, this points to a rot that is far deeper than languages and platforms. It is a fundamental failure of people who claim to be professionals to understand their tools and the principles that guide their usage.

It has been eight years since I wrote [the previous piece in this series that demonstrated poorly written PHP code](/how-to-write-unmaintainable-code-php-redux). The language gets a bad rap due to the malpractices that abound among users of the platform. But this was a theme I was hoping would be left behind after graduating to the .NET framework in the past few years.

It turns out that I was wrong. Bad programmers will write bad code irrespective of the language or platform that is offered to them. And the most shocking bit is that so many of the points from the previous article (and the original by Roedy Green) are still applicable, that it feels like we learned nothing at all.

### Reinvent the wheel again. Poorly.

Maintainable code adheres to standards – industry, platform, semantics, or just simply internal to the company. Standard practices make it easy to build, maintain and extend software. As such, they’re anathema to anybody who aims to exclude newcomers from modifying his program.

Therefore, ignore standards.

Take the case of date and time. It is 2018, and people want to and expect to be able to use any software product irrespective of their personal regional settings.

Be merciless in thrashing their expectations. Tailor your product to work exclusively with the regional settings used on your development computer. If you are using the American date format, say you’re paying homage to the original home of the PC. If you’re using British settings, extol upon the semantic benefits of the dd-mm-yy structure over the unintelligible American format.

Modern programming platforms have a dedicated date and time data type precisely to avoid this problem. Sidestep it by transmitting and storing dates as strings in your preferred formats (there doesn’t have to be just one). That way, you also get to scatter a 200-line snippet of code to parse and extract individual fields from the string.

For extra points, close all bug reports about the issue from the test engineers with a “Works for me” comment. Your development computer is the ultimate benchmark for your software. Everybody who wishes to run your program should aspire to replicate the immaculate state of existence of your computer. They have no business running or modifying your program otherwise.

Never acknowledge the presence of alternative universal standards.

### Ignorance is bliss

Nobody writes raw C# code if they are going to deploy on the web. A standard deployment of ASP.NET contains significant amounts of framework libraries that enable the web pipeline and extensions to work with popular third-party tools. Frameworks in the ecosystem are a programming language unto themselves, and require training before use.

Skip the books and dive into writing code headfirst.

Write your own code from scratch to do everything from form handling to error logging. Only n00bs use frameworks. Real programmers write their own frameworks to work inside of frameworks. This gives rise to brilliant nuggets such as this.

```csharp
public class FooController
{
    ...
    public new void OnActionExecuting(ActionExecutingContext filterContext)
    {
    }
    ...
}
```

By essentially reinventing the framework, you are the master of your destiny and that of the company that you are working for. Each line of custom-built code that you write to replace the standard library tightens your chokehold on their business, and makes you irreplaceable.

### Allow unsanitised input

Protecting from SQL injection is difficult and requires constant vigilance. If everything is open to injection, the maintenance programmer will be bogged under the sheer volume of things to repair and hopefully, either go away or be denied permission to fix it due to lack of meaningful effort estimates.

Mask these shortcomings by only writing client-side validation. That way, the bugs remain hidden until the day some script kiddie uses the contact form on the site to send “; DELETE TABLE Users” to your server.

### Try...catch...swallow

Nobody wants to see those ugly-ass “Server Error” pages in the browser. So do the most obvious thing and wrap your code in a try-catch block. But write only one catch handler for the most general exception possible. Then leave it empty.

This becomes doubly difficult to diagnose if you still return something which looks like a meaningful response, but is actually utterly incorrect. For example, if your method is supposed to return a JSON object for the active record, return a mock object from the error handler which looks like the real thing. But populate it with empty or completely random values. Leave some of the values correct to avoid making it too obvious.

Maintenance programmers have no business touching your code if they do not have an innate ken for creating perfect conditions where errors do not occur.

### String up performance

Fundamental data types such as strings and numbers are universal. Especially strings. Therefore, store all your data as strings, including obvious numeric entities such as record identifiers.

This strategy has even more potential when working with complex data types containing multiple data fields. Eschew standard schemes such as CSV. Instead come up with your own custom scheme using uncommonly used text characters. The Unicode standard is very vast. I personally recommend using pips from playing cards. The “♥” character is appropriately labelled “Black Heart Suit”, because it lets the maintenance programmer perceive the hatred you bear towards him for attempting to tarnish the pristine beauty of the code you have so lovingly written.

This technique also has a lot of potential in the database. Storing numeric data as strings increases the potential for writing custom parsers or making type-casts mandatory before the data can be used.

### Use the global scope

Global variables are one of the fundamental arsenal in the war against maintainable code. Never fail an opportunity to use them.

JavaScript is a prime environment for unleashing them upon the unwary maintenance programmer. Every variable that is not explicitly wrapped up inside a function automatically becomes accessible to all other code being loaded on that page. This is an increasingly rare scenario with modern languages. The closest it can be approximated in C# is to have a global class with several public properties which are referenced directly all over the application. While it looks the same, it is still highly insulated. Try these snippets as an example.

JavaScript –

```javascript
var a = 0; // Variable a declared in global scope
 
function doFoo() {
    a++; // Modifies the variable in global scope
}
 
function doBar() {
    var a = 1;
    a++; // Modifies the variable in local scope
}
```

C# –

```csharp
public class AppGlobals
{
    public int A = 0;
}
 
public class Foo
{
    public void DoSomething()
    {
        // Scope of A is abundantly clear
        AppGlobals.A++;
        var A = 0;
        A++;
    }
}
```

It is very easy to overlook the scope of the variable in JavaScript if the method is lengthy. Use it to your advantage. Camouflage any attempts to detect the scope correctly by using different conventions across the application. Modify the global variable in some functions. Define and use local variables with the same name in others. Modifying a function must require extensive meditation upon it first. Maintenance programmers must achieve a state of Zen and become one with your code in order to change it.

### Use unconventional data types

Libraries often leverage the use of conventions to eliminate the need to write custom code. For example, the Entity Framework can automatically handle table per type conditions if the primary key column in the base class is an identity column.

You can sideline this feature by using string or UUID columns as primary keys. Columns with these data types cannot be marked as identity. This necessitates writing custom code to operate upon the data entities. As you must be aware by now, every extra line of code is invaluable.

### Database tables without relationships

If you are working at a small organisation, chances are there is no dedicated database administrator role and developers manage their own database. Take advantage of this lack of oversight and build tables without any relationships or meaningful constraints. Extra points if you can pull it off with no primary keys or indexes.

Combine this with the practice of creating and leaving several unwanted tables with similar names to give rise to a special kind of monstrosity that nobody has the courage to deal with. For still extra marks, perform updates in all the tables, including the dead ones. Fetch it from different tables in different parts of the application. They cannot be called unwanted tables if even one part of your application depends on them. Call it “sharding” if anybody questions your design.

### Conclusion

This post is not meant to trigger language wars. Experienced developers have seen bad code written in many languages. Some languages are just more amenable to poor practices than others.

The same principle applies to the .NET framework, which was supposed to be a clean break from the monstrosities of the past. On the web, the ASP.NET framework and its associated libraries are still one of the best environments I have used to build applications.

That people still write badly structured code in spite of all these advances cements my original point – bad programmers write bad code irrespective of the language thrown at them.
