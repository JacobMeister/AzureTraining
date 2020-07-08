using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace QueueToTable
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run(
            [QueueTrigger("form-app-queue", Connection = "AzureWebJobsStorage")]Person queueItem,
            ILogger log)
        {
            SaveToTable(queueItem);
        }

        static async Task<PersonEntity> SaveToTable(Person item)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=azuretrainingformapp;AccountKey=i7S4Wnoi5sX5oH4kKUwAXLnakmQaf7JgIoW//MXJ0Ew1KkTu6y9IkhKe3zCNmVCigsU23gwdwIY/VbTX8vpHRg==;EndpointSuffix=core.windows.net");
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference("registered");
            TableOperation insertOperation = TableOperation.Insert(new PersonEntity(item.Name, item.Name, item.Name, item.Image, item.Thumbnail));
            var result = await table.ExecuteAsync(insertOperation).ConfigureAwait(true);
            return result.Result as PersonEntity;
        }

    }

    public class PersonEntity : TableEntity
    {
        public PersonEntity(string partitionKey, string rowKey, string name, string image, string thumbnail)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Name = name;
            Image = image;
            Thumbnail = thumbnail;
        }

        public string Name { get; set; }
        public string Image { get; set; }
        public string Thumbnail { get; set; }
        
    }

    public class Person
    {
        public Person(string name, string image, string thumbnail)
        {
            Name = name;
            Image = image;
            Thumbnail = thumbnail;
        }

        public string Name { get; set; }
        public string Image { get; set; }
        public string Thumbnail { get; set; }

    }
}
