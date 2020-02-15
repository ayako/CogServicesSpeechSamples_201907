using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace STTBatchFunc202002
{
    public static class GetTranscriptonFunc
    {
        private static readonly string SpeechServiceKey = Environment.GetEnvironmentVariable("SpeechServiceKey");
        private static readonly string SpeechServiceLocation = Environment.GetEnvironmentVariable("SpeechServiceLocation");
        private static readonly string SpeechLanguage = Environment.GetEnvironmentVariable("SpeechLanguage");
        private static readonly string CustomSpeechUrl = "https://" + SpeechServiceLocation + ".cris.ai/api/speechtotext/v2.0/transcriptions";

        [FunctionName("GetTranscriptonFunc")]
        public static async Task Run(
            [BlobTrigger("wav/{wavFileName}", Connection = "BlobStorage")] CloudBlockBlob wavBlob, string wavFileName,
            [Blob("txt", FileAccess.Write, Connection = "BlobStorage")] CloudBlobContainer txtBlobContainer,
            ILogger log)
        {
            var httpClient = new HttpClient();

            var wavFileUrl = wavBlob.Container.Uri.AbsoluteUri + "/" + wavFileName;
            var postResponse = await PostTranscriptionsAsync(httpClient, wavBlob, wavFileName, wavFileUrl);
            if (postResponse.StatusCode == HttpStatusCode.Accepted)
            {
                var location = postResponse.Headers.Location.ToString();
                var transcriptionId = location.Substring(location.LastIndexOf("/") + 1);

                var transcriptResult = new TranscriptResult();
                while (transcriptResult.status != "Succeeded")
                {
                    switch (transcriptResult.status)
                    {
                        case null:
                        case "Not Started":
                        case "Running":

                            await Task.Delay(10000);

                            var getResponse = await GetTranscriptionAsync(httpClient, transcriptionId);
                            if (getResponse.StatusCode == HttpStatusCode.OK)
                            {
                                var resultStr = await getResponse.Content.ReadAsStringAsync();
                                transcriptResult = JsonSerializer.Deserialize<TranscriptResult>(resultStr);
                            }
                            else
                            {
                                log.LogInformation($"HttpRequest Error: Failed to get transcription");
                            }

                            break;

                        // case "Failed"
                        default: 
                            log.LogInformation($"HttpRequest Error: Transcription creation job failed");
                            break;
                    }
                }
                if (transcriptResult.status == "Succeeded")
                {
                    var txtFileUrl = transcriptResult.resultsUrls.channel_0;
                    await UploadTxtFileAsync(httpClient, txtBlobContainer, wavFileName, txtFileUrl);
                }

                var deleteResponse = await DeleteTranscriptionAsync(httpClient, transcriptionId);
                if (deleteResponse.StatusCode != HttpStatusCode.NoContent)
                {
                    log.LogInformation($"HttpRequest Error: Failed to delete transcription");
                }

                log.LogInformation($"C# Blob trigger function (GetTranscriptionFunc)\nProcessed Blob Name: wav/{wavFileName}, Size: {wavBlob.Properties.Length} Bytes");

            }
            else
            {
                log.LogInformation($"HttpRequest Error: Failed to create transcription job");
            }
            
        }


        private static async Task<HttpResponseMessage> PostTranscriptionsAsync(HttpClient httpClient, CloudBlockBlob blob, string wavFileName, string wavFileUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, CustomSpeechUrl);
            request.Headers.Add("Ocp-Apim-Subscription-Key", SpeechServiceKey);

            var policy = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(25),
                Permissions = SharedAccessBlobPermissions.Read,
            };

            var content = new TranscriptRequest()
            {
                RecordingsUrl = wavFileUrl + blob.GetSharedAccessSignature(policy),
                Name = wavFileName,
                Locale = SpeechLanguage,
            };

            request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, @"application/json");
            var response = await httpClient.SendAsync(request);
            return response;
        }

        private static async Task<HttpResponseMessage> GetTranscriptionAsync(HttpClient httpClient, string transcriptionId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{CustomSpeechUrl}/{transcriptionId}");
            request.Headers.Add("Ocp-Apim-Subscription-Key", SpeechServiceKey);
            var response = await httpClient.SendAsync(request);
            return response;
        }

        private static async Task<HttpResponseMessage> DeleteTranscriptionAsync(HttpClient httpClient, string transcriptionId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{CustomSpeechUrl}/{transcriptionId}");
            request.Headers.Add("Ocp-Apim-Subscription-Key", SpeechServiceKey);
            var response = await httpClient.SendAsync(request);
            return response;
        }

        private static async Task UploadTxtFileAsync(HttpClient httpClient, CloudBlobContainer outputContainer, string wavFileName, string txtFileUrl)
        {
            await outputContainer.CreateIfNotExistsAsync();
            var cloudBlockBlob = outputContainer.GetBlockBlobReference(wavFileName.Replace(".wav", ".json"));
            using (var txtFileStream = await httpClient.GetStreamAsync(txtFileUrl))
            {
                await cloudBlockBlob.UploadFromStreamAsync(txtFileStream);
            }
        }

    }
}
