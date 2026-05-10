---
title: "Sharing Code – Static and Dynamic Libraries in ActionScript"
date: 2014-09-18T11:57:46Z
slug: sharing-code-static-and-dynamic-libraries-in-actionscript
aliases: ["/sharing-code-static-and-dynamic-libraries-in-actionscript/", "/libraries-or-bust/"]
categories:
  - "Technique"
tags:
  - "actionscript"
wp_post_id: 748
---

When I was in school, practically nobody had access to the internet. As a result, every student would have two essential items in his toolkit - an encyclopedia and a membership to a library. Homework, projects and supplementary studies were highly dependent on both of them.

Encyclopedias ranged from simple one-book units that specialized in a single subject, to multi-volume tomes spanning practically every topic imaginable (or at least deemed allowably imaginable by its publishers) by a school student. In both cases, the research was already done by someone else. When we were assigned a project on the solar system, nobody expected us to discover the planets in it. The planets remained discovered. All we had to do was read about them and share the information when making a presentation to the class.

### Code Libraries

It is heartening to know that somebody creating computer languages took this to heart and invented the concept of code libraries - units of code written by people who are really good in the domain, then shared with the also rans who just wanted to implement their business applications without knowing the mathematics of relational databases or the bit-juggling that network libraries perform.

Libraries are compiled separately from the application that ends up eventually using them. They are linked to the application through a process that maps function calls in the application code with function addresses in the library. This process, predictably, is called linking.

Code stored in statically linked libraries is copied into the application executable at compile time, resulting an individual copy being created for every application that uses the library. Static libraries become an integral part of the application that uses them and cannot be shared with other applications. If the library is modified in any manner, the application must be recompiled and relinked with the new version in order to utilize those changes.

Dynamic libraries are stored in a shared location, and are linked to the application at runtime by the operating system or other environment controller such as a virtual machine. The metaphor falls apart a bit when it comes to sharing dynamic libraries - in a physical library, only one member can borrow a book at a time, whereas any number of programs can concurrently use the code stored in a dynamic library.

With static linking, it is enough to include those parts of the library that are directly and indirectly referenced by the target executable (or target library). With dynamic libraries, the entire library is loaded, as it is not known in advance which functions will be invoked by applications. Whether this advantage is significant in practice depends on the structure of the library.

Other than the obvious benefit of being able to compile units of code separately, either type of libraries offer their own individual benefits also. Statically linked code guarantees that all required code units are present and compatible with the application. Dynamically linked libraries may be of a different version than what the application requires, or not be present at all. Either case either causes a runtime error, or requires defensive code and reduced functionality in the application. Static linking also simplifies product distribution by reducing the number of files to be shipped and installed.

All said and done, both types of libraries are popular and well-worn concepts in computer programming. Many modern languages support either method, usually both.

### Code Libraries in the Flash Ecosystem

Adobe's MXML compiler offers several parameters for programmers to employ libraries - both static and dynamic - in their code.

Static linking is almost identical to how it is used in conventional languages. Dynamic linking works slightly differently from Windows' Dynamically Linked Libraries and Unix's Shared Objects due to the browser-based operation of the Flash Player. But these differences are at an implementation level only. The concepts remain the same.

#### Static Linking

To statically link a library into an application, the compiler offers the library-path and include-libraries directives.

##### library-path

When a library is specified using the library-path directive, the compiler includes only those assets and classes that are referenced in the application. For example, if the Math library contains IntegerArithmetic and FloatArithmetic for performing arithmetic operations on two separate numeric data types, but the client application only uses integers, the FloatArithmetic class is excluded from the output. This reduces the file size of the application SWF.

```
mxmlc Main.mxml -output=Main.swf -library-path=math.swc
```

##### include-libraries

The include-libraries directive packages the entire library, irrespective of what the compiler thinks is used in the application. This comes in handy in a dynamic language like ActionScript because all method calls do not necessarily get tested for linkages by the compiler.

```actionscript
var classRef:Class = getDefinitionByName("com.notadesigner.math.FloatArithmetic") as Class;
var instance:Object = new classRef();
instance["add"](10.5, 2.5); // Linkage not tested by the compiler
```

The include-libraries directive is used like this.

```
mxmlc Main.mxml -output=Main.swf -include-libraries=math.swc
```

##### static-link-runtime-shared-libraries

This directive is a convenient shortcut to convert runtime shared libraries into static libraries without significant changes to the compiler configuration files. Simply setting its value to true causes the compiler to embed all RSLs into the application. This is a handy way to investigate library-related problems when debugging the application.

```
mxml Main.mxml -output=Main.swf -static-link-runtime-shared-libraries=true
```

#### Dynamic Linking

A Runtime Shared Library (commonly abbreviated to RSL) is loaded by the application before it begins execution. Loading a RSL is a complicated task if done in plain ActionScript, but is taken care of automatically if the application builds on the Spark or MX Application class.

Adobe-provided framework libraries are the most obvious use case for using RSLs. All Flex-based applications depend upon these files. By caching them after the first time they are downloaded, future applications can be started much faster as they do not need to download the same code again. Custom libraries can also take advantage of the same technique.

Flash libraries are compiled using the compc utility, that ships as part of the Flex SDK. It generates a SWC file which is a compressed archive containing a SWF (named library.swf) and an XML (catalog.xml). To use this library, the developer must manually extract the SWF from the SWC using an archival utility (such as PKZip) and place it where the application can download it at runtime. As a good practice, the SWF is also usually renamed from library.swf to something more meaningful.

##### runtime-shared-library-path

This directive is used to specify the location of RSL files for the compiler. The compiler requires the names of both, the SWC as well as the extracted SWF, separated by a comma.

```
mxmlc -o=main.swf -runtime-shared-library-path=libs/math.swc,bin/math.swf Main.mxml
```

#### Related Directives

The Flex compiler provides two other directives to externalize the application's assets and code fragments. The compiler tests linkages against these assets at compile-time, but leaves them out of the application binary. Libraries that contain these assets or classes are required at runtime, and the Flash Player throws a runtime error if they are not available.

These directives are useful when creating modules which are loaded dynamically into an application that they share code with. The application is linked to the requisite RSL, and the module does not need to fetch it again. However, the compiler still needs to test the linkages against the symbols - either against the original source code or a compiled binary. These directives assist in that task.

##### external-library-path

The compiler uses SWC files specified at these paths to test linkages with the application code, but does not compile the binaries into the application itself.

##### externs

This directive points to source files containing assets or classes which will be available at runtime. The compiler uses the source files for link testing, but does not compile them into the application binary.
