using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace UAMotorsCADAlert.Services;

public static class OtaUpdateService
{
    private const string RepoUrl = "https://api.github.com/repos/lexrammart/uamotors-cad-alert/releases/latest";
    private static readonly HttpClient _httpClient;

    static OtaUpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "UAMotorsCADAlert-Updater");
        _httpClient.Timeout = TimeSpan.FromMinutes(30); // Prevenir el límite de 100s en descargas pesadas
    }

    public static string GetCurrentVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("UAMotorsCADAlert.version.txt");
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd().Trim();
            }
        }
        catch { }
        return "v2.0"; // fallback
    }

    public static async Task CheckForUpdatesAsync(Action<string>? onUpdateFound = null)
    {
        // En entorno de desarrollo (Rider) no intentamos actualizar para evitar crasheos
        if (Config.Debug || Debugger.IsAttached || AppDomain.CurrentDomain.BaseDirectory.Contains("bin\\Debug", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var response = await _httpClient.GetAsync(RepoUrl);
            if (!response.IsSuccessStatusCode) return;

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("tag_name", out var tagElement))
            {
                string latestVersion = tagElement.GetString() ?? "";
                string currentVersion = GetCurrentVersion();

                if (IsNewerVersion(currentVersion, latestVersion))
                {
                    // Buscar el asset .exe
                    if (root.TryGetProperty("assets", out var assets) && assets.GetArrayLength() > 0)
                    {
                        foreach (var asset in assets.EnumerateArray())
                        {
                            if (asset.TryGetProperty("name", out var nameElement) && 
                                nameElement.GetString()?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                if (asset.TryGetProperty("browser_download_url", out var downloadUrlElement))
                                {
                                    string downloadUrl = downloadUrlElement.GetString() ?? "";
                                    onUpdateFound?.Invoke($"Descargando actualización a {latestVersion}...");
                                    await DownloadAndApplyUpdate(downloadUrl);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch { }
    }

    private static bool IsNewerVersion(string current, string latest)
    {
        // Ejemplo de versiones actual y reciente.
        current = current.ToLower().Replace("v", "").Trim();
        latest = latest.ToLower().Replace("v", "").Trim();

        if (Version.TryParse(current, out var currentVer) && Version.TryParse(latest, out var latestVer))
        {
            return latestVer > currentVer;
        }
        
        // Se comparan las cadenas asumiendo un incremento correcto si el formato no es analizable.
        return string.Compare(latest, current, StringComparison.Ordinal) > 0;
    }

    private static async Task DownloadAndApplyUpdate(string downloadUrl)
    {
        try
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "UAMotorsUpdate");
            Directory.CreateDirectory(tempDir);
            
            string actualExeName = Path.GetFileName(Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location);
            string tempExePath = Path.Combine(tempDir, actualExeName);

            // Descarga el archivo directamente a disco mediante un flujo.
            using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                using (var fs = new FileStream(tempExePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

            // Inicia el instalador desde la carpeta temporal.
            Process.Start(new ProcessStartInfo
            {
                FileName = tempExePath,
                WorkingDirectory = tempDir,
                UseShellExecute = true
            });

            // Cierra la instancia actual para permitir la sobrescritura.
            Environment.Exit(0);
        }
        catch { }
    }

    public static void StartBackgroundUpdateChecker(Action<string>? onUpdateFound)
    {
        Task.Run(async () =>
        {
            // Se espera un minuto para el establecimiento de la conexión a internet antes de la búsqueda.
            await Task.Delay(TimeSpan.FromMinutes(1));
            
            while (true)
            {
                await CheckForUpdatesAsync(onUpdateFound);
                await Task.Delay(TimeSpan.FromHours(4)); // Se realiza la revisión cada 4 horas.
            }
        });
    }
}
