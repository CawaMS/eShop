using Azure;
using Azure.AI.OpenAI;
using eShop.Models;
using Microsoft.IdentityModel.Tokens;
using NRedisStack.Search.Literals.Enums;
using NRedisStack.Search;
using StackExchange.Redis;
using static NRedisStack.Search.Schema;
using NRedisStack.RedisStackCommands;
using NRedisStack;
namespace eShop.Data;
public class DescriptionEmbeddings
{

    public static async Task GenerateEmbeddingsInRedis(eShopContext eShopContext, ILogger logger, IConfiguration config)
    {
        string? redisConnection = config["redisConnection"];
        //<cache_name>.eastus.redisenterprise.cache.azure.net:10000,password=<primary_access_key>,ssl=True,abortConnect=False
        string? aoaiConnection = config["aoaiConnection"];
        string? aoaiKey = config["aoaiKey"];
        string? embeddingsDeploymentName = config["textEmbeddingsDeploymentName"];
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnection);
        IDatabase db = redis.GetDatabase();

        //https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/embeddings?tabs=csharp
        // initialize Azure open ai ada-text-embeddings service
        Uri aoaiEndpoint = new(aoaiConnection);

        AzureKeyCredential credentials = new(aoaiKey);

        OpenAIClient openAIClient = new OpenAIClient(aoaiEndpoint, credentials);

        IEnumerable<Product> productList = eShopContext.Product.ToList();
        foreach (var _product in productList) 
        {
            if ((db.HashGet("id:"+_product.Id, "description_embeddings").IsNullOrEmpty) && (_product.description != null))
            {
                db.HashSet("id:"+_product.Id,
                [
                    new("Name", _product.Name),
                    new("Price", _product.Price.ToString()),
                    new("Category", _product.category),
                    new("description", _product.description),
                    new("description_embeddings",textToEmbeddings(_product.description,openAIClient, embeddingsDeploymentName).SelectMany(BitConverter.GetBytes).ToArray())
                ]);
            }
        }

        SearchCommands ft = db.FT();
        // index each vector field
        try { ft.DropIndex("vss_products"); } catch { };
        Console.WriteLine("Creating search index in Redis");
        Console.WriteLine();
        ft.Create("vss_products", new FTCreateParams().On(IndexDataType.HASH).Prefix("id:"),
            new Schema()
            .AddTagField("Name")
            .AddVectorField("description_embeddings", VectorField.VectorAlgo.FLAT,
                new Dictionary<string, object>()
                {
                    ["TYPE"] = "FLOAT32",
                    ["DIM"] = 1536,
                    ["DISTANCE_METRIC"] = "L2"
                }
        ));
    }

    static float[] textToEmbeddings(string text, OpenAIClient _openAIClient, string embeddingsDeploymentName)
    {
        EmbeddingsOptions embeddingOptions = new EmbeddingsOptions()
        {
            DeploymentName = embeddingsDeploymentName,
            Input = { text },
        };

        return _openAIClient.GetEmbeddings(embeddingOptions).Value.Data[0].Embedding.ToArray();
    }
}

