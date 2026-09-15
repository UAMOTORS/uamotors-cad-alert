using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;

namespace UAMotorsCADAlert.Services;

public static class DiscordService
{
    private static readonly BlockingCollection<string> _messageQueue = new();
    private static readonly HttpClient _httpClient = new();

    static DiscordService()
    {
        Task.Factory.StartNew(WorkerLoop, TaskCreationOptions.LongRunning);
    }

    public static void SendMessage(string message)
    {
        _messageQueue.Add(message);
    }

    private static async Task WorkerLoop()
    {
        foreach (var message in _messageQueue.GetConsumingEnumerable())
        {
            var payload = new { content = message };
            string json = JsonSerializer.Serialize(payload);

            while (true)
            {
                string url = "";
                
                // Obtiene la URL más reciente de la base de datos en cada intento.
                if (!string.IsNullOrEmpty(Config.ResolvedDrivePath))
                {
                    var db = UserService.LoadDriveDatabase(Config.ResolvedDrivePath);
                    if (db != null && db.Config.TryGetValue("webhook_url", out var wh) && !string.IsNullOrEmpty(wh))
                    {
                        if (Uri.TryCreate(wh, UriKind.Absolute, out Uri? uriResult) && 
                            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                        {
                            url = wh;
                        }
                    }
                }

                // Se pausa y se vuelve a leer la base de datos si no hay URL configurada.
                if (string.IsNullOrEmpty(url))
                {
                    await Task.Delay(10000);
                    continue; 
                }

                // Intenta enviar.
                try
                {
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(url, content);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(5000);
                        continue;
                    }
                    break; // Se sale del ciclo de reintentos y se pasa al siguiente mensaje en caso de éxito.
                }
                catch (HttpRequestException)
                {
                    // Se espera 15 segundos y se reinicia el ciclo ante un fallo de red.
                    await Task.Delay(15000); 
                }
                catch
                {
                    // Se descarta el mensaje para no bloquear la cola ante errores graves.
                    break; 
                }
            }
        }
    }
}
