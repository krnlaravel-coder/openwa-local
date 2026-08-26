using System;
using System.Net;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WhtsApi
{
    public class LocalApiServer
    {
        private HttpListener _listener;
        private bool _isRunning = false;

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:5000/api/");
            
            try
            {
                _listener.Start();
                _isRunning = true;
                Console.WriteLine("API Server started on http://localhost:5000/api/");

                while (_isRunning)
                {
                    var context = _listener.GetContext();
                    Task.Run(() => HandleRequest(context));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting server: " + ex.Message);
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
        }

        private void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.HttpMethod != "POST")
            {
                SendResponse(response, 405, "{\"error\": \"Method not allowed\"}");
                return;
            }

            try
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();
                    var data = JsonSerializer.Deserialize<JsonElement>(body);

                    if (!data.TryGetProperty("number", out var numberProp))
                    {
                        SendResponse(response, 400, "{\"error\": \"Missing number\"}");
                        return;
                    }

                    string number = numberProp.GetString();

                    if (request.Url.AbsolutePath.EndsWith("/send/text"))
                    {
                        string message = data.GetProperty("message").GetString();
                        WhatsAppClient.Instance.SendMessage(number, message);
                        SendResponse(response, 200, "{\"success\": true, \"type\": \"text\"}");
                    }
                    else if (request.Url.AbsolutePath.EndsWith("/send/document"))
                    {
                        string filename = data.GetProperty("filename").GetString();
                        string url = data.GetProperty("documentUrl").GetString();
                        WhatsAppClient.Instance.SendDocument(number, url, filename);
                        SendResponse(response, 200, "{\"success\": true, \"type\": \"document\"}");
                    }
                    else if (request.Url.AbsolutePath.EndsWith("/send/link"))
                    {
                        string url = data.GetProperty("url").GetString();
                        string message = data.GetProperty("message").GetString();
                        WhatsAppClient.Instance.SendMessage(number, $"{message}\n{url}");
                        SendResponse(response, 200, "{\"success\": true, \"type\": \"link\"}");
                    }
                    else
                    {
                        SendResponse(response, 404, "{\"error\": \"Endpoint not found\"}");
                    }
                }
            }
            catch (Exception ex)
            {
                SendResponse(response, 500, $"{{\"error\": \"{ex.Message}\"}}");
            }
        }

        private void SendResponse(HttpListenerResponse response, int statusCode, string content)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}
