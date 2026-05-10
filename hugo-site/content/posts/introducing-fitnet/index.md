---
title: "Introducing FitNet"
date: 2016-12-01T14:26:42Z
slug: introducing-fitnet
aliases: ["/introducing-fitnet/"]
categories:
  - "Demonstration"
tags:
  - "c#"
  - "fitnet"
  - "wpf"
wp_post_id: 977
---

The recent surge in fitness-related technology innovations bodes well for the future of many people. And though the Cheeto-munching, Red Bull-guzzling stereotype for the technologists behind this progress still sticks, the truth is that more people, including programmers, are gaining awareness of their physical health and nutrition and changing lifestyles to seek improvement.

Latching on to this bandwagon came a surge of fitness tracking apps. FitNotes, StrongLifts 5x5, the Starting Strength App by legendary strength-training coach Mark Rippetoe, Strong App for iOS, and many more, provide ample choice to the enthusiast as well as advanced lifter. These apps cover various aspects, from simple tracking, to full blown programme design with progressions and periodisation. Some applications work online. Your data is stored on their server, and requires an internet connection to access and update. Others are offline, or a hybrid of both modes. Many are free and use ad revenue to sustain operations. Some of the fancier ones are paid. They all have their pros and cons.

Having used a small subset of them in the past few years, I have identified certain shortcomings between them.

#### Incompatible Storage

This is especially annoying because most of these applications use the same database engine - SQLite. But it's impossible to move from one application to another without a lot of manual re-entry of data.

Now this is an understandable shortcoming. Database design is an engineering activity. People approach the same problem differently based on their skill levels and priorities. Copying databases between applications will never work directly. But the problem can be mitigated to a great extent by the industry agreeing to a common minimum format that they all support, which can be used to send data out of the application. Customers do not like being locked in.

#### Changing Platforms

Mobile phones change often. I have found myself switching between Blackberry to Android to iOS in a few short years. Fitness journeys last a lifetime. Phones last a few years, at best.

This is to some extent, a manifestation of point 1 above. If the common minimum format were to come together, changing platforms would become much easier. However, there's another more subtle point. The one unchanged platform over all these years has been the desktop and the Windows operating system. People usually have access to at least one Windows computer. Even though Android has become the most popular operating system, the Windows desktop remains firmly lodged into place at workplaces, schools and homes. It's still a very stable and dependable platform to target.

#### Privacy

It is possible to cast all these troubles away and switch to a web-based application. However, people are not always comfortable with storing sensitive personal information on a server that is not in their control. Not to mention the risk of the company going belly-up and taking their precious PR logs along with it.

This is a no-brainer. Put the data on the user's own computer, where they do not have to worry about losing their privacy. Include an automated backup feature that saves the encrypted data on their cloud-storage service. The data remains reasonably safe from loss as well as from prying eyes. Again, a common minimum format can prove to be very useful in the worst case scenario of 100% loss of the physical computer itself.

These were real and personal pain-points I experienced first-hand. And being a programmer gave me the ability to actually work on a solution that addresses these problems.

### Introducing FitNet

The easiest replacement to these woes was a spreadsheet. Smack a new row for every workout, see real-time graph updates. It works as a log, but getting a report out of it is a chore. Besides, programming fancy reports into a spreadsheet eventually turns it into a prime feature for The Daily WTF. So that was out. Microsoft Access used to be a very good tool for this kind of tasks, but it still leaves out graphical reports. And the new monthly subscription model for Office 365 just isn't for me.

#### Enter Windows Presentation Foundation.

I have been using Windows Forms since the longest time as the tool of choice for building desktop applications on Windows. It's easy and it's straightforward, and anybody who's working with the previous Windows API feels perfectly at home with the new managed API that WinForms represents.

Windows Presentation Foundation is different. It is not just a wrapper around the Windows API. It's a whole different framework which eschews the disparate development sub-frameworks from the Windows API in favour of a single well-integrated and extensible library. It also accounts for differing device capabilities in a more resilient fashion by abstracting away tasks like drawing objects relative to the device resolution and hardware accelerated graphics available out of the box without any extra effort. Finally, WPF brings a new type of resource - a XAML file - to be used as a declarative approach to building and programming user-interface elements rather than the procedural approach required by the Windows API.

This means that the following 50 lines of code –

```cpp
WNDCLASSEX wc;
HWND hwnd;
MSG Msg;

// Registering the Window Class
wc.cbSize        = sizeof(WNDCLASSEX);
wc.style         = 0;
wc.lpfnWndProc   = WndProc;
wc.cbClsExtra    = 0;
wc.cbWndExtra    = 0;
wc.hInstance     = hInstance;
wc.hIcon         = LoadIcon(NULL, IDI_APPLICATION);
wc.hCursor       = LoadCursor(NULL, IDC_ARROW);
wc.hbrBackground = (HBRUSH)(COLOR_WINDOW+1);
wc.lpszMenuName  = NULL;
wc.lpszClassName = g_szClassName;
wc.hIconSm       = LoadIcon(NULL, IDI_APPLICATION);

if (!RegisterClassEx(&wc))
{
  MessageBox(NULL, "Window Registration Failed!", "Error!",
MB_ICONEXCLAMATION | MB_OK);
return 0;
}

// Create the window
hwnd = CreateWindowEx(
  WS_EX_CLIENTEDGE,
  g_szClassName,
  "FitNet Desktop",
  WS_OVERLAPPEDWINDOW,
  CW_USEDEFAULT, CW_USEDEFAULT, 400, 300,
  NULL, NULL, hInstance, NULL);

if (NULL == hwnd)
{
  MessageBox(NULL, "Could not create application window.", "Error", MB_ICONEXCLAMATION | MB_OK);

  return 0;
}

ShowWindow(hwnd, nCmdShow);
UpdateWindow(hwnd);

// Message loop
while(GetMessage(&Msg, NULL, 0, 0) > 0)
{
  TranslateMessage(&Msg);
  DispatchMessage(&Msg);
}

return Msg.wParam;
```

– can be distilled down to –

```xml
<Window x:Class="WpfApplication1.Window1"
xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
Title="FitNet Desktop" Height="400" Width="300" />
```

Now it can be argued that the Windows Forms API also offers similar succinctness. But WPF takes this to a whole different level as I learned through the course of my project.

This series of articles is an ongoing log of my journey of building FitNet - a fitness tracker that meets my own needs.
