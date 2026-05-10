---
title: "Domain Entities"
date: 2018-07-28T22:29:46Z
slug: domain-entities
aliases: ["/domain-entities/"]
images: ["featured.jpg"]
categories:
  - "Construction"
tags:
  - "asp.net"
  - "c#"
  - "entity-framework"
featured_image: featured.jpg
wp_post_id: 1046
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

An entity class is written in your application programming language using standard language constructs such as the qualifier, class name and public properties. All properties must have an accessor and a mutator.

```csharp
namespace Notadesigner.Blog
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
```

The class is utilised into the Entity Framework by creating a DbSet instance property for it in the DbContext.

```csharp
namespace Notadesigner.Blog
{
    public class ContentContext : DbContext
    {
        public DbSet Posts { get; set; }
    }
}
```

### Scalar Properties

Entity classes have scalar properties to represent values which are stored in the database column directly. For example, the values for PostId, Title, Body and CreatedOn fields are stored in columns in the Posts table in the database.

### Navigation Properties

Navigation properties are used to represent relationships between database tables. Entity Framework supports relationship multiplicities of zero-or-one, one, zero-or-many, or one-or-many. Object references are used to represent relationships. Properties whose type is of another entity are used for zero-or-one, or one multiplicity. Properties whose type is a generic collection instance represent zero-or-many, or one-or-many relationships.

```csharp
namespace Notadesigner.Blog
{
    public class Post
    {
        public int PostId { get; set; }
        ...
        // Multiplicity of one or zero-or-one
        public Author CreatedBy { get; set; }

        // Multiplicity of zero-or-many, or many
        public ICollection { get; set; }
    }
}
```

### Entity States

Each entity class instance has an associated state which is maintained by the Entity Framework internally. The value of the state property changes automatically based on the operations that the developer performs on the entity instance or the database context instance.

This property can transition between the following enumerations.

- Added
- Modified
- Deleted
- Unchanged
- Detached

The context instance in the object service layer is responsible for maintaining the current state and changing it in response to API calls. When the SaveChanges method is called, the context instance determines what database operation has to be performed on each entity instance in the DbSet collection based on the value of its State property.

An insert operation is performed for entities whose state is set to Added, update for entities whose state property is set to Modified and delete for entities whose state property is set to Deleted.
