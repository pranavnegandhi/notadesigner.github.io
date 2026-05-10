---
title: "Nothing Is So Simple That it Cannot Be Difficult"
date: 2012-12-21T12:49:25Z
slug: nothing-is-so-simple-that-it-cannot-be-difficult
aliases: ["/nothing-is-so-simple-that-it-cannot-be-difficult/", "/531/", "/nothing-so-simple-that-it-cannot-be-difficult/"]
categories:
  - "Construction"
tags:
  - "usability"
  - "user interface"
wp_post_id: 531
---

Long years in the software industry have conditioned me to dread last-minute feature additions. Something as innocuous as a mailer subscription form can turn into a minefield of security and privacy concerns if done incorrectly. In the book Peopleware, writers Tom DeMarco and Timothy Lister have expressed how managing software projects is less of a technical challenge and more of a social interaction maze. Keeping clients happy while still being able to convince them about the intensive behind-the-scenes work behind the simplest of features takes a lot of people-skill.

Incomplete or inaccurate specifications can complicate the problem even further. This can be a result of the specifications being created by inexperienced people, or people with little or no understanding of UX or software. This becomes particularly insidious because the presence of a specification (even if it is only superficial) lulls stakeholders into thinking that the project is under control. But all sorts of bad things begin to happen once the rubber meets the road. Designers work towards an incomplete layout. Engineers make an incomplete product, which results either in customer dissatisfaction or time and cost overruns.

A seemingly simple feature such as pagination requires quite a bit of programming just to make it work at all. More code has to be written to consider common cases such as representation of and navigation through large data sets. Usability issues have to be taken into account, such as device specifics to handle different input systems such as touch screens, keyboards and mouse clicks.

Product requirements often begin with a very vague idea of what is required. "A blog application needs comments", says the product manager. So the engineering team gets to work and churns out a comment form, and a display mechanism. But nobody expected Jeff Atwood to deploy the software on his website, which regularly gets 3000 comments in the first 10 minutes of a post being put up, and nobody can now open his website any more because the database server choked on retrieving so many comments for a million simultaneous visitors (yes, even if they are flat discussions).

After Jeff posts a rant on his website, the product manager eats his hat and gets back to writing a specification for the comment pagination, as he should have to begin with. This is his design from the first draft of the specification.

```
Previous | 1 | 2 | 3 | 4 | 5 | Next
```

The engineering lead looks at the design and notices that it is still incomplete. The Previous and Next buttons will not do anything useful if the visitor is already on the first or last page. So those links will have to be disabled as necessary. The revised design looks like this.

```
Previous | 1 | 2 | 3 | 4 | 5 | Next
```

```
Previous | 1 | 2 | 3 | 4 | 5 | Next
```

But what happens when there are more than 5 pages of comments on the post? If they keep increasing the page count, eventually it will have to be wrapped to the next line or made to run off the screen, either of which looks very ugly.

```
Previous | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | Next
```

Somebody suggests letting users type in the page number in a text field that also doubles up as the current page indicator.

```
First |  | Last
```

While this is easy, it is not very user friendly. How many pages of comments do we have in all? What happens if the visitor enters too large a number or a non-numeric value? Visitors on mobile devices probably will not be very pleased to have to type very frequently.

After yet another round of discussion, the team finally decides that visitors are probably not interested in viewing every comment on the blog. Yet, showing the total number of comments would be desirable for Jeff because it is a measure of his popularity. Fitness models measure bicep circumference. Geeks measure blog comments.

Somebody suggests the following user interface, which seems to fit the bill.

```
1 | 2 | 3 | 4 | 5 ... 274
```

```
1 ... 65 | 66 | 67 | 68 ... 274
```

```
1 ... 270 | 271 | 272 | 273 | 274
```

The first and last page links are always displayed. The rest of the links are dedicated to displaying the current page number and a range of values around it. For the end user - the website visitor - the control is very simple and easy to learn at a glance.

Of course, this entire exercise is moot if it is done without proper usability testing with relevant target audiences in mind. Formal testing methods are often inaccessible to small teams, but hallway tests can still provide feedback, and results in a much better product.
