---
title: "Runtime Resource Authorisation in ASP.NET MVC"
date: 2019-12-05T16:21:00Z
slug: runtime-resource-authorisation-in-asp-net-mvc
aliases: ["/runtime-resource-authorisation-in-asp-net-mvc/"]
images: ["featured.jpg"]
categories:
  - "Technique"
tags:
  - "asp.net"
  - "c#"
  - "programming"
featured_image: featured.jpg
wp_post_id: 1447
---

The Authorize attribute is a feature of the ASP.NET MVC framework that programmers learn early on. While it is a good out of the box solution for general cases, it doesn't work well for dynamic authorisation. Take the HTTP request shown below.

```
GET /posts/edit/12 HTTP/1.1
Host: www.example.com
```

In colloquial MVC, this requests the `PostsController` to retrieve the contents of the post with ID 12 and display them in a form. The `Authorize` attribute does not determine if the currently logged in user has been granted editing rights for that specific post. At best, operations are allowed based on roles or claims, which still becomes an all or nothing situation. Either an individual user can edit all posts, or none at all.

Finer-grained control over individual resources for each user in the system requires a custom solution.

The system described below eschews the `Authorize` attribute entirely, and chooses to instead use filters in the ASP.NET request pipeline. It imposes the restriction that the name of the resource identifier parameter should always be well-known, such as `id`. Since the default route already follows this convention, this usually isn't a problem.

### Identifying the What

The first piece of the puzzle is a custom action filter called `SecuredAttribute`. This class inherits from `System.Attribute` and is applied to methods. Any controller action method that is marked with this attribute identifies as a sensitive access point that requires some kind of screening procedure before being invoked.

But this attribute only identifies the method. It does not perform any kind of screening on incoming requests. This is also why it doesn't inherit from any of the more higher-level attributes from the MVC framework, such as `ActionFilterAttribute`.

```csharp
public class SecuredAttribute : Attribute
{
}
```

The `SecuredAttribute` is used by applying it to the top of the controller method that needs runtime screening.

```csharp
public class AdminController : Controller
{
    [Secured]
    public IActionResult Edit(int id)
    {
        ...
    }
}
```

### Implementing the How

The screening is performed by a class that implements `IActionFilter`. There can be multiple screening filters, and they are queued up in the `GlobalFilterCollection` during `Application_Start()`. The screening process is performed before the action method is executed, by implementing it in the `OnActionExecuting` method of the filter class.

```csharp
public class AuthorizationFilter : IActionFilter
{
    ... 
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var secured = context.ActionDescriptor.GetCustomAttribute(typeof(SecuredAttribute), false).FirstOrDefault();
        if (null == secured)
        {
            return;
        }

        var user = context.HttpContext.User;
        var param = context.ActionParameters.Where(p => p.Key == "id").FirstOrDefault();
        var id = Convert.ToInt32(param.Value);

        // Invoke a service to check if the request should be allowed
        var isAllowed = securityService.IsAllowed(user, id);
        if (!isAllowed)
        {
             context.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
        }
    }
}
```

The filter looks for the `[Secured]` attribute. If the method being invoked doesn’t have the attribute, the filter immediately returns and lets the method execution proceed. If the attribute is found, the filter performs a screening procedure to determine if the request should be allowed or not. It may use a injected service class or even a third-party API to perform this action.

Since the attribute is only identifying the method, it remains simple. Discrete behaviours can be attached to the same action method, that can also be dependent on the request context (e.g. invocation through web vs. API) while maintaining a clean separation of concerns.

Some of these techniques are shown below.

### Extending Beyond Simple Authorisation

The method attribute can be leveraged for performing other cross-functional requirements, which are tangent to authorisation. The secured method may require an audit trail.

```csharp
public class AuditTrailFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var secured = filterContext.ActionDescriptor.GetCustomAttribute(typeof(SecuredAttribute), false).FirstOrDefault();
        if (null == secured)
        {
            return;
        }

        // Invoke a service to log the method access
        Logger.Info(...);
    }
}
```

The authorisation and audit trail filters can coexist and are fired independently. They use the same marker to identify the methods, but perform widely different tasks with different resources at their disposal. `AuditTrailFilter` can be programmed to log requests to secured location in one store and all other requests into another store, while `AuthorizationFilter` always allows requests to unsecured locations.

Another example is to return different responses to the client based on its type. When a request comes from a browser, its Accepts header is set to `text/html`, while an API client such as a SPA or a mobile app sets it to `application/xml` or `application/json`. The `WebAuthorizationFilter` class returns the access-denied error as a HTML view, which the browser displays as a user-friendly error page.

```csharp
public class WebAuthorizationFilter : IActionFilter
{
    ... 
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Return if a non-API request is received
        var acceptTypes = HttpContext.Current.Request.AcceptTypes;
        if (!acceptTypes.Contains("text/html"))
        {
            return;
        }

        var secured = context.ActionDescriptor.GetCustomAttribute(typeof(SecuredAttribute), false).FirstOrDefault();
        if (null == secured)
        {
            return;
        }

        var user = context.HttpContext.User;
        var param = context.ActionParameters.Where(p => p.Key == "id").FirstOrDefault();
        var id = Convert.ToInt32(param.Value);

        // Invoke a service to check if the request should be allowed
        var isAllowed = securityService.IsAllowed(user, id);
        if (!isAllowed)
        {
            context.Result = new ViewResult()
            {
                ViewName = "AccessDenied",
            }
        }
    }
}
```

The `ApiAuthorizationFilter` class, on the other hand, returns a HTTP status code 403 in the response. The API client generates an appropriate error view on the client-side.

```csharp
public class ApiAuthorizationFilter : IActionFilter
{
    ... 
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Return if a non-API request is received
        var acceptTypes = HttpContext.Current.Request.AcceptTypes;
        if (!acceptTypes.Contains("application/xml"))
        {
            return;
        }

        var secured = context.ActionDescriptor.GetCustomAttribute(typeof(SecuredAttribute), false).FirstOrDefault();
        if (null == secured)
        {
            return;
        }

        var user = context.HttpContext.User;
        var param = context.ActionParameters.Where(p => p.Key == "id").FirstOrDefault();
        var id = Convert.ToInt32(param.Value);

        // Invoke a service to check if the request should be allowed
        var isAllowed = securityService.IsAllowed(user, id);
        if (!isAllowed)
        {
             context.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
        }
    }
}
```
