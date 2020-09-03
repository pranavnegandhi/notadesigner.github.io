# Valknut &ndash; Validation

## Introduction

ASP.NET MVC has always had a robust request validation model that is easily harnessed. The model in question has to be decorated with validation attributes from a broad collection of possibilities, and the framework takes care of ensuring compliance and distilling it into a single boolean that can be referenced from `ModelState.IsValid`.

Nothing could be easier.

However, this approach requires that the programmer must remember to write the code to perform a validation check in each action method. This is tedious and error-prone, and results in duplicated code.

It is possible to implement this functionality in a more modular fashion by implementing it as a filter attribute and plugging it into the ASP.NET request pipeline.

## Passive Attributes

Before we go into the implementation of the filter itself, there is a slight design idiosyncracy that has to be understood. Request filtering through a class that derives from `ActionFilterAttribute` is a common enough pattern. A request is filtered through this attribute if the method implementing its corresponding action is decorated with this attribute. But this approach imposes various technical restrictions and design compromises.

A more robust approach is to decorate the action method with a non-behavioural attribute that derives from `Attribute`, and adding the filter directly into the global filter collection.

Passive attributes are described by Mark Seemann at the link below.

[Passive Attributes](https://blog.ploeh.dk/2014/06/13/passive-attributes/)

## Implementation

The passive attribute approach results in an attribute class called `ValidateModelAttribute`. Any method that requires model validation can be decorated with this attribute.

    [HttpPost]
    [ValidateModel]
    public ActionResult Edit(PageViewModel<ExerciseEditViewModel> viewModel)

A `RequestValidateModelFilter` class implements the `IActionFilter` interface and is added into the global filters collection during the `Application_Start` event. When an incoming request arrives, it is passed through the `RequestValidateModelFilter` instance, which checks if the request requires validation, and if so, checks if the `IsValid` property of the model state is true.

If the model state is not valid, then the request pipeline is truncated and all validation errors are gathered into a JSON response. The application returns a HTTP status code 400 along with the list of errors in the body of the response.
