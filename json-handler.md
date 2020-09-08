# Valknut &ndash; JSON Serialization

Many service responses are returned as JSON objects. For this, the application must implement some basic configuration and error handling in each method. By utilising the cross-cutting facet of attributes, this code is unified into a common location and applied wherever required.

## Signalling

Firstly, the action method that returns JSON has to be marked. The JsonHandlerAttribute class inherits from ActionFilterAttribute. It is applied to the method in question. When the method is invoked, the ASP.NET pipeline executes the `OnActionExecuted` method on its attribute. The method converts the `System.Web.Mvc.JsonResult` result into a custom-written class called `FitNet.Web.Infrastructure.JsonNetResult`. This class inherits from `JsonResult` and adds some shared functionality.

## Error Handling and Serialization

The conversion from `JsonResult` to `JsonNetResult` is performed inside the `JsonHandlerAttribute` class. The `Result` property of the current filter context is typed into `JsonNetResult`. If the conversion is successful, then a new instance of `JsonNetResult` is created and the copies of the current result copied over.

Finally, the `ExecuteResult` method is invoked on the newly created result instance. This step performs the following error checks.

1. Deny GET requests by default, unless it is explicitly allowed in the method.

2. Set the content type of the response to `application/json`.

3. Configure the underlying serializer to throw an error in case of looping references.

If all the checks pass, then the response data is serialized and the output is assigned to the `Output` property of the current response.

This structure of the code removes the need to have all these checks in each method that has to return a JSON response.
