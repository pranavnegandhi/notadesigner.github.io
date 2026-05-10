---
title: "Laying the Groundwork"
date: 2018-08-04T08:58:17Z
slug: laying-the-groundwork
aliases: ["/laying-the-groundwork/"]
images: ["featured.jpg"]
categories:
  - "Construction"
tags:
  - "asp.net"
  - "c#"
  - "entity-framework"
featured_image: featured.jpg
wp_post_id: 1070
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

Many application developers prefer a code-first approach to implementing a business application with Entity Framework. In this workflow, the programmer begins by writing classes that represent entities in the domain model. As we are aware, each instance of these classes represents a single row from the database. The entity classes correspond to a single table. Public properties on the class are mapped to columns in this table.

For example, a Person class maps to the Persons table in the database in the conventional Entity Framework implementation. The properties Name (string), Age (int) and Gender (enum), correspond to columns Name (nvarchar), Age (int) and Gender (int). When a row is retrieved from the database, the values from its columns are loaded into the properties of this instance. Conversely, modified values in the Person instance can be written back to the table by passing its reference back into the appropriate Entity Framework APIs.

Traditionalists would cringe at the thought of writing application code without a rock-solid database model already in place to back it up. And their outlook is borne out of poor outcomes from automatic table-generation tools. Fortunately, Entity Framework (and several other modern ORM tools) address the table performance characteristics and data integrity problems very well.

If performance or correctness are a problem, they are probably a result of poor description of the application domain by the programmer, rather than a shortcoming of the ORM. Being able to generate entity structures from the application layer does not take away the need to understand the performance characteristics of a database engine. And errors in designing or describing the domain model by inexperienced application developer can still affect the product adversely.

### The Business Model

The subject of this exercise is a multi-user blog that can be used to host and serve posts authored by one or more registered users. Posts are created and owned by any user with the appropriate level of access to the editor interface. The owner can then invite more authors to collaborate with them. Collaborators can edit the contents of a post, but cannot delete it.

Finally, anonymous users can view the post by visiting a specific URL for each post. The blog home page is a read-only index of all posts, sorted in reverse chronological order.

#### The Blog

The blog is the primary entity in our application. It has several descriptive properties of the site itself, such as the name, description and URL. A blog is owned by a user who has super-admin rights to the application. This user has to be created and linked to the blog at the time of setup.

```csharp
using System.ComponentModel.DataAnnotations;

namespace Notadesigner.Blog.Models
{
    public class Blog
    {
        [Key]
        public int BlogId { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        public string BaseUrl { get; set; }

        [Required]
        [ForeignKey("Owner")]
        public int OwnerId { get; set; }

        public User Owner { get; set; }

        public ICollection<Category> Categories { get; set; }

        public ICollection<Post> Posts { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
```

This example introduces several classes from the DataAnnotations namespace which Entity Framework uses when creating the database tables.

##### Key

The default convention of the framework is to make any property suffixed with the word Id into the primary key for the corresponding table. The Key attribute is not required on the BlogId property. But making it explicit helps maintenance later down the line. The index in the database engine is prefixed with IX\_, e.g. IX\_PostId.

##### Required

Nullable columns are the default convention in Entity Framework. The Required attribute changes this and marks them as non-nullable. In addition to enforcing data integrity, this attribute is also used to provide client-side validation in the MVC framework.

##### StringLength

This attribute defines an explicit maximum on the size of string columns, such as nvarchar in MS SQL. Like Required, this attribute is also usable for client-side validation.

##### ForeignKey

This attribute defines a foreign key between two entities which enforces relational integrity between tables in the database. The UserId column can only contain a value which already exists in the corresponding column in the User table.

This attribute requires the name of the property which contains the referenced entity record. In this case, the parameter is “Owner”, which is of type User. The User class is described further below.

#### Users & Roles

A User is the unique identification of a person who operates or accesses content from the blog. Is is composed of a minimum of a unique identification number and a unique user name. A person who wishes to identify as a particular User in the application also requires the password for that account. Passwords are stored as salted and hashed strings.

Roles are coarse-level controls over the actions that a single User can perform in the application. A role is composed of several granular permissions, which can either be explicitly granted or denied. Permissions are not provided by individual modules in the application. For example, the Posts module might have the permissions view, create, edit-title, edit-body, edit-attributes, publish, unpublish and delete. The Users module might offer the permissions create, reset-password, recover, edit-attributes and delete. A Role organises these permissions into a cohesive group of related permissions (e.g. Subscriber, Publisher, Content Creator, Intern, Moderator, Administrator, etc.).

In a typical usage scenario, Users must belong to at least one role in order to be able to perform any useful identifying activity in the application. But the application can also be configured to allow anonymous access to the entire application.

#### Posts

A Post is a single unit of content in the application. It is created and owned by a single user, and can have one or more collaborators who contribute to the content.

```csharp
using System.ComponentModel.DataAnnotations;

namespace Notadesigner.Blog.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(256)]
        public string Title { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [StringLength(128)]
        public string Slug { get; set; }

        [StringLength(102400)]
        public string Body { get; set; }

        [Required]
        public PostStatus Status { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        [Required]
        [ForeignKey("Owner")]
        public OwnerId { get; set; }

        public User Owner { get; set; }

        public ICollection<User> Collaborators { get; set; }

        public ICollection<Category> Categories { get; set; }
    }
}
```

The class is fairly vanilla, and has all the data annotations which were described previously. It also introduces the Index attribute on the Slug property. A slug is a human-friendly identifier for a post. Words and phrases add more context to the human mind than plain integers. So it is easier to remember and parse what the phrase “a-field-guide-to-entity-framework” points to, than it is to remember the post ID.

In order for slugs to work correctly, they have to be unique. Therefore, the model requires a unique constraint on the value contained in the slug. This instruction is converted into a unique, clustered index in the database table.

A single entity can have multiple indexes, only one of which can be clustered. An index can be composed of multiple properties.

#### Categories

Categories provide the taxonomy framework to organise posts into groups of common topics. It is a fairly common feature in all content management systems.

```csharp
using System.ComponentModel.DataAnnotations;

namespace Notadesigner.Blog.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        [Required]
        [Index(IsUnique = true)]
        public string Slug { get; set; }

        [Required]
        [ForeignKey("Owner")]
        public OwnerId { get; set; }

        public User Owner { get; set; }

        public ICollection<Post> Posts { get; set; }
    }
}
```

These classes collectively provide all the data objects needed to store and manipulate records from the database in the application. The next post in the series will cover the process of connecting to a data source and loading records into memory by using the data context.
