---
title: "Runtime Shared Libraries with Plain ActionScript"
date: 2014-10-05T14:22:23Z
slug: runtime-shared-libraries-with-plain-actionscript
aliases: ["/runtime-shared-libraries-with-plain-actionscript/"]
categories:
  - "Technique"
tags:
  - "actionscript"
wp_post_id: 784
---

Using RSLs is still a bit of a black art in ActionScript. This is partly due to the fact that documentation on it is sparse, partly because of the complex ActionScript code that mxmlc generates when compiling MXML files, and partly because the process itself is a bit unintuitive. These notes are the result of a few days of research on the topic.

A runtime shared library is simply a SWF file that is loaded into the application at launch. The code and assets in the library are then available for the application to use. This makes it handy when using third-party code libraries, or even your own libraries which are built to a published specification and are not going to change frequently. By making them external to the application, you can also share them between several different applications.

### The Flex Way

ActionScript developers working with mxmlc directly outside the Flash Builder IDE may be familiar with the following warning.

```
/Projects/rsl-demo/Main.as: Warning: This compilation unit did not have a factoryClass specified in Frame metadata to load the configured runtime shared libraries. To compile without runtime shared libraries either set the -static-link-runtime-shared-libraries option to true or remove the -runtime-shared-libraries option.
```

This warning indicates that the programmer may have overlooked the task of loading external libraries at runtime. The Flash Player will throw a runtime exception and halt further execution of the application if attempts are made to execute code which is not yet loaded.

The solution is to add a Frame metatag at the top of the application class and set its factoryClass attribute to point to a class that will be responsible to load external libraries.

If you were to inspect the output of the mxmlc compiler on a Flex application, you will see hard-coded references to all the RSLs that the application is using in the factoryClass designate. These references come from the configuration options passed on to the compiler through either its command-line parameters or the compiler configuration file.

The class that contains these references is usually a child of the mx.managers.SystemManager class and generated automatically by mxmlc. The SystemManager class provides the infrastructure to load these files along with error handling and progress feedback to the user.

This generated class also contains a reference to the entry point class - the one that extends from the Application class. When all the library files are loaded, the framework instantiates this class and adds it to the display list of the factoryClass designate. This makes the factoryClass the root document class, while the Application-derived class is actually a child of the loader in the display graph.

The programmer still provides the Application-inheriting class as the compiler target. But when the compiler encounters the Frame metatag, it automatically associates the Preloader class with the document root and makes the Application a child of the Preloader.

### An ActionScript Implementation

When using ActionScript directly instead of the Flex framework, the developer must manually add the Frame metatag to the top of the application entry point class.

```actionscript
[Frame(factoryClass="Preloader")]
public class Main extends Sprite
{

}
```

#### The Preloader Class

The preloading is a straightforward consumption of the flash.display.Loader API. It is implemented here using the Preloader class. This class must fetch every external library file needed by the application. The paths to the libraries are supplied to the Preloader class. When using the Flex framework, the mxmlc compiler bakes in the references to the library files into the code that it generates. The example below also uses the same technique. However, the URLs of library files can also be supplied from any other source such as a web service or external text file. All standard Flash Player APIs are already available to the Preloader class.

```actionscript
public function Preloader()
{
	var loader:Loader = new Loader();
	loader.contentLoaderInfo.addEventListener(Event.COMPLETE, this.loader_completeHandler);
	loader.contentLoaderInfo.addEventListener(IOErrorEvent.IO_ERROR, this.loader_ioErrorHandler);
	var request:URLRequest = new URLRequest("math.swf");
	var context:LoaderContext = new LoaderContext(false, ApplicationDomain.currentDomain);
	loader.load(request, context);
}
```

It is important to note that the library has to be loaded into the same application domain as the main application. Otherwise, it will not have access to the classes in the library and any attempt to instantiate them will trigger a VerifyError at runtime.

```
VerifyError: Error #1014: Class com.notadesigner.math::IntegerArithmetic could not be found.
```

#### Using the Runtime Shared Library API

After the library has been downloaded, the Preloader class instantiates the application class and adds it to the display list. The developer must use the flash.system.getDefinitionByName API to get a reference to the application class. This is necessary because application class contains a reference to the IntegerArithmetic class. If the Preloader references Main directly, the compiler will pick up the complete chain of references and statically link the IntegerArithmetic class into the application SWF. By deferring to reference the application class until runtime, the compiler is prevented from scanning the dependency chain and statically linking the library classes into the application SWF.

```actionscript
private function loader_completeHandler(event:Event):void
{
	var mainClass:Class = getDefinitionByName("Main") as Class;
	var mainInstance:Main = new mainClass();
	this.addChild(mainInstance);
}
```

The Main class then continues with its business as normal. In this case, it is instantiating a type declared in the library and calling its method.

```actionscript
public function Main()
{
	var integer:IntegerArithmetic = new IntegerArithmetic(); // Type declared in math.swf
	var operand1:int = 10;
	var operand2:int = 10;
	var result:int = integer.add(operand1, operand2);
}
```

#### Deploying Runtime Shared Libraries

The confusing bit about using a runtime shared library is realizing that the SWF has to be extracted from the SWC at the time of deploying the application. This was not immediately obvious and I ended up spending days placing a compiled SWC file in various locations and wondering why the application was unable to load it at runtime. An obscure article on the Adobe website made explicit this particular step and set things straight.

Again, when placing the files, standard path rules apply. The Preloader can refer to relative or absolute paths. If the files are on external domains, the Flash Player attempts to fetch a crossdomain policy file before attempting to download the SWF. The policy file is to be specified as an additional value to the -runtime-shared-library-path parameter to mxmlc.
