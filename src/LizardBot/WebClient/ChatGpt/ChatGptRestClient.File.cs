using LizardBot.WebClient.ChatGpt.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace LizardBot.WebClient.ChatGpt
{
    /// <summary>
    /// ChatGpt용 RestClient.
    /// </summary>
    public partial class ChatGptRestClient
    {
        /// <summary>
        /// 파일 업로드.
        /// <see href="https://platform.openai.com/docs/api-reference/files/create?lang=curl"/>.
        /// </summary>
        public async Task<FileObj?> UploadFileAsync(Stream stream)
        {
            var request = new RestRequest("/v1/files");
            request
                .AddHeader("Authorization", _client.Config.SecretKey)
                .AddHeader("Content-Type", "multipart/form-data");

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                request.AddFile("file", memoryStream.ToArray(), "test.txt");
            }

            request.AddParameter("purpose", "assistants");

            var response = await _client.PostAsync(request);
            ArgumentNullException.ThrowIfNull(response.Content);
            return JsonConvert.DeserializeObject<FileObj>(response.Content) ?? null;
        }

        public async Task<VectorStoreFileObj?> CreateVectorStoreFileAsync(string fileId, string vectorStoreId)
        {
            var request = new RestRequest($"/v1/vector_stores/{vectorStoreId}/files");
            request
                .AddHeader("Content-Type", "application/json")
                .AddHeader("Authorization", _client.Config.SecretKey)
                .AddHeader("OpenAI-Beta", "assistants=v2");

            var body = new Dictionary<string, string>
            {
                { "file_id", fileId },
            };

            request.AddBody(body);
            var response = await _client.PostAsync(request);
            ArgumentNullException.ThrowIfNull(response.Content);
            return JsonConvert.DeserializeObject<VectorStoreFileObj>(response.Content) ?? null;
        }
    }
}
