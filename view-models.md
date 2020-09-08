# Valknut &ndash; View Models

The application utilises view models heavily for all kinds of client-side communication. A page view model is an obvious candidate. But even JSON requests and responses are deserialised into a view model for better server-side manipulation.

## Core Hierarchy

The application view model architecture is highly formalised. It follows a strong hierarchy for transmitting the various kinds of data models to the client and back. The base of this entire hierachy is the class `FitNet.Web.ViewModels.EntityViewModel`. This class exposes the properties `Id` and `Name`.

All entity view models which are used to enumerate records, show editable forms or delete rows inherit from `EntityViewModel`. This offers the structure required to perform common sets of actions on these entities irrespective of their individual types and propertiies.

## Page View Model

This is a generic class that is designed for transmitting data for a standard HTML output. It has a single generic type parameter that has to inherit from the `EntityViewModel`. This class exposes the properties `Title`, `Permalink` and `Entity`. The type of `Entity` matches the type of the generic parameter.

All entity controllers can use this class to generate an editor page with a shared set of page properties and layout hosting a form with entity type-specific fields.

## DataTables View Models

DataTables is a popular jQuery plugin for rendering grids on the client-side. It supports dynamic retrieval of data, page-wise splitting, record offsets and many other features required for viewing collections.

This library is utilised heavily in Valknut, and as a result, the back-end provides built-in view models for data retrieval and submission directly through DataTables.

The classes required for this feature are listed below.

1. `FitNet.Web.ViewModels.DataTables.RequestViewModel`

1. `FitNet.Web.ViewModels.DataTables.ColumnViewModel`

1. `FitNet.Web.ViewModels.DataTables.OrderViewModel`

1. `FitNet.Web.ViewModels.DataTables.SearchViewModel`

1. `FitNet.Web.ViewModels.DataTables.ResponseViewModel`

### Request View Model

This is a composite class that organises several other view models into a single instance. When the plugin makes an AJAX request, the request is serialised into this type. It's native properties are `Draw`, `Start`, `Length` and `Error`. These properties are required for cache-busting, record navigation and error handling.

The `Search` property contains an instance of the `SearchViewModel`, while `Columns` and `Order` are collections of type `ColumnViewModel` and `OrderViewModel` respectively.

#### Server-side Processing

An AJAX request from a DataTables instance can be processed on the client-side or on the server. In the former scenario, all data is retrieved in a single network operation and stored in memory on the client. This is convenient and quick for small data sets.

But if your data sets are large or contain an indefinite number of rows, server-side processing is more efficient. For this, the plugin sends some additional information with each request.

1. Column Data

1. Column Name

1. Column Searchable

1. Column Orderable

1. Column Search Value

1. Column Search RegEx

1. Column to Order By

1. Direction of Ordering

1. Record Number to Start At

1. Length of Data Set

1. Global Search Value

1. Global Search RegEx

Column and ordering parameters are an indexed collection. The entire request is URL encoded. An example of the body sent is shown below (with the request split into multiple lines for legibility).

    draw=1&
    columns[0][data]=Name&
    columns[0][name]=&
    columns[0][searchable]=true&
    columns[0][orderable]=true&
    columns[0][search][value]=&
    columns[0][search][regex]=false&
    columns[1][data]=Id&
    columns[1][name]=&
    columns[1][searchable]=true&
    columns[1][orderable]=false&
    columns[1][search][value]=&
    columns[1][search][regex]=false&
    order[0][column]=0&
    order[0][dir]=asc&
    start=0&
    length=10&
    search[value]=&
    search[regex]=false

### Column View Model

The column fields are deserialised into an instance of this class. Each column has its own instance, and each instance has the properties `Data`, `Name`, `Searchable`, `Orderable`, `Start`, `Length`, as well as a nested instance of the search view model.

The server can utilise these values to perform any additional ordering or filtering on the result data set before dispatching it back to the client.

### Order View Model

This is a very simple class consisting of only two properties &ndash; `Column` and `Direction`. The first is the index of the primary sorting column, and the second is a string containing the value `asc` or `desc`.

The server can utilise these properties to sort the data on the server.

### Search View Model

This class contains the properties `Value` and `RegEx`, which is used to transmit keywords or regular expressions to the server. The data set can be filtered against these parameters before returning it to the client.

### Response View Model

In addition to the collection of records, the DataTables instance also requires some additional information such as the current page number. This view model implements properties for these values.

The complete list of properties are `Draw`, `Start`, `RecordsFiltered`, `RecordsTotal`, `Search`, `Error` and `Data`. The last one is an `ICollection` instance and contains the rows to be rendered.
