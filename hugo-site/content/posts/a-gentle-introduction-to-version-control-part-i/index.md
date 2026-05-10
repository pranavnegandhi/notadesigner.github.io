---
title: "A Gentle Introduction to Version Control – Part I"
date: 2008-02-22T07:18:36Z
slug: a-gentle-introduction-to-version-control-part-i
aliases: ["/a-gentle-introduction-to-version-control-part-i/"]
categories:
  - "Construction"
tags:
  - "version-control"
wp_post_id: 59
---

In our rush to Web 2.0 our lives, we seem to have forgotten to imbibe the essentials. Everyone is guilty of that – from fruit farmers using artificial fertilizer, to politicians driving nations to war, to developers not using version control.

Shocking, really.

I do not have much pull with the fruit farmers or international diplomats. But as a fellow developer, it’s my duty to bring back the lost software teams of today into the fold. Forget “enterprisey architectures” for a while and look at the basics. In this article I will introduce you to the wholesome goodness of Subversion, a fantastic version control tool.

### Essentially Missing

Version control is an essential tool in any software team’s kit. In spite of that, I regularly run into senior developers with years of experience who have never used it. Which is, to be honest, quite frustrating because then their working directory tends to look like this –

Product-latest

Prodcut-new-12-march (yes, it is misspelled)

Product-new-mar-15

...[and the disaster repeats till eternity](http://thedailywtf.com/Articles/The_Best-est_Version_Control.aspx "The Best-est Version Control").

If you notice, this directory structure is already functions as a basic version control repository, in that it separates successive updates to the product, but without all the extra goodies that a true version control tool would let you have such as comments, labels and developer history.

### Towards the Light

Whenever I have had the good fortune of introducing developers to version control – even with rogue tools like Visual SourceSafe – they knew this was the elixir they were missing for so long. It takes some work to actually drive it into their system. But once there, it stays on forever.

The single biggest advantage all of them cite is how easy it becomes to **synchronize files between team members**. Because everybody is updating their files every morning from the same location, it becomes easier to keep abreast with everyone’s changes.

The next biggest advantage is **the ability to roll back mistakes**. If a file has not been checked in, simply revert it back and the VCS replaces it with the latest one in the repository. Even after the file has been checked in, a previous version of the file can be retrieved from the repository and used to restore the changes made.

A side-effect of this feature is the ability to **sandbox major changes**. Rewriting core algorithms of your accounting product using that new design patterns book? (Hah!) Do it in a local working copy, test it and then ~~throw it away when you discover you suck at patterns~~ check it in after it works fine.

As a project’s requirements evolve, files mutate into completely different beasts from their initial incarnation. By logging a note about each change made to the file in the VCS, developers are able to **track the project history** in the long term.

Another nice feature of the tracking tools is that they help **assign ownership** by logging the person who has made a change. This proves to be quite helpful when giving credit, or more frequently, blamestorming.

And the greatest relief that a VCS provides to all stakeholders is the **daily backup that occurs** **automatically** when developers check-out the latest changes every morning. It’s rare for a team using VCS to lose a lot of data due to hard drive failures.

### The VCS Dictionary

Let’s begin with learning the terminology used when dealing with version control.

#### Parts of a VCS

- Repository: The database storing the files. The repository is usually expected to be on a central location such as a network server, although it can exist on locally stored directories.
- Server: The computer storing the repository. If the repository is stored on a directory on your own computer, then your computer is called the server although there is no network access involved.
- Client: The computer connecting to the repository. Your computer.
- Working Set/Working Copy: Your local directory of files, where you make changes.
- Trunk/Main: The primary location for code in the repository. This is the in-progress version of code, with untested or partially implemented features. Feature-complete snapshots are stored in a branch folder.

#### Common Terms

- Revision: What version a file is on (v1, v2, v3, etc.).
- Head: The latest revision in the repository.
- Commit Message: A short message entered at the time of committing a file, describing what was changed.
- Changelog/History: A list of changes made to a file since it was created.

#### Basic Actions

- Add: Put a file into the repository for the first time.
- Check-out: Download a file from the repository.
- Commit/Check-in: Upload a file to the repository. The file gets a new revision number, and people can check out the latest one.
- Update/Sync: Synchronize your files with the latest from the repository. This lets you grab the latest revisions of all files. Do this at least once a day.
- Revert: Throw away your local changes and return to the latest version from the repository.
- Diff/Change/Delta: Finding the differences between two files. Useful for seeing what changed between revisions. This only works on text files (e.g. .as, .htm, .cs). Binary files (e.g. .psd, .fla, .doc) cannot be diffed.

#### Advanced Actions

Branch: Create a separate copy of a file/folder for private use (bug fixing, testing, etc). Branch is both a verb (“branch the code”) and a noun (“Which branch is it in?”).

Merge: Apply the changes from one file to another, to bring it up-to-date. For example, you can merge features from one branch into another.

Conflict: Occurs when two people edit the same file simultaneously. The first person to edit the file does not face any error. However, the file on the server is now out of sync with the file on the second person’s working folder. When he attempts to commit the file to the repository he gets a conflict.

Conflicts can be resolved for text files by manually selecting which lines of code to keep and which ones to discard.

There is no way to resolve binary files. If there is a conflict in binary files, then the second person has to begin again by retrieving the latest file, and re-creating the changes made to it before the conflict occurred.

To avoid conflicts on binary files, a user can lock the file before editing.

- Resolve: Fixing the changes that contradict each other and checking in the correct version.
- Locking: Flagging a file for exclusive use until it is committed again.
- Breaking the lock: Forcibly unlocking a file so you can edit it. It may be needed if someone locks a file and goes on vacation (or “calls in sick” the day Halo 3 comes out).

### An Illustrated Example

Pop Candy is a Java developer who has just joined the team at Timeless Software. Her task is to [add an email client to their product](http://www.iwar.org.uk/hackers/resources/faq/jargon.htm#Zawinski's%20Law "Zawinski's Law"). She **creates a folder on her hard drive** for the project, and proceeds to **check out the files from the repository**.

Once she has the entire product code check out onto her hard drive, she compiles it and familiarizes herself with its features.

Once she’s ready, and [has read the spec](http://www.joelonsoftware.com/articles/fog0000000036.html "Painless Functional Specifications - Why Bother?"), she begins editing the first file. She adds a few more files to the project, compiles and tests. Makes a few changes, goes back and compiles and tests, and in no time at all, she has reached her first milestone. Her email client connects to the server and successfully [handles the server’s response to HELO](http://en.wikipedia.org/wiki/Simple_Mail_Transfer_Protocol#Sample_communications "Simple Mail Transfer Protocol Sample").

She feels she’s achieved quite a bit for the day and proceeds to add her changes to the repository. The first step is to **select the new files** she’s created and **adding them to the repository**. Then, she selects the new files, and the ones she’s modified and **commits them** all in a single operation. She remembers to **enter a log comment** that describes the change clearly. When the other developers come in the next day and **update their working copies**, they’ll see that Candy has begun working on the email client.

The next day Candy is working on adding a rich text editor into the email client. She uses an off-the-shelf JavaScript library to do the job to save time. Smart!

But when her team lead reviews it, he points out to her that the terms of license of this library conflict with those of their commercial product. Candy is grumpy because it took an entire day to integrate the editor. It was her fault, though. The spec clearly stated that no off-the-shelf library is to be used. So she has to go back and undo her changes. She **deletes all the unversioned files** in the project folder and proceeds to **revert the edited files** to their head version.

A few days into development, as the email client begins adding bulk, the testers complain that the product itself seems to be very sluggish. Everybody scrambles to get their hands on **the changelog** on the date since when the testers noticed the slowdown.

All fingers are pointed at Candy’s code for the moment, but the team lead defers any decision until he’s actually reviewed her commits on that particular day. He retrieves the files of the previous revision and runs them through the tests. On a whim, he decides to replace their in-house SMTP test server with the live email server.

Luckily, it turns out that it was their testing SMTP server which was misbehaving. Instead of closing the connections on receiving QUIT, it continued to hold them. And the testers were the only ones to notice this because they sent the client through a gruelling 1,000,000 rounds of sending and receiving email.

You now see how easy it is to manipulate readers into seeing from your viewpoint with a contrived example veiling your actual agenda.

Oops!

No, that’s not what I meant. The moral of this story is that it is always a good idea to use version control. Version control is mother’s love and apple pie. It is the cat’s whiskers and the bee’s knees. And you just gotta have it!

[Move on to part two here](/a-gentle-introduction-to-version-control-part-ii/ "A Gentle Introduction to Version Control – Part II").
