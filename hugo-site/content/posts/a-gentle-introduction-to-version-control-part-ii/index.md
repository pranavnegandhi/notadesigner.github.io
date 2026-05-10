---
title: "A Gentle Introduction to Version Control – Part II"
date: 2008-02-23T07:24:31Z
slug: a-gentle-introduction-to-version-control-part-ii
aliases: ["/a-gentle-introduction-to-version-control-part-ii/"]
categories:
  - "Construction"
tags:
  - "version-control"
wp_post_id: 61
---

### Using SVN

Subversion, or svn, is a very popular version control tool, used by some of the best software teams in the world. The fact that it is free and open source, in addition to being really, really good may have something to do with that. Like most good version control tools, Subversion too is split into two components – a server and a client.

You don’t need to worry about the server usually because it is set up on a system far out of your reach. If you are located in North America or Europe, it means that your company has outsourced the Subversion hosting somewhere in South America or India. Funnily enough, good companies in Bangalore (or Pune) usually outsource their Subversion hosting to a really, really good outfit in LA (read, Dreamhost). Silly. But that’s how the global economy works, and which is why it is more fun to be a software developer than to be an economist.

Coming back to our topic...

What you will use on a day-to-day basis is the Subversion command-line client (or TortoiseSVN, a GUI replacement).

**Disclaimer:** This article touches briefly upon how to get started with the client, without worrying about details. If you are a svn alpha-geek who dreams about hosting your own repository someday or whatever else you svn alpha-geeks dream about, and find my guide to be incomplete, don’t email me to write about the 10,542 variations in syntax that the svn client allows. I know you adore your svn client. But this article is targeted at people who don’t as much as *know* about svn, much less adore it.

Once the client is installed, you can launch a command prompt session by typing cmd into the Run dialog box in Windows and hitting enter. Once on the command prompt, type the following and hit enter.

```powershell
C:\svn help
```

Detailed help is also available for some of the more oft-used commands by modifying the help command like so –

```powershell
C:\svn help import
```

### Import

You can use the import command to **add files from your computer into the repository for the first time**, and hence, begin versioning them.

```powershell
C:\svn import trunk/ file:///F:/projects/notadesigner/trunk/web/index.htm -m "Initial import"
```

### Checkout

If you are a new developer on a team that has been using Subversion for a while, you can use the checkout command **to retrieve the source code from the repository onto your hard drive**.

```powershell
C:\cd projects\notadesigner\trunk
C:\projects\notadesigner\trunk\web>svn checkout https://notadesigner.com/svn/trunk
```

Note how the active directory is first changed to the path where the files have to be stored, and then the checkout command is called.

Alternatively, you can also specify the path to the working folder as a parameter to the checkout command.

```powershell
C:\svn checkout https://notadesigner.com/svn/trunk C:\projects\notadesigner\trunk
```

### Update

This is probably the next most used command in the Subversion client. Millions of developers across the globe run this command every morning to retrieve the latest version of files from the repository into their working folder. Running this command makes sure that you are running abreast of everyone else on your team, by bringing the latest changes they have committed to the repository onto your machine (and likewise, your latest changes get updated on their machines).

```powershell
C:\projects\notadesigner\trunk\web>svn update
```

Like true Zen, it is a deceptively simple command. Just two words that do you a whole lot of good.

### Add / Delete

These commands do just what they say. They make changes to your working copy, and schedule the same change to the repository when you commit your changes later.

```powershell
C:\projects\notadesigner\trunk\web>svn add locations.htm
C:\projects\notadesigner\trunk\web>svn delete directions.htm
C:\projects\notadesigner\trunk\web>svn commit -m "Added new locations. Deleted directions because we're spread all over the city and I didn't know where to send them."
```

### Copy / Move

Copy and move work exactly as they do on your operating system, except that they are targeted at the repository.

```powershell
C:\projects\notadesigner\trunk\web>svn copy jobs.htm https://notadesigner.com/svn/trunk -m "Adding new jobs page"
```

Move requires a separate commit to implement your changes into the repository.

```powershell
C:\projects\notadesigner\trunk\web>svn move jobs.htm jobs/index.htm
C:\projects\notadesigner\trunk\web>svn commit -m "Moved jobs page into separate folder"
```

### Revert

This command rolls back any changes made to a file in your working folder, and restores it to the version on the repository.

```powershell
C:\projects\notadesigner\trunk\web>svn revert contact.htm
```

### Using TortoiseSVN

TortoiseSVN lets you perform all actions that the svn command-line client lets you, without messing with the command-line. If you are new to Subversion, it’s a compelling replacement to the regular svn client.

### Finally, Don't Do This

I have found a shockingly large number of people doing silly things while using a VCS. I am tired of telling them not to do it and so if you abuse a VCS in any of the following ways and I catch you at it, I’ll make you write this list on a blackboard that screeches, for at least a week...without ear muffs.

It’s not that difficult to avoid them. So listen up.

#### Don't make a copy of your working folder to make edits

If you are checking out files from the repository in a working folder and then making a copy of that folder to make edits because you don’t want to overwrite your old files, you haven’t understood the most basic premise of version control.

The repository already has a copy of your files which compile correctly. If you make a mistake, revert your files and you’re back where you began from.

#### Don't make a copy of your folder in the repository before editing your files

A more severe case of the previous symptom is when a developer copies the entire trunk in the repository into trunk-2 (effectively creating a branch) and making edits to that.

#### Don't delete the current version of the file from the repository to add the edited working copy in its place

The whole point of version control is to have a history of your file over time. If you delete the file from your repository, you destroy that history. If you do it every time, somebody who looks at your file six months down the line will have no idea about the pedigree of your file, the bugs that have been fixed or the new code that has been added to it.

#### Don't forget to add comments when committing changes

You’ve got to be really lazy to not add comments at the time of committing your changes. Vague comments such as "updated file" are just as unhelpful.

It helps to be descriptive in the changelog because that’s the first place people go to find out what’s been cooking in your files. If you don’t tell them up front what you’ve been up to, they’ll just think that you’ve been messing up and saddle you with all the blame.

#### Don't forget to add new files to the repository

This is a common mistake and can happen by even the most seasoned developers. But I’m writing it here to simply reinforce it that this should be avoided.

### References

If you find this article doesn’t quite quench your thirst about version control systems or Subversion, you can learn more stuff by visiting the following websites.

http://svnbook.red-bean.com/
Colloquially referred to as the svn-book, this is the granddaddy of all Subversion books. Contains detailed explanations about the history of svn, repository and server setup and administration, detailed explanations about the acrobatic feats that the client lets you do, and if you’re really interested, a description of the C APIs that let you hook up svn with your own applications.

[http://www.joelonsoftware.com](http://www.joelonsoftware.com/)
A delightful collection of extremely well-written and funny articles about computers, software and management by Joel Spolsky. This should be a must-read for every developer.

http://www.ericsink.com/scm/source_control.html
Eric Sink is the founder of SourceGear, a company that specializes in selling version control software. You can’t get anybody better to write about version control.

http://betterexplained.com/articles/a-visual-guide-to-version-control/
Kalid Azad maintains this brilliant blog about all things mathematical and computer-sciencey. It’s a refreshing view to everyday complexities that you take for granted.
