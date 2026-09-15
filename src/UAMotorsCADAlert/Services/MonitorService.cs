using System.Text.RegularExpressions;
using System.Diagnostics;

namespace UAMotorsCADAlert.Services;

public class MonitorService
{
    private readonly FileSystemWatcher _watcher;
    private readonly HashSet<string> _activeLockPaths = new();
    private readonly string _userDisplay;
    private bool _isRunning = true;

    public MonitorService(string rutaActiva)
    {
        var profile = UserService.LoadLocalProfile();
        _userDisplay = profile != null ? profile.Name : "Usuario Desconocido";

        _watcher = new FileSystemWatcher(rutaActiva)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Attributes
        };

        _watcher.Created += OnCreated;
        _watcher.Changed += OnCreated;
        _watcher.Renamed += OnRenamed;
        _watcher.Deleted += OnDeleted;

        Task.Factory.StartNew(GhostLockWatcher, TaskCreationOptions.LongRunning);
    }

    public void Stop()
    {
        _isRunning = false;
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        lock (_activeLockPaths)
        {
            _activeLockPaths.Clear();
        }
    }

    private async Task GhostLockWatcher()
    {
        while (_isRunning)
        {
            await Task.Delay(5000); // Frecuencia de revisión.
            
            bool isSldworksClosed = !SldworksEstaAbierto();
            List<string> pathsToCheck;
            
            lock (_activeLockPaths)
            {
                pathsToCheck = _activeLockPaths.ToList();
            }

            foreach (var path in pathsToCheck)
            {
                bool fileMissing = !File.Exists(path);
                bool esBloqueoFalso = !fileMissing && !EsBloqueoRealSw(path);
                
                if (isSldworksClosed || fileMissing || esBloqueoFalso)
                {
                    // Eliminación de archivo residual.
                    if (!fileMissing && (isSldworksClosed || esBloqueoFalso))
                    {
                        try { File.Delete(path); } catch { }
                    }
                    else if (fileMissing)
                    {
                        // Remoción manual de registro.
                        bool removed;
                        lock (_activeLockPaths)
                        {
                            removed = _activeLockPaths.Remove(path);
                        }
                        
                        if (removed)
                        {
                            string filename = Path.GetFileName(path);
                            string realName = filename.Length >= 2 ? filename.Substring(2) : filename;
                            DiscordService.SendMessage($"🟢 **[LIBRE]:** Ensamble disponible (`{realName}`) - Liberado por `{_userDisplay}`");
                        }
                    }
                }
            }
        }
    }

    public static bool SldworksEstaAbierto()
    {
        try
        {
            return Process.GetProcessesByName("SLDWORKS").Length > 0;
        }
        catch
        {
            return true;
        }
    }

    public static string? BuscarCarpetaUAMOTORS()
    {
        string target = Config.TargetFolder;
        
        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] driveNames = { "Google Drive", "Drive", "GoogleDrive" };

        foreach (var name in driveNames)
        {
            string path = Path.Combine(userHome, name, target);
            if (Directory.Exists(path) && VerificarEnsamble(path))
                return path;
        }

        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            // Se ignora la unidad C por rendimiento, asumiendo la creación de un disco virtual por Drive.
            if (drive.Name.StartsWith("C", StringComparison.OrdinalIgnoreCase)) continue;

            try
            {
                string? candidate = FindFolderDeep(drive.RootDirectory, target, 0, 2);
                if (candidate != null) return candidate;
            }
            catch (Exception) { }
        }

        return null;
    }

    private static string? FindFolderDeep(DirectoryInfo dir, string target, int currentDepth, int maxDepth)
    {
        if (currentDepth > maxDepth) return null;

        try
        {
            foreach (var d in dir.GetDirectories())
            {
                if (d.Name.Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    if (VerificarEnsamble(d.FullName)) return d.FullName;
                }
            }

            foreach (var d in dir.GetDirectories())
            {
                string? found = FindFolderDeep(d, target, currentDepth + 1, maxDepth);
                if (found != null) return found;
            }
        }
        catch (Exception) { }
        return null;
    }

    private static bool VerificarEnsamble(string ruta)
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(ruta, "*", SearchOption.AllDirectories))
            {
                if (Regex.IsMatch(Path.GetFileName(file), Config.AssemblyPattern, RegexOptions.IgnoreCase))
                    return true;
            }
        }
        catch (Exception) { }
        return false;
    }

    private static bool EsBloqueoRealSw(string filepath)
    {
        if (Config.Debug) 
            return true; 
            
        // Resolución de ruta del archivo base.
        string directory = Path.GetDirectoryName(filepath) ?? "";
        string lockFileName = Path.GetFileName(filepath);
        if (!lockFileName.StartsWith("~$")) return false;
        
        string baseFileName = lockFileName.Substring(2);
        string baseFilePath = Path.Combine(directory, baseFileName);
        
        // Validación de existencia.
        if (!File.Exists(baseFilePath)) return false;
        
        try
        {
            // Verificación de bloqueo en sistema operativo.
            using var fs = new FileStream(baseFilePath, FileMode.Open, FileAccess.Write, FileShare.None);
            
            // Bloqueo no detectado.
            return false;
        }
        catch (IOException)
        {
            // Bloqueo detectado.
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            // Restricción de permisos.
            return true;
        }
        catch (Exception)
        {
            // Excepción no controlada.
            return true;
        }
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        OnCreated(sender, e);
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        string filename = Path.GetFileName(e.FullPath);
        if (Regex.IsMatch(filename, Config.LockPattern, RegexOptions.IgnoreCase))
        {
            // Verificación de autenticidad del evento.
            if (!EsBloqueoRealSw(e.FullPath))
                return;

            lock (_activeLockPaths)
            {
                if (!_activeLockPaths.Add(e.FullPath))
                    return; // Mitigación de eventos duplicados.
            }
            string realName = filename.Substring(2);
            DiscordService.SendMessage($"🔴 **[OCUPADO]:** Ensamble en uso (`{realName}`) por `{_userDisplay}`");
        }
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        string filename = Path.GetFileName(e.FullPath);
        if (Regex.IsMatch(filename, Config.LockPattern, RegexOptions.IgnoreCase))
        {
            bool removed;
            lock (_activeLockPaths)
            {
                removed = _activeLockPaths.Remove(e.FullPath);
            }

            if (removed)
            {
                string realName = filename.Substring(2);
                DiscordService.SendMessage($"🟢 **[LIBRE]:** Ensamble disponible (`{realName}`) - Liberado por `{_userDisplay}`");
            }
        }
    }
}
