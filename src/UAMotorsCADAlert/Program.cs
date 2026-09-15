using UAMotorsCADAlert.Forms;
using UAMotorsCADAlert.Services;

namespace UAMotorsCADAlert;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        InstallerService.CheckSingleInstance();

        // Instala la aplicación en entorno de producción.
        InstallerService.AutoInstalar();

        var profile = UserService.LoadLocalProfile();
        
        // Inicia el contexto de aplicación de la bandeja del sistema.
        var trayContext = new TrayApplicationContext(profile);
        Application.Run(trayContext);
    }
}
