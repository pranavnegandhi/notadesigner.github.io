---
title: "Operating Entity Framework"
date: 2018-07-15T12:23:37Z
slug: operating-entity-framework
aliases: ["/operating-entity-framework/"]
images: ["featured.jpg"]
categories:
  - "Construction"
tags:
  - "asp.net"
  - "c#"
  - "entity-framework"
featured_image: featured.jpg
wp_post_id: 1042
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

### Define the Model

Entity Framework begins be preparing an in-memory model of the entity objects and their relationships, and the mapping of this model to the storage model. A default configuration can be derived by using conventional names for entities and their properties. Programmers can achieve finer control for exceptional cases by overriding configuration settings through various mechanisms.

If the database does not exist, or the configuration directs automatic modification or recreation of the database, then the database is modified or created to match the fields in the conceptual model.

### Data Creation, Retrieval, Modification & Storage

An entity is instantiated from its defining class and added to the database context through the object service. Querying to retrieve records is also done through the same layer. Any modifications to the data are performed on the entity instances, followed by a call to the SaveChanges API to commit them to the database.
