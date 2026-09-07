<h1>
  <a href="https://uamotors.github.io/">
    <img src="assets/1.completo.svg" width="250" align="absmiddle" alt="UAMOTORS Logo">
  </a>
  &nbsp;UAMOTORS CAD Alert
</h1>

*English documentation below.*

Monitor silencioso en segundo plano para archivos de ensamblaje de SolidWorks.

**[Visita el sitio web del equipo UAMOTORS](https://uamotors.github.io/)**

Esta aplicación monitorea en tiempo real los archivos de ensamblaje de SolidWorks dentro del directorio de Google Drive del equipo y envía actualizaciones de estado (quién está editando qué) a un canal de Discord de forma automática.

## Descarga e Instalación

1. Ve a la sección de **[Releases](../../releases/latest)** en el lado derecho de este repositorio.
2. Descarga la versión más reciente ubicada bajo la sección **Assets** (el archivo se llama `UAMOTORS CAD ALERT.exe`).
3. Ejecuta el archivo descargado.

> [!WARNING]
> **Sobre la pantalla azul de Windows (SmartScreen):**
> Dado que nuestra aplicación es nueva, es muy probable que Windows lance una pantalla azul de advertencia diciendo *"Windows protegió su PC"*.
> Para continuar:
> 1. Haz clic en el texto que dice **"Más información"** (justo debajo del texto principal).
> 2. Aparecerá un nuevo botón abajo a la derecha. Haz clic en **"Ejecutar de todas formas"**.

4. Al abrirse, ingresa tu correo institucional para vincular tu equipo.
5. El sistema se instalará automáticamente en segundo plano y se ejecutará solo cada vez que enciendas tu computadora. No necesitas volver a abrir el archivo manualmente.

## Características ⚙️

* **Motor C# (.NET 8):** Alto rendimiento y muy bajo consumo de memoria RAM.
* **Actualizaciones OTA:** Descarga e instala nuevas versiones automáticamente en segundo plano.
* **Auto-Arranque:** Se ancla de forma segura al Registro de Windows (Run Key) sin necesidad de permisos de administrador.

---

# English Documentation

A silent, background monitor for SolidWorks assemblies.

This application monitors SolidWorks assembly files within a local or cloud-synced directory and sends real-time status updates to a Discord webhook.

## Download & Installation

1. Go to the **[Releases](../../releases/latest)** section on the right side of this repository.
2. Download the latest release under the **Assets** section (the file is named `UAMOTORS CAD ALERT.exe`).
3. Run the downloaded executable.

> [!WARNING]
> **About the Windows SmartScreen (Blue Screen):**
> Since our application is new, Windows will likely display a blue warning screen saying *"Windows protected your PC"*.
> To proceed with the installation:
> 1. Click on the text that says **"More info"** (right below the main text).
> 2. A new button will appear at the bottom right. Click on **"Run anyway"**.

4. Once the app opens, enter your institutional email to link your device.
5. The system will automatically install itself in the background and configure Windows to run it on startup. You do not need to manually open the file again.

## Core Features ⚙️

* **C# (.NET 8) Engine:** High performance and minimal memory footprint.
* **OTA Updates:** Automatically downloads and applies new updates in the background.
* **Auto-Start Integration:** Safely hooks into the Windows Registry Run Key without requiring administrator privileges.
