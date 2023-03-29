# .NET Union Startup Template with Multitenancy Support

This repositoy contains a project template for rapid development of new projects.  
The solution contains a RESTful API that follows the Onion architecture with ASP.NET Core and .NET 6. The Onion architecture is also commonly known as the "Clean architecture" or "Ports and adapters", these architectural approaches are just variations of the same theme.

There are multiple ways that we can split the onion, but we are going to choose the following approach where weare going to split the architecture into 4 layers and one outer layer  

Conceptually, we can consider that the Infrastructure and Presentation layers are on the same level of the hierarchy.

* Domain Layer
* Service Layer
* Infrastructure Layer
* Presentation Layer

## Advantages of the Onion Architecture
All of the layers interact with each other strictly through the interfaces defined in the layers below. The flow of dependencies is towards the core of the Onion.

Using dependency inversion throughout the project, depending on abstractions (interfaces) and not the implementations, allows us to switch out the implementation at runtime transparently. We are depending on abstractions at compile-time, which gives us strict contracts to work with, and we are being provided with the implementation at runtime.

##  Domain Layer
* Entities
* Repository interfaces
* Exceptions
* Domain services

 <em>These is what I defined in the Domain layer. You can be more or less strict, depending on our needs. </em>
 
 The entities defined in the Domain layer are going to capture the information that is important for describing the problem domain. 
 
 Isn’t an anemic domain model a bad thing? It depends. If you have very complex business logic, it would make sense to encapsulate it inside of  domain entities. But for most applications, it is usually easier to start with a simpler domain model, and only introduce complexity if it is required by the project.


## Service Layer
* Services.Abstractions
* Services

The Service layer is right above the Domain layer, this means that it has a reference to the Domain layer. 

## Infrastructure Layer
* Database
* Identity provider
* Messaging queue
* Email service
* etc

The Infrastructure layer should be concerned with encapsulating anything related to external systems or services that the application is interacting with. 

All the implementation details are hiden in the Infrastructure layer because it is at the top of the Onion architecture, while all of the lower layers depend on the interfaces (abstractions).


## Presentation Layer
*  Web API built with standard ASP.NET Core

Represent the entry point to the system so that consumers can interact with the data. This layer can be implemented in many ways, for example creating a REST API, gRPC, etc.

 Presentation project will only have a reference to the Services.Abstraction project. And since the Services.Abstractions project does not reference any other project, we have imposed a very strict set of methods that we can call inside of the controllers.

*Utilities project is used to support extensibility of common operations as: openapi support, azure kv, etc...*

### Generic async repositories
TBD

### Generic business services
TBD

#### How to run

Set Web project as startup project
Update appsettings.json file Database ConnectionString value accordingly.
``` 
"Database": {
    "ConnectionString": "Server={SQL SERVER};User ID=={sql user};Password={sql password};Initial Catalog=AppCatalog;Persist Security Info=True; MultipleActiveResultSets=True;"
```
*A nugget package is used for automatic migration*

Execute run
