# Firepuma-Payments-MicroService

A repository containing code for a payments micro service.

[![Deploy function app](https://github.com/francoishill/Firepuma-Payments-MicroService/actions/workflows/deploy-function-app.yml/badge.svg)](https://github.com/francoishill/Firepuma-Payments-MicroService/actions/workflows/deploy-function-app.yml)
[![Deploy manager function app](https://github.com/francoishill/Firepuma-Payments-MicroService/actions/workflows/deploy-function-app-manager.yml/badge.svg)](https://github.com/francoishill/Firepuma-Payments-MicroService/actions/workflows/deploy-function-app-manager.yml)

## The plan

The plan of building this repo contained in these steps, not necessarily in this order:

* [x] Scaffold code with a single payment gateway (PayFast)
* [x] Deploy with Github Actions
* [x] Test APIs flow manually (use a dummy app with PayFast sandbox config)
* [x] Create functions to manage client applications (create function host key and add the application config)
* [x] Write a rudimentary sample web app to interact with the manager function app
* [x] Add a sample application that uses the Client library, by directly importing the client csproj
    * [x] Ensure to look through TODOs in code
    * [x] Fill in correct values in sample project `appsettings.json`
    * [x] Add user secrets for `PaymentsMicroservice->AuthorizationCode` and `PaymentsMicroservice->ServiceBusConnectionString`
* [x] Deploy a client library to Nuget
* [ ] Fix all the `//FIX` TODO comments
* [ ] Unit tests
* [ ] Create a template and add a "Launch in Azure" button
* [ ] Add ability to extend it with other payment gateways
* [ ] Find a native Azure way to authenticate the function calls (instead of 'code') and that can automatically derive the "Application Id" from the auth token
* [ ] Change sample application to use published Nuget client library instead of directly importing the client csproj

--------------------------------------------------------------------

## Local Development

### Firepuma.Payments.FunctionApp

To run the `Firepuma.Payments.FunctionApp` locally, you will need to add the following Values to your `local.settings.json` file:

| Name                                               | Description                                                                                                                                                                                                            |
|----------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ServiceBus                                         | A connection string to the service bus that will be used internally for the payments service (to drop notifications of payment systems unto it for better robustness)                                                  |
| QueueName                                          | The queue name to use in the service bus                                                                                                                                                                               |
| FirepumaValidateAndStorePaymentNotificationBaseUrl | Used for the callback/notification of 3rd party services; if you use [`ngrok http 7071`](https://ngrok.com/), it will be something like https://123c-123-123-22-33.sa.ngrok.io/api/ValidateAndStorePaymentNotification |
| EventGridEndpoint                                  | The Azure Event Grid custom topic endpoint, ie. https://YOUR-GRID-TOPIC.eastus2-1.eventgrid.azure.net/api/events                                                                                                       |
| EventGridAccessKey                                 | Access key to your event grid                                                                                                                                                                                          |

### Firepuma.Payments.FunctionAppManager

To run the `Firepuma.Payments.FunctionAppManager` locally, you will need to add the following Values to your `local.settings.json` file:

| Name                        | Description                                                                          |
|-----------------------------|--------------------------------------------------------------------------------------|
| PaymentsServiceFunctionsUrl | http://localhost:7071                                                                |
| PaymentsServiceFunctionsKey | Just fill in something random as it is not used locally, like use `no-key-for-local` |

### Nuget packages

For local development of the nuget packages you can follow these steps or inspect [.github/workflows/publish-nuget-packages.yml](.github/workflows/publish-nuget-packages.yml):

Ensure you add nuget source called "Local feed" which points to a local directory (documentation for [Rider](https://www.jetbrains.com/help/rider/2022.1/Reference_Windows_NuGet.html#sources-tab)).

Run the following commands:

```shell
cd [root folder of this solution]
dotnet pack --configuration Release --output . -p:PackageVersion=0.0.1-dev Firepuma.Payments.Core/Firepuma.Payments.Core.csproj
dotnet pack --configuration Release --output . -p:PackageVersion=0.0.1-dev Firepuma.Payments.Client/Firepuma.Payments.Client.csproj
dotnet nuget push *.nupkg --source "Local feed"
```

### Credits to

* [ShawnShiSS/clean-architecture-azure-cosmos-db](https://github.com/ShawnShiSS/clean-architecture-azure-cosmos-db)