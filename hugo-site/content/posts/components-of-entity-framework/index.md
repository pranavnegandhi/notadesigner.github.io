---
title: "Components of Entity Framework"
date: 2018-07-08T12:23:00Z
slug: components-of-entity-framework
aliases: ["/components-of-entity-framework/"]
images: ["featured.jpg"]
categories:
  - "Construction"
tags:
  - "asp.net"
  - "c#"
  - "entity-framework"
featured_image: featured.jpg
wp_post_id: 1040
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

Like most well-designed APIs, Entity Framework is composed of several discrete components which complement each other. There are three high-level components – the Entity Data Model, the Querying API and the Persistence API.

Collectively, these components allow application programmers to operate upon classes which are specific to their business domain rather than indulging in the low-level details of ADO.NET and SQL.

### Entity Data Model

#### Conceptual Model

This layer of the data access layer is built by the application programmer. It contains class representations of the domain model for whatever business requirement that the application aims to achieve. The programmer identifies the objects which make up the given domain, then implements them as classes. The classes have public properties that correspond to properties of the domain object, and are also converted into database columns by the Storage Model. At runtime, these classes are instantiated as CLR objects and their properties are populated with values from corresponding database columns.

#### Storage Model

The database and its various elements – the tables, views, stored procedures, indexes and keys – make up this layer. This is a relational database such as SQL Server or PostgreSQL, although future versions of the Entity Framework are said to support NoSQL databases.

#### Mappings

Object-oriented programming can be mapped to relational schemas quite closely in most standard scenarios. A class can easily have the same properties as a table column and use equivalent data types to represent it in memory at runtime. Object references are represented as relationships between two tables with all accompanying aspects such as defining keys between tables.

Data constraints and default values can also be declared in this layer by using the appropriate configuration APIs. Any mismatch between the objects, tables and mapping results in a runtime exception which can be handled by the programmer.

### Querying API

There are two APIs that programmers can use to interact with the database from Entity Framework.

#### LINQ to Entities

This is a query language that retrieves data from the storage model, and with the aid of mappings, converts it into object instances from the conceptual model.

#### Entity SQL

This is a dialect of SQL which operates upon conceptual models instead of relational tables. It is independent of the underlying SQL engine, and as a result, can be used unchanged between various database engines.

Both querying languages are operational through the Entity Client data provider, which manages connections, translates queries into data source-specific SQL syntax, and returns a data reader with records for conversion into entity instances. By dint of being an abstraction over the connection, Entity Client can also be used to breach the Entity Framework abstraction and interact with the database directly with ADO.NET APIs.

### Persistence API

#### Object Service

The programmer interacts with the object service to access information from the database. It is the primary actor in the process of converting the records retrieved from the data provider into an entity object instance.

#### Entity Client Data Provider

The Entity Client Data Provider works the other way around, to translate queries written in LINQ-to-Entity into SQL queries that the database understands. This layer interacts with the ADO.NET data provider to fetch from or send data into the database.

#### ADO.NET Data Provider

This is the standard data access framework from Microsoft. Entity Framework is an entity-level abstraction over the APIs provided by this library.
