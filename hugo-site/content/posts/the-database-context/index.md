---
title: "The Database Context"
date: 2018-07-22T08:38:22Z
slug: the-database-context
aliases: ["/the-database-context/"]
images: ["featured.jpg"]
categories:
  - "Construction"
tags:
  - "asp.net"
  - "c#"
  - "entity-framework"
featured_image: featured.jpg
wp_post_id: 1044
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

DbContext is probably the single most important class in the EF API, as it directly represents a connection with the database from the application. A minimal context class inherits from DbContext and exposes DbSet instances for each entity type which has to be stored or retrieved from the database.

Querying for information retrieval and storage is done through the DbContext instance. If the developer has not overridden any configuration settings, Entity Framework attempts to connect to a database with the fully qualified name of the DbContext class itself.

```csharp
namespace Notadesigner.Blog
{
    public class ContentContext : DbContext
    {
        public ContentContext()
        {
        }
    }
}
```

In the above example, Entity Framework automatically tries to connect to a database called Notadesigner.Blog.ContentContext. This can be overridden by setting the configuration in various ways.

```csharp
namespace Notadesigner.Blog
{
    public class ContentContext : DbContext
    {
        public ContentContext("Notadesigner")
        {
        }
    }
}
```

Other ways to override the default connection name is to add a connection string in the application configuration file and supplying it as a parameter to the DbContext constructor.
