using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace UAMotorsCADAlert.Services;

public class UserProfile
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class WhitelistEntry
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
}

public class DriveDatabase
{
    [JsonPropertyName("config")]
    public Dictionary<string, string> Config { get; set; } = new();

    [JsonPropertyName("users")]
    public Dictionary<string, WhitelistEntry> Users { get; set; } = new();
}

public static class UserService
{
    public static string GetProfileFilePath() => Path.Combine(Config.GetAppDataDir(), "user_profile.uamotors");

    public static UserProfile? LoadLocalProfile()
    {
        string path = GetProfileFilePath();
        if (!File.Exists(path)) return null;

        try
        {
            byte[] encryptedData = File.ReadAllBytes(path);
            byte[] decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            string json = Encoding.UTF8.GetString(decryptedData);
            return JsonSerializer.Deserialize<UserProfile>(json);
        }
        catch
        {
            return null;
        }
    }

    public static UserProfile SaveLocalProfile(string email, string name)
    {
        // Sanitización de caracteres especiales.
        string cleanName = name.Trim().ToUpper().Replace("*", "").Replace("_", "").Replace("`", "").Replace("~", "");
        var profile = new UserProfile { Email = email.Trim().ToLower(), Name = cleanName };
        
        string jsonStr = JsonSerializer.Serialize(profile);
        byte[] dataToEncrypt = Encoding.UTF8.GetBytes(jsonStr);
        byte[] encryptedData = ProtectedData.Protect(dataToEncrypt, null, DataProtectionScope.CurrentUser);
        
        File.WriteAllBytes(GetProfileFilePath(), encryptedData);
        return profile;
    }

    private static byte[] GetEncryptionKey()
    {
        string secret = "UAMOTORS_2026_CAD_ELECTRONICS";
        string saltStr = "UAMOTORS_CAD_ALERT_SALT";
        byte[] salt = Encoding.UTF8.GetBytes(saltStr);
        
        using var pbkdf2 = new Rfc2898DeriveBytes(secret, salt, 100000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    public static DriveDatabase? LoadDriveDatabase(string rutaUAMOTORS)
    {
        string dbPath = Path.Combine(rutaUAMOTORS, Config.RelDriveDbPath);
        if (!File.Exists(dbPath)) return null;

        try
        {
            byte[] encryptedBytes = File.ReadAllBytes(dbPath);
            string fernetToken = Encoding.UTF8.GetString(encryptedBytes);
            byte[] tokenBytes = Base64UrlDecode(fernetToken);
            
            // Extracción de componentes de encriptación.
            byte[] iv = new byte[16];
            Array.Copy(tokenBytes, 9, iv, 0, 16);
            
            int ciphertextLength = tokenBytes.Length - 9 - 16 - 32;
            byte[] ciphertext = new byte[ciphertextLength];
            Array.Copy(tokenBytes, 25, ciphertext, 0, ciphertextLength);

            byte[] key = GetEncryptionKey(); 
            // División de clave para descifrado AES.
            byte[] encKey = new byte[16];
            Array.Copy(key, 16, encKey, 0, 16); 

            using var aes = Aes.Create();
            aes.Key = encKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            string json = Encoding.UTF8.GetString(decryptedBytes);
            
            return JsonSerializer.Deserialize<DriveDatabase>(json);
        }
        catch
        {
            return null;
        }
    }

    public static (bool Success, string? Name, string ErrorMsg) VerifyUserEmail(string email, string rutaUAMOTORS)
    {
        string emailClean = email.Trim().ToLower();
        if (string.IsNullOrEmpty(emailClean) || !emailClean.Contains("@"))
            return (false, null, "Por favor ingresa un correo electrónico válido.");

        var db = LoadDriveDatabase(rutaUAMOTORS);
        if (db == null)
            return (false, null, $"No se encontró la Base de Datos en Drive. Asegúrate de tener Drive abierto.");

        if (db.Users.TryGetValue(emailClean, out var userEntry))
        {
            string name = !string.IsNullOrEmpty(userEntry.Nombre) ? userEntry.Nombre : "Usuario Autorizado";
            return (true, name, string.Empty);
        }

        return (false, null, "Correo no encontrado. Contacta al administrador si crees que es un error.");
    }
}
