# ASP.NET MVC

## Areas

5.2.2 Create structure for Area ProductManage

```
dotnet aspnet-codegenerator area ProductManage

dotnet aspnet-codegenerator controller -name Product -outDir Areas\ProductManage\Controllers\ -namespace AppMVC.Areas.ProductManage.Controllers
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
