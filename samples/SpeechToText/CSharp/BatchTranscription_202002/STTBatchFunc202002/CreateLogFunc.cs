using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace STTBatchFunc202002
{
    public static class CreateLogFunc
    {
        [FunctionName("CreateLogFunc")]
        public static async Task Run(
            [BlobTrigger("wav/{name}", Connection = "BlobStorage")] CloudBlockBlob blob, string name,
            [Table("log", Connection = "TableStorage")] CloudTable table,
            ILogger log)
        {
            await CreateTableItemAsync(table, blob, name);

            log.LogInformation($"C# Blob trigger function (CreateLogFunc)\nProcessed Blob Name:{name}");
        }

        private static async Task CreateTableItemAsync(CloudTable table, CloudBlockBlob blob, string wavFileName)
        {
            var wavFileUrl = blob.Container.GetBlockBlobReference(wavFileName).Uri.AbsoluteUri.ToString();

            var query = new TableQuery<STTEntity>();
            var result = await table.ExecuteQuerySegmentedAsync<STTEntity>(query, null);
            var entity = result.ToList<STTEntity>().Find(x => x.WavFileName == wavFileName);

            if (entity == null)
            {
                var newEntity = new STTEntity()
                {
                    PartitionKey = "direct_upload",
                    RowKey = "direct_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                    WavFileName = wavFileName,
                    WavFileUrl = wavFileUrl,
                    //TxtFileName = null,
                    //TxtFileUrl = null,
                };

                await table.ExecuteAsync(TableOperation.InsertOrMerge(newEntity));
            }

        }

        public class STTEntity : TableEntity
        {
            public STTEntity()
            { }

            public string WavFileUrl { get; set; }
            public string WavFileName { get; set; }
            public string TxtFileUrl { get; set; }
            public string TxtFileName { get; set; }
        }

    }
}