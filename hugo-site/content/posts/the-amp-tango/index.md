---
title: "The AMP Tango"
date: 2007-08-10T05:20:35Z
slug: the-amp-tango
aliases: ["/the-amp-tango/", "/29/"]
categories:
  - "Technique"
tags:
  - "apache"
  - "mysql"
  - "php"
wp_post_id: 29
---

I have recently moved to a smaller city that can’t quite boast of the public transport system that Mumbai provides to its citizens. It does exist on paper, and when you go out you do have the option of trying to locate a public utility bus or train. But that task is so difficult that most people cave in to the temptation of flagging an auto rickshaw. Even if it costs up to ten times the price of a bus.

Which brings us to another problem that the people of Mumbai never have to face - auto rickshaw drivers often quote a fixed price to take you to your destination. That figure doesn’t always correspond to what the meter might have indicated had it been used. So you can either yield to their demands while feeling frustrated at being robbed in broad daylight, or haggle with them to bring them down to what you might consider a fair price. And if you don’t like haggling too much, the experience isn’t going to be very pleasant.

I spent the afternoon haggling with some rather brilliant code, trying to coax it to work as it was advertised it would. I’m documenting the efforts I went through out here so that I can reduce some of the agony the next time I have to do this.

### A Brief Background

Developers and software managers everywhere laud the power and elegance of the Apache, MySQL and PHP stack. Several commercial outfits have built their entire business model around them. I have been going through the ritual of installing these three on Windows for a long time now and yet, I stumble every time I try to integrate them.

Installing Apache is a cinch. The developers have packaged it into a very nice installer that sets it up on your system within minutes. The default settings are fine enough for almost anybody who does not want to play system administrator. And you don’t even need to restart your system in most cases.

Integrating PHP on top of Apache used to be a bit of a task, but lately it’s been becoming easier. I had to add 3 lines to the Apache httpd.conf file to get it to work. There is a huge dependency on the PHP initialization file, php.ini, but the risk is reduced because the default settings in that file too are sufficient for a typical developer machine.

MySQL too comes with its own installer that takes away most of the pain from the setup process.

### The Problem

But this is where the ease of use disappears. Making MySQL work with the Apache and PHP stack always seems to be a pain. Since I don’t do this very often, I can’t remember the integration recipe very well. I’m usually not even sure if I faced the same problems the last time around - much less the resolution.

This article is a guideline for me to follow the next time I have to go through installing these applications. The problem I faced and its resolution is still fresh in my mind since I only stepped through it this afternoon.

It all started when I attempted to load the php\_mysqli.dll extension to integrate MySQL into the stack. For extensions to load, PHP needs to be given the path to the folder where the .dll files are stored. The default location is the “ext” folder under the PHP root folder. This path is defined in the variable “extensions\_dir” in php.ini.

For some reason the path was not interpreted correctly by PHP on start-up. The Apache error log kept showing the following error -

PHP Warning: PHP Startup: Unable to load dynamic library 'E:\\apps\\php-5.2.3\\ext\\php\_mysqli.dll' - The specified module could not be found.\r\n in Unknown on line 0

The double backslashes in the path baffled me because the value in php.ini was definitely not typed that way. It is a common convention to escape certain characters by placing the backslash before it, so I felt that this was mangling the path.

### Resolutions

I searched the web and found that many people have faced this issue. The solutions proposed were varied.

Some suggested that the library file libmysql.dll bundled with PHP 5.2.3 was buggy. I replaced it with the recommended earlier version (from PHP 5.2.1) but the error still persisted. Others suggested that the extensions that were needed could be moved to the System32 folder, so that they would be found by the PATH system variable. But I refused to corrupt my OS folders with such unnecessary files.

The obvious other solution was to add the extensions folder to my system path, which I duly did. But to my surprise the extension still didn’t load. I checked and rechecked that the folder was included in the system path.

By chance I happened to run a script in my browser that called the phpinfo() function. Browsing through the values, I came upon the Apache Environment section that described how Apache interacted within the OS. And that’s where I found the problem. For some reason, even after a restarting the service, Apache continued to maintain the old system variables in its memory. Because of this, PHP didn’t know where to search for extensions and kept returning the same error.

Restarting Windows solved the problem.

I still haven’t figured out why the “extensions\_dir” variable doesn’t work as expected. I have tried various combinations of front- and backslashes, besides experimenting with several relative and absolute path names without any luck. Backslashes are always escaped in the error log, although front slashes are left as-is.

In all, I learnt that restarting your machine does often solve problems - not through magic but through simply realigning different portions of memory that may be connected in some obscure manner. Not bad for an afternoon.

### WAMP Installation Checklist

For the benefit of those struggling with the kind of problems I faced, here’s the complete checklist to follow when installing the AMP stack on Windows.

1. Download and install Apache
2. Download the PHP archive and unzip into a folder of your choice
3. Add the following line to httpd.conf depending upon the version of Apache installed -LoadModule php5\_module "E:/apps/php-5.2.3/php5apache2.dll"
or
LoadModule php5\_module "E:/apps/php-5.2.3/php5apache2\_2.dll"
4. Add the following line to httpd.conf -AddType application/x-httpd-php .php
5. Add the following line to httpd.conf -PHPIniDir "E:/apps/php-5.2.3"
6. Add the PHP folder to the Windows PATH variable.
7. Restart your system after completing the last step to make sure that Apache refreshes the environment values. For some reason this does not happen simply by restarting Apache.
8. Download and install MySQL.
9. Change the extension\_dir variable in php.ini to point to the complete path to the extensions directory.
10. Add the following line to php.ini -extension=php\_mysqli.dll
11. In some situations phpMyAdmin also requires the php\_mcrypt.dll and php\_mbstring.dll extensions enabled. If needed, uncomment their loader lines in php.ini
12. Copy libmcrypt.dll to %SYSTEM% folder if you need the php\_mcrypt.dll extension to work.
