using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace STTBatchWeb202001.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IFormFile WavFile { get; set; }
        public List<STTEntity> SttEntityList { get; set; }


        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        private readonly CloudTableClient _tableClient;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _storageSasToken;

        private const string TableName = "log";
        private const string WavBlobName = "wav";


        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var StorageConnectionString = _configuration["StorageConnectionString"];

            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            _tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            var blobServiceClient = new BlobServiceClient(StorageConnectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(WavBlobName);

            var policy = new SharedAccessAccountPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(25),
                //SharedAccessExpiryTime = DateTime.UtcNow.AddDays(3),
                Permissions = SharedAccessAccountPermissions.Read,
                Services = SharedAccessAccountServices.Blob,
                ResourceTypes = SharedAccessAccountResourceTypes.Object
            };
            _storageSasToken = storageAccount.GetSharedAccessSignature(policy);

        }

        public async Task<IActionResult> OnGet()
        {
            var sttEntityList = await GetTableItemsAsync(_tableClient);
            SttEntityList = AddSasToken(sttEntityList);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var userName = "user01";
            var operationId = userName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var wavFileName = operationId + "_" + WavFile.FileName;
            var txtFileName = operationId + "_" + WavFile.FileName.Replace("wav", "json");

            var wavFileUrl = await UploadWavFileAsync(_blobContainerClient, operationId, wavFileName, WavFile);

            var entity = new STTEntity()
            {
                PartitionKey = userName,
                RowKey = operationId,
                WavFileName = wavFileName,
                WavFileUrl = wavFileUrl,
                TxtFileName = txtFileName,
            };

            await InsertTableItemAsync(_tableClient, entity);

            WavFile = null;

            var sttEntityList = await GetTableItemsAsync(_tableClient);
            SttEntityList = AddSasToken(sttEntityList);

            return Page();
        }

        public async Task<List<STTEntity>> GetTableItemsAsync(CloudTableClient tableClient)
        {
            var table = tableClient.GetTableReference(TableName);
            var query = new TableQuery<STTEntity>();

            var result = await table.ExecuteQuerySegmentedAsync<STTEntity>(query, null);
            var entity = result.ToList();
            return entity;
        }

        public async Task InsertTableItemAsync(CloudTableClient tableClient, TableEntity entity)
        {
            var table = tableClient.GetTableReference(TableName);

            var operation = TableOperation.InsertOrMerge(entity);
            await table.ExecuteAsync(operation);
        }

        public async Task<string> UploadWavFileAsync(BlobContainerClient blobContainerClient, string operationId, string wavFileName,IFormFile wavFile)
        {
            var blobClient = blobContainerClient.GetBlobClient(wavFileName);
            await blobClient.UploadAsync(wavFile.OpenReadStream());
            var result = blobClient.Uri.AbsoluteUri.ToString();
            return result;
        }

        private List<STTEntity> AddSasToken(List<STTEntity> sttEntityList)
        {
            foreach (var entity in sttEntityList)
            {
                if (entity.WavFileUrl != null)
                    entity.WavFileUrl += _storageSasToken;
                if (entity.TxtFileUrl != null)
                    entity.TxtFileUrl += _storageSasToken;
            }

            return sttEntityList;
        }

    }
}
