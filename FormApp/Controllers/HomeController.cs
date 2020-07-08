using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FormApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Microsoft.Azure.Cosmos.Table.CloudStorageAccount storageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=azuretrainingformapp;AccountKey=i7S4Wnoi5sX5oH4kKUwAXLnakmQaf7JgIoW//MXJ0Ew1KkTu6y9IkhKe3zCNmVCigsU23gwdwIY/VbTX8vpHRg==;EndpointSuffix=core.windows.net");
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference("registered");
            var registeredPeople = table.ExecuteQuery(new TableQuery<PersonEntity>()).ToList();
            ViewBag.RegisteredPeople = registeredPeople;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult Index(string name, HttpPostedFileBase photo)
        {
            if(photo == null || name == null)
            {
                ViewBag.Name = "Er is iets mis gegaan, probeer het opnieuw";
                return View();
            }
            ViewBag.Name = name;

            UploadImage(photo);
            UploadThumbnail(photo);
            var fileName = Path.GetFileName(photo.FileName);
            AddMessage(new Person(name, fileName, fileName));
            return RedirectToAction("/Index");
        }

        public static void UploadImage(HttpPostedFileBase photo)
        {
            var client = new BlobClient(
                "DefaultEndpointsProtocol=https;AccountName=azuretrainingformapp;AccountKey=i7S4Wnoi5sX5oH4kKUwAXLnakmQaf7JgIoW//MXJ0Ew1KkTu6y9IkhKe3zCNmVCigsU23gwdwIY/VbTX8vpHRg==;EndpointSuffix=core.windows.net",
                "images",
                Path.GetFileName(photo.FileName));
            client.UploadAsync(photo.InputStream);
        }


        public static void UploadThumbnail(HttpPostedFileBase photo)
        {
            byte[] thumbnail;
            using (Image img = Image.FromStream(photo.InputStream))
            {
                int h = 100;
                int w = 100;

                using(Bitmap b = new Bitmap(img, new Size(w, h)))
            {
                    using (MemoryStream ms2 = new MemoryStream())
                    {
                        b.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);
                        thumbnail = ms2.ToArray();
                    }
                }
            }

            var client = new BlobClient(
                "DefaultEndpointsProtocol=https;AccountName=azuretrainingformapp;AccountKey=i7S4Wnoi5sX5oH4kKUwAXLnakmQaf7JgIoW//MXJ0Ew1KkTu6y9IkhKe3zCNmVCigsU23gwdwIY/VbTX8vpHRg==;EndpointSuffix=core.windows.net",
                "thumbnails",
                Path.GetFileName(photo.FileName));
            client.UploadAsync(new MemoryStream(thumbnail));
        }

        public static void AddMessage(Person person)
        {
            Microsoft.Azure.Storage.CloudStorageAccount storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=azuretrainingformapp;AccountKey=i7S4Wnoi5sX5oH4kKUwAXLnakmQaf7JgIoW//MXJ0Ew1KkTu6y9IkhKe3zCNmVCigsU23gwdwIY/VbTX8vpHRg==;EndpointSuffix=core.windows.net");
            CloudQueueClient cloudQueueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue cloudQueue = cloudQueueClient.GetQueueReference("form-app-queue");
            cloudQueue.CreateIfNotExists();
            CloudQueueMessage queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(person));
            cloudQueue.AddMessage(queueMessage);
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

            public PersonEntity() { }

            public string Name { get; set; }
            public string Image { get; set; }
            public string Thumbnail { get; set; }

        }

    }

}


