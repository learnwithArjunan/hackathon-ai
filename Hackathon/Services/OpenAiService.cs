using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Hackathon.Services
{
    public class OpenAiService
    {
        private readonly string _apiKey = "";
        private readonly HttpClient _client = new();

        public async Task<string> ExtractFieldsAsync(string text)
        {
            var prompt = $"""
            Extract the following fields from this medical report:
            - Date (YYYY-MM-DD)
            - Hospital Name
            - Diagnosis
            Only return JSON.
            Report:
            {text}
            """;
            var body = new
            {
                model = "gpt-4",
                messages = new[] { new { role = "user", content = prompt } }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            return result.choices[0].message.content.ToString();
        }

        public async Task<string> AskAssistantAsync(string ocrText, string question)
        {
            var prompt = $"Report: {ocrText}\n\nQuestion: {question}";

            var body = new
            {
                model = "gpt-4",
                messages = new[] { new { role = "user", content = prompt } }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            return result.choices[0].message.content.ToString();
        }
    }
}