---
title: "How to Write Unmaintainable Code – PHP Redux"
date: 2010-02-21T05:36:15Z
slug: how-to-write-unmaintainable-code-php-redux
aliases: ["/how-to-write-unmaintainable-code-php-redux/"]
categories:
  - "Technique"
tags:
  - "coding-horror"
  - "php"
wp_post_id: 46
---

How to Write Unmaintainable Code is a satirical piece of programming advice written by Roedy Green a long time ago. That it remains his most popular essay till date speaks volumes about the programming skills of human society as a whole. Collectively, we are either a massive bunch of shit-chucking apes who take this essay seriously, or a small bunch of highly skilled developers seeking relief from daily disillusionment in our work behind its biting humour.

I hope to hell that it’s the latter.

Green’s essay is written with Java and C programmers in mind, but many of the techniques described can be applied to any programming language. Consider this essay as a “lite” version of Green’s to keep us script kiddies from feeling left out.

### Write code. Or markup. Or both. Together.

PHP has a very unique ability of allowing the programmer to intersperse program code with HTML markup in the same file by using special PHP tags to identify code blocks. And as with all things PHP, there is more than one way to skin a cat.

The "old style":

```php
<? echo "Hello, World!"; >
```

The "first new style". Note the added question mark in the end tag:

```php
<? echo "Hello, World!"; ?>
```

The "second new style" is similar to the first new style and most recommended:

```php
<?php echo "Hello, World!"; ?>
```

What else? The "third new style" of tags:

```html
<script language="php">
    echo "Hello, World!";
</script>
```

And finally, a fourth way that uses ASP-style tags:

```php
<% echo "Hello, World!"; %>
```

This in itself opens whole new avenues for abuse for a programmer used to the sharper separation of code and presentation enforced in a more serious language. What’s worse is that the tag rules can be changed on individual systems through the php.ini file. Develop with all style flags enabled. Tell your colleagues that you’re doing it in the interest of maximum compatibility with third-party libraries. For added fun, find (or write your own) library that liberally switches between all five types of tags and make it an indispensable part of your project. Or keep switching coding standards every few weeks until there’s a healthy mix of all types of PHP tags scattered throughout your code. Refuse to replace previously used styles with the current favourite because it’s better to leave well enough alone.

Do not document this anywhere.

This is in the interest of avoiding single-step deployments on a stock (or sane) PHP installation. Force system administrators to find the problem in your code and then comb through the php.ini file for the appropriate settings. Always say that you have security in mind, which is encouraged by having the system administrator read each line of the settings file.

But the ability of the language to intersperse HTML markup even inside if...else or loop blocks evidences the sheer the heinousness that must have enveloped the language designers when they wrote that bit of the specification.

There are so many ways to abuse this “feature”.

A tamer way would be –

```html
<?php if (foo == null): ?>
    <p> Invalid foo</p>
<? elseif (foo == 1): ?>
    <p>Foo is too small.</p>
<? else ?>
    <p>Correct foo entered</p>
<?php endif ?>
```

A more advanced programmer might use something like this –

```html
<div class="query_output">
    <?
        $sql="select * from names";
        if (db_connect()): ?>
            <p>Error!</p>
        <?php else: ?>
            <p><strong>Names</strong></p>
        <?
            $res = db_query($sql);
            $names = db_result($res);
            foreach ($names as $names): ?>
                <!--span><?php echo $name ?></span-->
                <li class="name"><? echo $name ?></li>
        <? endforeach; ?>
    <? endif; ?>
</div>
```

By closely coupling the business logic with the presentation layer, your manager will have to assign a qualified programmer to make even innocuous changes to the front-end code rather than assign them to a design trainee. Male lions in the wild mark their territory by pissing on trees. Programmers should mark theirs by pissing all over the front-end markup.

### Reinvent the wheel. Poorly

If there’s one thing that PHP is famous for, it’s the rich selection of extremely useful, but badly named functions in its libraries and standard extensions. Make things worse by ignoring the in-built functions entirely and rolling your own instead. Feign ignorance, which is a pretty valid excuse when talking about PHP because it has extensions for everything you might dream about, and some things you might have never even heard of (can you spell Swish-e?).

Write your functions in the most naïve and inefficient manner possible. You get bonus points if it is error prone due to subtle variations in the input.

For example, PHP has an in-built strtotime() function that can convert “about any English textual datetime description into a Unix timestamp”. Ignore it. Take a date/time string and split it into its component values by using hard-coded delimiters. Don’t take regional differences into account. Everybody should just be using ISO 8601 anyways.

### Naming is the key

Begin programming before the domain model is fully ready or understood. That way you get to create your own colourful terminology for objects which haven’t been named yet. For example, suppose you’re building a web-based interface for a controller in a bakery. Look up the thesaurus for the term “mould” and use an unrelated synonym such as “die” in place of the mould. Repeated instances of the word “die” will make the maintenance programmer wonder if you wanted to pass a subliminal message on to him through your code.

Take this to the next level by using different naming conventions for the same element. If your database field stores the machine operator’s name in a “username” field, store the value in a variable called “user\_machine\_op\_curr”, “machine\_operator\_curuser”, “machop\_current” and “machine\_operator\_current\_user” in different places where you need the value.

For maximum effect, instead of encapsulating usage of this variable in a single module, apply the RYE principle to litter the code across all the files. Suppose you need a widget to display the active username at the top of the page. Write the query and code to do this on every page with subtle variations in field names and joining random bits of data from unrelated or unneeded tables. The maintenance programmer will have no idea whether the query really needs the other fields, which will dissuade him from refactoring the code into a single module.

Tell everyone who asks that you are caching data for later use in order to improve performance.

### Code formatting is hard. Let’s go shopping.

Randomize whitespace rules in your project. Use tabs in some places, spaces in another, or you can even mix and match both on the same line. Aggressively prevent any kind of structure from appearing in the code through indentation. Nested if...else blocks or loops are best for this kind of camouflage. The unwary programmer will not notice a nested loop if it is not indented further than its parent. Compound this complexity with the ability to enter and exit PHP code blocks at random locations within the file.

### Repeat Yourself Everywhere (RYE)

Functions and objects are for weenies. Duplicating large chunks of badly formatted code all over the project puts hair on your chest. Even if you are a girl.

This is the ultimate abuse of a maintenance programmer in any language. But by effectively combining the previous principles, PHP gives this technique the potential to become a lifelong nightmare of bugs which are hard to find, fix or test.

### Putting on the Pounds

By diligently following all the pointers given above you can be sure to reach file sizes of magnanimous proportions. The ultimate goal is to make a file large enough to timeout the version control server while a network operation is in progress. Make enough of those and the maintenance programmer will be caught between a hard place and a rock. On one hand, there is the fragility of the code which requires frequent commits to keep from making too big a change. On the other there are constant network errors while trying to commit changes to version control.

### Database Abuse

PHP’s ultimate utility is its excellent data-processing pedigree. Numerous extensions have been written to enable connecting PHP with different database servers. And the one common thing that all database publishers extol is to consolidate query operations through the use of joins for fewer disk operations and lower network transfer.

Ignore them.

Instead, run multiple queries – one to retrieve the master table and the rest while cycling through the result set to retrieve values from the secondary table, using the current record as the selection criteria. If you have enough values in the master table, this will slow your server down enough to frustrate the end user. Use the techniques mentioned above to obfuscate and entangle your queries with unrelated code to make it resistant to change.

For added joy, create a new connection at random places in your code to the same database server. This will confuse the casual maintenance programmer into thinking that you’re actually retrieving information from two databases, and hence justify you not using a join.

### Summary

Perform enough of these activities in a single project and you can be assured of months of job security. The best part about working in a language like PHP is that you don’t have to worry about making it not look maintainable. Legions of bad programmers have given the language such a bad rep that people almost expect PHP code to look bad.

Which kind of makes this whole essay moot, though.
