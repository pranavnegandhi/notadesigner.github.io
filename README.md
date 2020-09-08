# Valknut &ndash; Workout Tracking

The primary motivation for this project is to design and implement a model project template for the ASP.NET MVC framework. Between a choice of correct and expeditious, I've usually chosen the former, even if it means learning an obscure aspect of the ASP.NET MVC framework.

## Project Status

The product is in what I call functionally usable state. It provides all necessary features to capture, store, retrieve, edit and delete the most essential facets of the product information model. A basic summary report has been implemented, and more reports are on their way.

## History

The initial product idea germinated in the year 2016 as a means to store personal health information safely on an individual desktop computer, away from prying eyes. To that end, the application never considered the possibility of multiple simultaneous users or authentication, or non-local persistence. Each individual user would store their own records in their personal file, which would be protected by the user's own encryption key.

The project was rather unimaginitively titled Fit Net.

The product and the exercise were both collosal failures due to the steep learning curve of the WPF framework on top of my day job (where I was learning ASP.NET MVC + EF6 already). After remaining shelved for a very long time, I picked it up again early in 2020 to breathe some life back into it.

Among other things, I changed it into a web application since that was a platform I already knew well by then, and renamed it to a much more distinguished Valknut, invoking imagery of Norse mythology, Viking warriors and Valhalla.

The architecture of the application still aspires to be amended into a desktop-based product someday.

## Architecture

Valknut follows classical MVC architecture. The application is separated into two major projects for the website and the entity model classes. The website project contains classes that implement the web controller interfaces and the views. The models project is a class library to implement the entities that make up the domain model.

[Domain Model](domain-model.md)

[Localisation](localisation.md)

[Validation](validation.md)

[View Models](view-models.md)

[JSON Handler](json-handler.md)
