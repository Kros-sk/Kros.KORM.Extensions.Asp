# Kros.KORM.Extensions.Asp

For simple integration into ASP.NET Core projects, the [__Kros.KORM.Extensions.Asp__](https://www.nuget.org/packages/Kros.KORM.Extensions.Asp/) package was created.
[![Build status](https://ci.appveyor.com/api/projects/status/xebjpdbakd45mfs4/branch/master?svg=true)](https://ci.appveyor.com/project/Kros/kros-libs-u2wo6/branch/master)

### Download

Kros.KORM.Extensions.Asp is available from __Nuget__ [__Kros.KORM.Extensions.Asp__](https://www.nuget.org/packages/Kros.KORM.Extensions.Asp/).

## Contributing Guide

To contribute with new topics/information or make changes, see [contributing](https://github.com/Kros-sk/Kros.KORM.Extensions.Asp/blob/master/CONTRIBUTING.md) for instructions and guidelines.

## This topic contains following sections

* [ASP.NET Core extensions](#aspnet-core-extensions)
* [Database migrations](#database-migrations)

### ASP.NET Core extensions

You can use the `AddKorm` extension method to register `IDatabase` to the DI container.

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddKorm(Configuration);
}
```

The configuration file *(typically `appsettings.json`)* must contain a section `ConnectionString`.
```
  "ConnectionString": {
    "ProviderName": "System.Data.SqlClient",
    "ConnectionString": "Server=servername\\instancename;Initial Catalog=database;Persist Security Info=False;"
  }
```

If you need to initialize the database for [IIdGenerator](https://kros-sk.github.io/Kros.Libs.Documentation/api/Kros.Utils/Kros.Data.IIdGenerator.html) then you can call `InitDatabaseForIdGenerator`.

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddKorm(Configuration)
        .InitDatabaseForIdGenerator();
}
```

### Database migrations
For simple database migration, you must call:
```C#
public void ConfigureServices(IServiceCollection services)
{
  services.AddKorm(Configuration)
	.AddKormMigrations(Configuration)
	.Migrate();
}
```
The previous code requires the `KormMigration` section in the configurations:

```json
"KormMigrations": {
  "ConnectionString": {
    "ProviderName": "System.Data.SqlClient",
    "ConnectionString": "Server=servername\\instancename;Initial Catalog=database;Persist Security Info=False;"
  },
  "AutoMigrate": "True"
}
```

Korm performs migrations that default searches in the main assembly in the `Sql_scripts` directory. The script file name must match pattern `{migrationId}_{MigrationName}.sql`.
`MigrationId` is increasing number over time.

For example: `20190301001_AddPeopleTable.sql`
```sql
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].People(
	[Id] [int] NOT NULL,
	[Name] [nvarchar](255)
CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED
(
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
```

You can disable auto migrations when you start the service by setting `AutoUpgrade: False`.

Migration can also be executed through an HTTP query. By calling the `../kormmigration` endpoint, the necessary migration will be executed.
However, you need to add middleware:

```CSharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
  app.UseKormMigrations(o =>
  {
    o.EndpointUrl = "/kormmigration";
  });
}
```

If you have scripts stored in a different way (for example, somewhere on a disk or in another assembly, ...), you can configure your own providers to get these scripts.

```CSharp
public void ConfigureServices(IServiceCollection services)
{
  services.AddKorm(Configuration)
    .AddKormMigrations(Configuration, o =>
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.StartWith("Demo.DatabaseLayer"));
        o.AddAssemblyScriptsProvider(assembly, "Demo.DatabaseLayer.Resources");
        o.AddFileScriptsProvider(@"C:\scripts\");
        o.AddScriptsProvider(new MyCustomScriptsProvider());
    })
    .Migrate();
}
```

KORM creates a `__KormMigrationsHistory` table in which it has a history of individual migrations.
