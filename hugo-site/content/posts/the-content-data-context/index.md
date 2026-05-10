---
title: "The Content Data Context"
date: 2018-09-13T12:39:35Z
slug: the-content-data-context
aliases: ["/the-content-data-context/"]
images: ["featured.jpg"]
categories:
  - "Construction"
tags:
  - "asp.net"
  - "c#"
  - "entity-framework"
featured_image: featured.jpg
wp_post_id: 1108
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

Recall [from an earlier post](/the-database-context/) how the DbContext class provides the framework to establish and maintain an open connection to the database. This class is typically extended with a custom context for the application, which allows the developer to add DbSet instances for all the types which are part of the application’s Entity Framework model. Each DbSet property is a collection of entity instances, one for each record in the database. The DbSet is an implementation of IDbSet, which in turn extends IQueryable and IEnumerable.

The DbContext primarily fulfils the following roles.

#### Querying

Provides an interface to convert statements from LINQ-to-Entities (using queries as well as method syntax) to the SQL syntax specific to the underlying database provider.

#### Object Materialisation

Converts database records from ADO.NET types such as DataSet to application-specific types through mapping rules.

#### Change Tracking

Tracks the state of newly created, modified or deleted entities for later committing to the database.

#### Persistence

Triggering INSERT, UPDATE or DELETE queries on changed entities when the SaveChanges method is called.

#### Caching

Provides a local, lightweight embedded store for recently retrieved entities to avoid repeated querying against the primary database.

### Connecting to the Database

The DbContext class automatically creates a database whose name is derived from the fully-qualified name of the context class. However, this behaviour can be overridden by the programmer by passing a custom connection string, database name, or connection string name from the .config file associated with the running application.

The DbContext class has 7 variants of the constructor to provide different ways of establishing a connection to the database server. The default constructor triggers the behaviour described above.

The constructor variant that takes a string parameter can be used to specify any one of the following.

1. A standard connection string.
2. A name of a connection string declared in the configuration file (using the syntax "name=connectionString").
3. An Entity Framework connection string declared in the configuration file.
4. A database name (which is sought on the server .\SQLEXPRESS by default).

Alternative constructors can accept DbConnection or ObjectContext instances instead of a string in order to work with already open connections.

### Entity Collections

Records in the database tables are referenced through a DbSet instance, which is an implementation of the set abstract data type specifically designed for Entity Framework. A collection can be non-generic by using the System.Data.Entity.DbSet type, or generic through the use of System.Data.Entity.DbSet<TEntity> type.

Collections of root entities are often exposed as DbSet properties of the application’s context class. But specific entity types can also be extracted from the database by using the Set method (or its generic variant) on the context.

The DbSet class exposes methods to perform your standard CRUD operations on the table. The most important methods are Create, Add, Find, Remove and Attach. You will notice that most of these methods are also available on the Set ADT, which the DbSet type is based on.

Create, Add and Attach sound similar in nature, but are quite different. The Create method instantiates an entity without adding it to the DbSet. The Add method is used to add the entity to the database and mark it for saving later. Finally, Attach adds an entity to the collection without marking it for saving. This is useful for adding temporary entities to read-only collections.

The revised ContentContext class for this application looks like this.

```csharp
namespace Notadesigner.Blog
{
    public class ContentContext : DbContext
    {
        public ContentContext("Notadesigner")
        {
        }

        public virtual DbSet Blogs
        {
            get;
            set;
        }

        public virtual DbSet Posts
        {
            get;
            set;
        }

        public virtual DbSet Categories
        {
            get;
            set;
        }
    }
}
```

Based on this structure, the code to retrieve a specific Post by its primary key would be as shown below.

```csharp
var db = new ContentContext();
var post = db.Posts.Find(10);
```

Inserting a new record is done in two parts. First, the entity instance is created, then it is added to the collection and the changes saved to the database.

```csharp
var db = new ContentContext();
var post = db.Posts.Create();
post.Title = "Ways to Try and Take Over the World";
post.Slug = GenerateSlug(post.Title);
// Set other properties on the entity
...
db.Posts.Add(post);
db.SaveChanges();
```

Attach is useful when you wish to update only a few properties in a record which contains many columns.

```csharp
var db = new ContentContext();
var post = db.Posts
                .AsNoTracking()
                .Where(p => p.PostId = 10)
                .FirstOrDefault();
// Change the title only
post.Title = "More Ways to Try and Take Over the World";
db.Posts.Attach(post);
db.SaveChanges();
```

The above code generates an update statement that only affects the Title column, as opposed to updating all the columns in the Posts table.

Deleting records is as simple as finding an entity and marking it for deletion through the Remove method.

```
var db = new ContentContext();
var post = db.Posts.Find(10);
db.Posts.Remove(post);
db.SaveChanges();
```

Irrespective of the method used, changes are committed only if the SaveChanges method is called on the context instance.

### Conventions, Overrides

When an application using the code-first approach is launched for the first time, Entity Framework creates tables for each entity in the newly created database by converting the entity name into its plural form. For example, the Post entity is associated with a table called “Posts”. The columns in the table are also created automatically based on the entity properties. The property PostId is associated with a column of non-nullable integers of the same name, and it is assigned as the primary key. Similarly, the Title property gets associated with the non-nullable varchar column called Title, whose maximum size is 256 characters. Default conventions can be overridden by using attributes as shown in the previous post, or by using the Fluent API in the initialisation strategy.

Some of the most essential default conventions are shown below.

#### Schema

The dbo schema is used for all database operations by default.

#### Tables

Table names are derived from the plural form of the entity they represent.

#### Primary and Foreign Keys

The Id property or <Entity>Id property is the default primary key. A property with the same name is required in the dependent table. The foreign key is named by combining the dependent navigation property name with the principal primary key property name, separated by an underscore (i.e. Post\_OwnerId).

#### Nullability

Columns to store reference type and nullable primitive type properties are nullable, and primitive type properties are stored in non-nullable columns.
