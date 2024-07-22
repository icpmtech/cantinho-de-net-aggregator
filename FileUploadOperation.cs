
namespace MarketAnalyticHub
{
  using Microsoft.OpenApi.Models;
  using Swashbuckle.AspNetCore.SwaggerGen;
  using System.Linq;

  public class FileUploadOperation : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      var fileParameters = context.ApiDescription.ParameterDescriptions
          .Where(p => p.Type == typeof(IFormFile))
          .ToList();

      if (fileParameters.Count == 0)
        return;

      operation.RequestBody = new OpenApiRequestBody
      {
        Content = {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileParameters.ToDictionary(
                            p => p.Name,
                            p => new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            })
                    }
                }
            }
      };
    }
  }


}
