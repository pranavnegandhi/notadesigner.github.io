---
title: "Packaging AIR Native Extensions"
date: 2014-11-18T02:40:50Z
slug: packaging-air-native-extensions
aliases: ["/packaging-air-native-extensions/"]
categories:
  - "Technique"
tags:
  - "actionscript"
  - "native-extensions"
wp_post_id: 813
---

Native extensions are a handy improvement to the AIR ecosystem. There are numerous benefits of being able to to drop into some C code for processor-intensive tasks or extending the Flash API into domains outside of the standard library provided by Adobe.

But anybody who has had to deal with developing their own extensions knows that the packaging process is less than straightforward. It requires maintaining at least two different code trees - one for the native portion of the extension, and the other for the ActionScript library that glues the client application to the native portion. The two projects have to be compiled separately - the ActionScript library through the compc tool, and the native portion with its relevant compiler. Finally, both binary outputs have to be packaged into a single ANE file, which is equivalent in structure and function to a SWC.

In addition to all these, the ADT tool which packages the ANE has its own quirks with relative paths (learned at the school of hard knocks), which are best dealt with by placing all the files required for the extension into a single directory rather than scattered across different directories on the hard drive. Which means copying all the various output binaries and XML files into a single directory every time they are changed.

Running all these activities by hand is time consuming and error prone. However, they are mechanical tasks that computers are well-suited to handle. The following batch script is designed for a Windows build chain that compiles both source trees and packages them into an ANE for consumption into the client application.

1. Delete the previously generated binaries to begin with a thoroughly clean slate.

    ```
    del build/*.*
    
    rmdir build /Q
    ```
2. Create a new designated build directory. This is the directory where your binaries are copied into and packaged into an ANE.

    ```
    md build
    ```
3. Compile the ActionScript library using the compc tool. It becomes much easier if you pass a flex-config.xml file as a parameter to the tool rather than passing every setting as a parameter to the command line.

    ```
    compc -load-config bridge/src/flex-config-lib.xml
    ```
4. Copy the output SWC into the build directory. Alternatively, the compc tool has an -output parameter that can be used to specify the location and filename of the generated binary.

    ```
    copy bridge.swc build
    ```
5. Extract the library.swf file from the SWC generated in the previous step into the build directory. Unfortunately, Windows does not ship with a native command line utility to extract files from an archive. Alternatives such as 7-zip help fill up that gap.

    ```
    7z.exe x build/bridge.swc
    ```
6. Compile the native code using your compiler toolchain of choice.

    ```
    MSBuild.exe NativeLibrary/NativeLibrary.vcxproj
    ```
7. Copy the native runtime library along with a previously authored descriptor.xml into the build directory.

    ```
    copy NativeLibrary/Release/NativeLibrary.dll build
    
    copy descriptor.xml build
    ```
8. Package all these files into an ANE using the ADT tool. Invoke the ADT command from inside the build directory.

    ```
    cd build
    
    adt.exe -package -target ane NativeLibrary.ane descriptor.xml -swc bridge.swc -platform Windows-x86 NativeLibrary.dll library.swf
    ```
