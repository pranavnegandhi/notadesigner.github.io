# Valknut &ndash; Localisation

## Client-specific Regional Settings

### Request Structure

Valknut implements localisation of temporal data to match the client's preferred locale through the use of the `Accept-Language` header in an incoming request. The value of this header can be an array of one or more language tags, such as en-US, along with a weightage indicator for the language.

This value is set automatically by the browser based on the regional settings of the operating system, and can be overridden by the user to suit their needs. The language tag may be followed by an optional quality factor parameter that indicates the preferred weightage to the given language tag. The value of the q-factor is a decimal number between 0 to 1.

    en-US;q=1,en-GB;0.5

The above example indicates that the client prefers American as well as British English, with a higher preference for American English.

An example of an entire `Accept-Language` header is shown below.

    Accept-Language: en, en-GB;q=0.8, hi;q=0.7

This is equivalent to having the value `en;q=1,en-GB;q=0.8,hi;q=0.7`.

### Server-side Extraction

On the server side, the framework parses the header and automatically populates the `UserLanguages` property of the request object. The type of `UserLanguages` is `string[]`, and its entries are sorted in descending order by the quality factor. In the example above, the language tag en is the first entry, and hi is the last entry.

Once the request language has been determined, the method checks if it is whitelisted in the allowedLanguages parameter. It first checks if the entire language tag and its subtag are present in the whitelist, or failing that, if the language tag alone is found. That way, if the application only supports French language localisation, but the request contains the tag fr-CA, the response is still localised to the French language, even though it does not contain Canadian region-specific date or number formatting.

In spite of all this, if the method is unable to determine the preferred language from the request, it applies the culture that is currently active on the operating system.

### Request Handling

The server matches the current culture of the thread fulfilling the request to a value from this array in order to process the request in a culture-aware manner. Currently this extends to temporal data, with scope for future support for UI string localisation, formatting of numerical data, units of measurement and iconography.

The code to configure this for the current thread is written in an extension method on the `System.Web.HttpApplication` class.

    void SetLocale(this HttpApplication application, 
        string culture = null, 
        string uiCulture = null, 
        bool setUiCulture = true, 
        string allowedLocales = null)

This method is invoked from the `Application_BeginRequest` event handler, because the locale has to be set separately for each request. The `SetLocale` method is called directly from this event handler. The method is invoked without any parameters, which indicates that the preferred locale has to be inferred from the request headers.

If the `Accept-Language` header is not present in the request, the method falls back upon the regional settings configured on the server to handle the request.

The `Thread` class has two properties to set the culture.

1. CurrentCulture, which determines the writing system (LTR or RTL), calendar, string sorting and formatting of date and time strings.

2. CurrentUICulture, which determines the resource file used by the Resource Manager to localise the strings, icons and other non-code assets for the application.

This separation allows applications to be able to handle regional settings for data even if it does not support region-specific UI labels. The `SetLocale` method sets both properties to the same culture unless the value of `setUiCulture` is false.
