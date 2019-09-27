# Kros.KORM.Extensions.Asp [![Build Status](https://dev.azure.com/krossk/DevShared/_apis/build/status/Kros.KORM.Extensions.Asp/Kros.KORM.Extensions.Asp?branchName=features/build)](https://dev.azure.com/krossk/DevShared/_build/latest?definitionId=69&branchName=master)

For simple integration into ASP.NET Core projects, the [__Kros.KORM.Extensions.Asp__](https://www.nuget.org/packages/Kros.KORM.Extensions.Asp/) package was created.

## Documentation

For configuration, general information and examples [see the documentation](https://kros-sk.github.io/docs/Kros.KORM.Extensions.Asp/).

### Download

Kros.KORM.Extensions.Asp is available from __Nuget__ [__Kros.KORM.Extensions.Asp__](https://www.nuget.org/packages/Kros.KORM.Extensions.Asp/).

## Contributing Guide

To contribute with new topics/information or make changes, see [contributing](https://github.com/Kros-sk/Kros.KORM.Extensions.Asp/blob/master/CONTRIBUTING.md) for instructions and guidelines.

## This topic contains following sections

* [ASP.NET Core Extensions](#aspnet-core-extensions)
* [Database Migrations](#database-migrations)
* [Id Generators](#id-generators)

### ASP.NET Core Extensions

You can use the `AddKorm` extension methods to register databases to the DI container. This registers `IDatabaseFactory` into DI container. This factory can be used to retrieve `IDatabase` instances by name. If no name is specified, default name `DefaultConnection` will be used. `IDatabase` instances has scoped lifetime.

**The first** database registered by `AddKorm` method is also added to the DI container directly as `IDatabase` dependency. This is for simple use case, when only one database is used. So there is no need for using `IDatabaseFactory`.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddKorm(Configuration);
}
```

The configuration file *(typically `appsettings.json`)* must contain a standard connection strings section (`ConnectionStrings`). Name of the connection string can be specified as second parameter. If the name is not specified, default name `DefaultConnection` will be used.

``` json
"ConnectionStrings": {
  "DefaultConnection": "Server=ServerName\\InstanceName; Initial Catalog=database; Integrated Security=true",
  "localConnection": "Server=Server2\\Instance; Integrated Security=true;"
}
```

Connection string can be passed directly to `AddKorm` method, together with its name. The name `DefaultConnection` will be used if no name is specified.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    // Added from appsettings.json under "localConnection" name.
    services.AddKorm(Configuration, "localConnection");

    // Added directly with the name "db2".
    services.AddKorm("Server=ServerName\\InstanceName; Initial Catalog=database; Integrated Security=true", "db2");
}
```

KORM supports additional settings for connections:

* `AutoMigrate`: The value is boolean `true`/`false`. If not set (or the value is invalid), the default value is `false`. If it is `true`, it allows automatic [database migrations](#database-migrations).
* `KormProvider`: This specifies database provider which will be used. If not set, the value `System.Data.SqlClient` will be used. KORM currently supports only Microsoft SQL Server, so there is no need to use this parameter.

These settings (if needed) can also be set in configuration file under the `KormSettings` section. Settings are identified by connection string name.

``` json
"KormSettings": {
  "DefaultConnection": {
    "AutoMigrate": true
  },
  "localConnection": {
    "AutoMigrate": true
  }
}
```

### Database Migrations

For simple database migration, you must call:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddKorm(Configuration)
        .AddKormMigrations()
        .Migrate();
}
```

Migrations are disabled by default, so the previous code requires that the automatic migrations are enabled in KormSettings for each connection string: `AutoMigrate=true`

Korm by default performs migrations by searching the main assembly for files in `SqlScripts` directory. The script file name must match pattern `{migrationId}_{MigrationName}.sql`. `MigrationId` is increasing number over time.

For example: `20190301001_AddPeopleTable.sql`

``` sql
CREATE TABLE [dbo].People (
    [Id] [int] NOT NULL,
    [Name] [nvarchar](255)
CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED ([Id] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
```

Migration can also be executed through an HTTP request. By calling the `/kormmigration` endpoint, the necessary migrations will be executed. However, you need to add middleware:

``` csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseKormMigrations();
}
```

You can change the endpoint URL by configuration:

``` csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseKormMigrations(options =>
    {
        options.EndpointUrl = "/loremipsum";
    });
}
```

If multiple KORM databases are registered, all of them have unique name. Migrations are performed per database and the name of the database is specified in URL as another path segment: `/kormmigration/dbname` If the name is not specified, default connection string will be used (`DefaultConnection`).

If you have scripts stored in a different way (for example, somewhere on a disk or in another assembly), you can configure your own providers to get these scripts.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddKorm(Configuration)
        .AddKormMigrations(o =>
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()    .FirstOrDefault(x => x.FullName.StartsWith  ("Demo.DatabaseLayer")  );
            o.AddAssemblyScriptsProvider(assembly,     "Demo.DatabaseLayer.Resources");
            o.AddFileScriptsProvider(@"C:\scripts\");
            o.AddScriptsProvider(new MyCustomScriptsProvider());
        })
        .Migrate();
}
```

KORM creates a `__KormMigrationsHistory` table in which it has a history of individual migrations.

### Id Generators

If you need to initialize the database for [IIdGenerator](https://kros-sk.github.io/Kros.Libs.Documentation/api/Kros.Utils/Kros.Data.IIdGenerator.html) then you can call `InitDatabaseForIdGenerator`.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddKorm(Configuration)
        .InitDatabaseForIdGenerator();
}
```
