# .net6-startup-template

This repositoy contains a project template for rapid development of new projects.  
The solution contains a RESTful API that follows the Onion architecture with ASP.NET Core and .NET 6. The Onion architecture is also commonly known as the "Clean architecture" or "Ports and adapters", these architectural approaches are just variations of the same theme.

There are multiple ways that we can split the onion, but we are going to choose the following approach where weare going to split the architecture into 4 layers and one outer layer  

* Domain Layer
* Service Layer
* Infrastructure Layer
* Presentation Layer

##  Domain Layer
* Entities
* Repository interfaces
* Exceptions


## Service Layer
* Services.Abstractions
* Services

## Infrastructure Layer
* Database
* Identity provider
* Messaging queue
* Email service
* etc

## Presentation Layer
*  Web AP built with standard ASP.NET Core


### Generic async repositories
TBD

### Generic business services
TBD

#### How to run

Set Web project as startup project
Update appsettings.json file Database ConnectionString value accordingly.
``` 
"Database": {
    "ConnectionString": "Server={SQL SERVER};User ID=={sql user};Password={sql password};Initial Catalog=TW.Core;Persist Security Info=True; MultipleActiveResultSets=True;"
```
*A nugget package is used for automatic migration*

Execute run
