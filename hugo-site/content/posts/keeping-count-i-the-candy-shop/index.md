---
title: "Keeping Count I – The Candy Shop"
date: 2006-09-22T05:29:16Z
slug: keeping-count-i-the-candy-shop
aliases: ["/keeping-count-i-the-candy-shop/"]
categories:
  - "Construction"
wp_post_id: 35
---

Its been quite a while since I wrote about a serious programming-specific problem. In this two part series I will show you a popular database normalization method, and an alternative use for delegates. All code is written in Flash ActionScript and is purely illustrative.

Betty Green runs a candy shop that is wildly popular with the neighbourhood kids. The sweets she sells are sold by weight or by piece, depending upon the type. For example, peppermints are sold by weight, while chocolate bars are sold by the number of bars purchased. Being a good manager, Betty also keeps a register to track the amount of items of each type that she’s sold. At the end of the day she totals up the register and updates her inventory for the next day.

The system itself is quite good, but Betty would prefer that she didn’t have to wait till the end of the day to check out on which items she’s running low on, because then it means that her supplier can be notified only the next day. If she could let him know sooner, then she could stock up again on the same day and not lose customers.

Betty has received a new computer on her birthday from her aunt, which she feels can be used to good effect in her store. Daryl, a friend of hers, is a computer geek of sorts who has offered to write a software application for her billing and inventory management. He offers her many snazzy features such as automatic SMS order placement to her supplier when inventory falls low, a digital gallery of candies that she can display on an LCD ticker outside her store and of course, email. But what really gets her attention is a boring feature called inventory management. That is, the computer keeps track of her inventory and can give her updates after every sale, which allows her to place orders immediately if stock runs low.

So Daryl gets down to work. One thing that keeps nagging him is that inventory is to be maintained in two different units - grams and number of items. In her book-register, Betty used to draw two columns - one for weight and one for pieces. Whenever a sale was made, she’d fill in the appropriate column based upon the type of sweet she sold. Now, why should the database care how she sells her sweets? That is something that only Betty needs to know when she makes inventory. Daryl designs his database with a single UnitsSold column, in which he stores the number of units of each sale. His application interprets the sale units depending upon the type of sweet and displays the value with the appropriate unit symbols.

This is a simple illustration of an extremely powerful concept in data processing. All data are eventually converted to integers for the processor to work upon. By understanding how those numbers are encoded for abstract data types, you not only understand what’s going on behind the scenes, you can also drop down into the primitive level to perform operations that are not supported on abstract data types. For example, bitwise operators will balk at strings, but will gladly accept integers.

You can easily tell when a person doesn't understand this, because their tracking database for fifty different activities will contain fifty columns, some of which contain numeric values, some floating point, some boolean and some time. If new activities have to be added, they will add another column to the table and replace the database files. Everything looks okay until someone logs in the next time after the update and finds that their previous scores are all gone. Whoops!

In my next article I’ll explain how multiple data types can be parsed efficiently at runtime, without rewriting too much code. Stay tuned.
