using Microsoft.EntityFrameworkCore;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
namespace MarketAnalyticHub.Controllers;

public static class CompanyEndpoints
{
    public static void MapCompanyEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Company").WithTags(nameof(Company));

        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            return await db.Companies.ToListAsync();
        })
        .WithName("GetAllCompanies")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Company>, NotFound>> (int id, ApplicationDbContext db) =>
        {
            return await db.Companies.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Company model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetCompanyById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Company company, ApplicationDbContext db) =>
        {
            var affected = await db.Companies
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, company.Id)
                    .SetProperty(m => m.Name, company.Name)
                    .SetProperty(m => m.Description, company.Description)
                    .SetProperty(m => m.Industry, company.Industry)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateCompany")
        .WithOpenApi();

        group.MapPost("/", async (Company company, ApplicationDbContext db) =>
        {
            db.Companies.Add(company);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Company/{company.Id}",company);
        })
        .WithName("CreateCompany")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, ApplicationDbContext db) =>
        {
            var affected = await db.Companies
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteCompany")
        .WithOpenApi();
    }
}
