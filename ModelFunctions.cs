using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Internal;
using System.Linq;

namespace TemplateFunctions
{
    public static class ModelFunctions
    {
        [FunctionName("GetModels")]
        public static async Task<IActionResult> GetModels(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "models")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger get models function was called");

            string name = req.Query["name"]; //take Query parameter

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return new OkObjectResult(MemoryDatabase.Models);
        }

        [FunctionName("GetModelById")]
        public static async Task<IActionResult> GetModelById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "models/{modelId}")] HttpRequest req,
            ILogger log,
            string modelId)
        {
            var model = MemoryDatabase.Models.FirstOrDefault(t => t.Id == modelId);
            if (model == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(model);
        }

        [FunctionName("PostModels")]
        public static async Task<IActionResult> PostModels(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "models")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var modelsList = JsonConvert.DeserializeObject<List<PostModel>>(requestBody);

            var addedModels = new List<GetModel>();
            modelsList.ForEach(m => addedModels.Add(new GetModel
            {
                Description = m.Description,
                Name = m.Name
            }));
            MemoryDatabase.Models.AddRange(addedModels);


            return new OkObjectResult(addedModels);
        }

        [FunctionName("PostModel")]
        public static async Task<IActionResult> PostModel(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "models/model")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var postModel = JsonConvert.DeserializeObject<PostModel>(requestBody);

            var addedModel = new GetModel
            {
                Description = postModel.Description,
                Name = postModel.Name
            };
            MemoryDatabase.Models.Add(addedModel);


            return new OkObjectResult(addedModel);
        }

        [FunctionName("UpdateModel")]
        public static async Task<IActionResult> UpdateModel(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "models/{modelId}")] HttpRequest req,
            ILogger log,
            string modelId)
        {
            var model = MemoryDatabase.Models.FirstOrDefault(m => m.Id == modelId);
            if (model == null)
            {
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateModel = JsonConvert.DeserializeObject<GetModel>(requestBody);

            model.Description = updateModel.Description;
            model.Name = updateModel.Name;

            return new OkObjectResult(model);
        }

        [FunctionName("DeleteModel")]
        public static async Task<IActionResult> DeleteModel(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "models/{modelId}")] HttpRequest req,
            ILogger log,
            string modelId)
        {
            var model = MemoryDatabase.Models.FirstOrDefault(m => m.Id == modelId);
            if (model == null)
            {
                return new NotFoundResult();
            }

            MemoryDatabase.Models.Remove(model);
            return new OkResult();
        }
    }

    public class GetModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class PostModel
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }

    public static class MemoryDatabase
    {
        public static List<GetModel> Models = new List<GetModel>()
        {
            new GetModel
            {
                Name = "First",
                Description = "Desc1"
            },
            new GetModel
            {
                Name = "Second",
                Description = "Desc2"
            }
        };
    }
}
