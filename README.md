# ASP.NET MVC

## Areas

5.2.2 Create structure for Area ProductManage

```
dotnet aspnet-codegenerator area ProductManage => Create folder

dotnet aspnet-codegenerator controller -name Product -outDir Areas\ProductManage\Controllers\ -namespace AppMVC.Areas.ProductManage.Controllers => Create Controller

dotnet aspnet-codegenerator controller -name Contact -outDir Areas\Contact\Controllers\ -namespace AppMVC.Areas.Contact.Controllers -m AppMVC.Models.Contacts.Contact -udl -dc AppMVC.Models.AppDbContext => Create Controller & CRUD code
```

Areas/ProductManage/Controllers => Create ProductController.cs
Areas/ProductManage/Views => Create Product/Index.cshtml

## Integrate Entity Framework into AppMvc

1. Install packages

```
dotnet new mvc

dotnet tool install --global dotnet-ef
dotnet tool install --global dotnet-aspnet-codegenerator

dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package MySql.Data.EntityFramework
```

2. Create AppDbContext & Inject

3. Migrations

```
dotnet ef migrations add Init
dotnet ef database update
```

## Create Contacts table in DB

1. Create Contact Model & add to AppDbContext
2. dotnet ef migrations add Contact & dotnet ef database update
3. Create Controller & CRUD code for Contact
