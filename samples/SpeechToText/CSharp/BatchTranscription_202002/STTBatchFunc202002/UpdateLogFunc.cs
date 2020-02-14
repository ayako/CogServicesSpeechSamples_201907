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
    public static class UpdateLogFunc
    {
        [FunctionName("UpdateLogFunc")]
        public static async Task Run(
            [BlobTrigger("txt/{name}", Connection = "BlobStorage")] CloudBlockBlob blob, string name,
            [Table("log", Connection = "TableStorage")] CloudTable table,
            ILogger log)
        {
            await UpdateTableItemAsync(table, blob, name);

            log.LogInformation($"C# Blob trigger function (txtBlob) Processed blob\n Name:{name}");
        }

        private static async Task UpdateTableItemAsync(CloudTable table, CloudBlockBlob blob, string txtFileName)
        {
            var txtFileUrl = blob.Container.GetBlockBlobReference(txtFileName).Uri.AbsoluteUri.ToString();

            var wavFileName = txtFileName.Replace(".json", ".wav");
            var query = new TableQuery<STTEntity>();
            var result = await table.ExecuteQuerySegmentedAsync<STTEntity>(query, null);
            var entity = result.ToList<STTEntity>().Find(x => x.WavFileName == wavFileName);

            if (entity != null)
            {
                var newEntity = new STTEntity()
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    WavFileName = entity.WavFileName,
                    WavFileUrl = entity.WavFileUrl,
                    TxtFileName = txtFileName,
                    TxtFileUrl = txtFileUrl,
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
