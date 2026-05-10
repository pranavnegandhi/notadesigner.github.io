---
title: "Introduction to ORM & Entity Framework"
date: 2018-07-01T12:15:29Z
slug: introduction-to-orm-entity-framework
aliases: ["/introduction-to-orm-entity-framework/"]
images: ["featured.jpg"]
categories:
  - "Construction"
tags:
  - "asp.net"
  - "c#"
  - "entity-framework"
featured_image: featured.jpg
wp_post_id: 1038
---

*This post is part of a series on learning how to use Entity Framework. The rest of the posts in the series are linked below.*

### Basics of Entity Framework

- [Introduction to ORM & Entity Framework](/introduction-to-orm-entity-framework/)
- [Components of Entity Framework](/components-of-entity-framework/)
- [Operating Entity Framework](/operating-entity-framework/)
- [The Database Context](/the-database-context/)
- [Domain Entities](/domain-entities/)

### Code First

- [Laying the Groundwork](/laying-the-groundwork/)
- [The Content Data Context](/the-content-data-context/)

### Database First

*Coming Soon!*

* * *

Having application programmers design, implement and maintain databases has often been a challenge. Maintainability and performance have taken a hit, resulting in less than stellar application performance and poor extensibility.

Object Relational Mapping libraries have been written since a long time to gloss over lack of proficiency in SQL and database design. However, they too have been cited as a major bottleneck to performance due to abstracting over the underlying relational model, and generating bloated and inefficient queries.

Newer generation ORM tools have solved performance and design problems to a large extent, resulting in frameworks which developers can use with confidence even for large-scale and high-availability applications.

### What is Entity Framework?

Entity Framework is Microsoft’s offering in this space for use with the .NET framework. It is built upon the traditional ADO.NET framework and therefore, can be made to work with almost any relational database with bindings for the .NET framework.

Entity Framework is an open source ORM library which is published by Microsoft. It complements the .NET framework and is built upon the ADO.NET framework.

Entity Framework frees up the programmer from having to translate information between .NET instances and DataSet objects which are used by ADO.NET to retrieve, insert and update records in the data store.
