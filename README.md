# ContosoCrafts (Dapr Edition)

This repo contains a sample application that shows how to integrate Stripe Checkout into an ASP.NET Core application. The solution contains three projects.

- [Blazor Frontend](src/ContosoCrafts.Web.Client)
- [ASP.NET Core Backend](src/ContosoCrafts.Web.Server)
- [Shared Models](src/ContosoCrafts.Web.Shared)

## Spinning up the environment

Build and run the `ContosoCrafts.Web.Server` project.

```bash
> cd src/ContosoCrafts.Web.Server
> dotnet run
```

### Requirements

- Visual Studio Code
- .NET 5 SDK
- A Stripe test account
