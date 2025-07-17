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

        public async Task<string> GetNonClaimableItemsAsync(string text)
        {
            var prompt = $$"""
    You are a hospital billing claim assistant.

    From the following hospital bill text, identify items that are NOT claimable under insurance. The commonly non-claimable items include (but are not limited to):
    - Medical Gloves
    - Surgical Gown
    - Disposable Cap & Mask
    - Sterile Drapes

    Extract their:
    - Date (YYYY-MM-DD)
    - Hospital Name
    - Diagnosis
    - Name (as shown in the bill)
    - Quantity and unit rate (if available)
    - Calculate and return the total amount for each non-claimable item
    - Return the sum of all non-claimable item amounts
    - Subtract that from the total bill amount to get the **approved claim amount**

    Return output only in the following JSON format:
    {
        "nonClaimableItems": [
            { "name": "Medical Gloves", "amount": 100 },
            { "name": "Surgical Gown", "amount": 200 }
        ],
        "nonClaimableTotal": 300,
        "approvedAmount": 7890,
        "Date": "2023-10-01",
        "Hospital Name": "Apollo Hospital",
        "Diagnosis": "Bronchitis"
    }

    Hospital Bill Text:
    {{text}}
    """;

            var body = new
            {
                model = "gpt-4",  // Fixed typo from "apt-4" to "gpt-4"
                messages = new[] { new { role = "user", content = prompt } }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);

            if (result?.choices != null && result.choices.Count > 0)
            {
                return result.choices[0].message.content.ToString();
            }
            return string.Empty;
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