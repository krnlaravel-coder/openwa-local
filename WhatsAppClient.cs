using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WhtsApi
{
    public class WhatsAppClient
    {
        private static WhatsAppClient _instance;
        public static WhatsAppClient Instance => _instance ?? (_instance = new WhatsAppClient());

        private readonly HttpClient _httpClient;
        public string OpenWaBaseUrl { get; set; } = "http://localhost:8000";

        private WhatsAppClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetQrCodeAsync()
        {
            try
            {
                // In OpenWA, there are specific endpoints for fetching the QR.
                // Adapting based on standard OpenWA REST setups.
                var response = await _httpClient.GetAsync($"{OpenWaBaseUrl}/session/qr");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                // Assuming the API returns a base64 string or a JSON with it
                using (JsonDocument doc = JsonDocument.Parse(content))
                {
                    if (doc.RootElement.TryGetProperty("qr", out JsonElement qrElement))
                    {
                        return qrElement.GetString();
                    }
                }
                return string.Empty;
            }
            catch (HttpRequestException)
            {
                throw new Exception("Server is not started. Please check OpenWA.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching QR: {ex.Message}");
            }
        }

        public async Task<bool> CheckSessionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{OpenWaBaseUrl}/session/status");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content.Contains("CONNECTED");
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async void SendMessage(string number, string message)
        {
            try
            {
                var payload = new
                {
                    chatId = $"{number}@c.us",
                    contentType = "string",
                    content = message
                };

                var json = JsonSerializer.Serialize(payload);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                await _httpClient.PostAsync($"{OpenWaBaseUrl}/client/sendMessage", stringContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending message: " + ex.Message);
            }
        }

        public async void SendDocument(string number, string documentUrl, string filename)
        {
            try
            {
                var payload = new
                {
                    chatId = $"{number}@c.us",
                    url = documentUrl,
                    title = filename,
                    filename = filename
                };

                var json = JsonSerializer.Serialize(payload);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                await _httpClient.PostAsync($"{OpenWaBaseUrl}/client/sendFileFromUrl", stringContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending document: " + ex.Message);
            }
        }
    }
}
