import os
import re

replacements = {
    # Program.cs
    r"// Instalacion de la aplicacion en entorno de produccion": r"// Instala la aplicación en entorno de producción.",
    r"// Iniciar el contexto de aplicación del System Tray \(icono junto al reloj\)": r"// Inicia el contexto de aplicación de la bandeja del sistema.",

    # TrayApplicationContext.cs
    r"// Iniciar el watcher asíncrono sin bloquear el hilo principal": r"// Inicia el observador asíncrono sin bloquear el hilo principal.",
    r"// Cortar por la primera palabra": r"// Corta por la primera palabra.",
    r"// Limitar a máximo 12 caracteres": r"// Limita a un máximo de 12 caracteres.",
    r"// Agregar \"\.\.\.\" si el nombre original era más largo \(tenía más palabras o excedía 12 caracteres\)\r?\n\s*// o simplemente agregarlo como solicitaste para indicar acortamiento visual.": r"// Agrega puntos suspensivos para indicar acortamiento visual.",
    r"// Ensure text is not longer than 63 chars \(Windows limit\)": r"// Asegura que el texto no exceda los 63 caracteres.",
    r"// -- DRIVE ENCONTRADO --": r"// Unidad Drive encontrada.",
    r"// Iniciar el Monitor": r"// Inicia el monitor.",
    r"// Actualizar UI": r"// Actualiza la interfaz de usuario.",
    r"// Aviso de reconexión si hubo error previo": r"// Aviso de reconexión ante error previo.",
    r"// Ensures it pops up from background": r"// Asegura la aparición desde el fondo.",
    r"// -- DRIVE NO ENCONTRADO --": r"// Unidad Drive no encontrada.",
    r"// Mostrar error una sola vez después de 45 segundos": r"// Muestra el error una sola vez después de 45 segundos.",
    r"// -- ESTAMOS CONECTADOS --": r"// Estado de conexión activa.",
    r"// Monitorear pasivamente si la carpeta desaparece \(Drive se cerró / perdió internet\)": r"// Monitorea pasivamente la desaparición de la carpeta.",
    r"// Revisamos cada 15 seg": r"// Se revisa cada 15 segundos.",
    r"// ¡SE PERDIÓ LA CONEXIÓN!": r"// Pérdida de conexión.",
    r"// Reiniciar contador para volver a avisar si no vuelve pronto": r"// Reinicia el contador para futuros avisos.",

    # RegistrationForm.cs
    r"// -- BANNER IMAGE --": r"// Imagen de encabezado.",
    r"// Incrementado para mayor visibilidad": r"// Incremento de tamaño para mayor visibilidad.",
    r"// -- TITLES --": r"// Títulos.",
    r"// -- INPUT --": r"// Entrada.",
    r"// -- BUTTON --": r"// Botón.",
    r"// -- FOOTER \(3 centered rows\) --": r"// Pie de página.",
    r"// -- VERSION \(Esquina inferior derecha\) --": r"// Versión.",
    r"// Gris": r"// Color gris.",

    # DiscordService.cs
    r"// 1\. Obtener la URL más reciente de la DB cada vez que intentamos \(o reintentamos\)": r"// Obtiene la URL más reciente de la base de datos en cada intento.",
    r"// 2\. Si no hay URL configurada, pausamos y volvemos a leer la DB después": r"// Se pausa y se vuelve a leer la base de datos si no hay URL configurada.",
    r"// 3\. Intentar enviar": r"// Intenta enviar.",
    r"// Exito, salimos del ciclo de reintentos y pasamos al siguiente mensaje": r"// Se sale del ciclo de reintentos y se pasa al siguiente mensaje en caso de éxito.",
    r"// Fallo de red, esperamos 15s y volvemos a empezar el ciclo \(lo cual volverá a leer la DB\)": r"// Se espera 15 segundos y se reinicia el ciclo ante un fallo de red.",
    r"// Otros errores graves \(ej\. URL rota no detectada\), descartamos el mensaje para no trabar la cola": r"// Se descarta el mensaje para no bloquear la cola ante errores graves.",

    # OtaUpdateService.cs
    r"// current: \"v2\.0\", latest: \"v2\.1\.0\"": r"// Ejemplo de versiones actual y reciente.",
    r"// Si no es parseable \(ej\. faltan minor/build\), comparamos los strings asumiendo que el usuario incrementa correctamente": r"// Se comparan las cadenas asumiendo un incremento correcto si el formato no es analizable.",
    r"// Descargar el archivo directamente a disco \(Stream\) en lugar de RAM, ideal para archivos pesados \(155MB\)": r"// Descarga el archivo directamente a disco mediante un flujo.",
    r"// Iniciar el instalador desde la carpeta temporal": r"// Inicia el instalador desde la carpeta temporal.",
    r"// Cerrar la instancia actual para que el nuevo \.exe la sobrescriba": r"// Cierra la instancia actual para permitir la sobrescritura.",
    r"// Darle 1 minuto a Windows para establecer la conexión a internet antes de buscar": r"// Se espera un minuto para el establecimiento de la conexión a internet antes de la búsqueda.",
    r"// Revisa cada 4 horas": r"// Se realiza la revisión cada 4 horas.",

    # UserService.cs
    r"// Sanitizacion de caracteres especiales": r"// Sanitización de caracteres especiales.",
    r"// Extraccion de componentes de encriptacion": r"// Extracción de componentes de encriptación.",
    r"// Division de llave para descifrado AES": r"// División de clave para descifrado AES.",

    # InstallerService.cs
    r"// 1\. Matar el proceso compilado de la versión anterior en Python": r"// Finaliza el proceso compilado de la versión anterior en Python.",
    r"// 2\. Limpiar la carpeta de inicio \(Startup\) de accesos directos o scripts \.bat viejos": r"// Limpia la carpeta de inicio de accesos directos o scripts antiguos.",
    r"// 3\. Limpiar el registro de Windows \(por si la versión vieja se ancló ahí\)": r"// Limpia el registro de Windows de anclajes anteriores.",

    # MonitorService.cs
    r"// Frecuencia de revision": r"// Frecuencia de revisión.",
    r"// Eliminacion de archivo residual": r"// Eliminación de archivo residual.",
    r"// Remocion manual de registro": r"// Remoción manual de registro.",
    r"// Ignoramos C: por rendimiento, asumiendo que Drive crea un disco virtual \(G:, H:, etc\)": r"// Se ignora la unidad C por rendimiento, asumiendo la creación de un disco virtual por Drive.",
    r"// Resolucion de ruta del archivo base": r"// Resolución de ruta del archivo base.",
    r"// Validacion de existencia": r"// Validación de existencia.",
    r"// Verificacion de bloqueo en sistema operativo": r"// Verificación de bloqueo en sistema operativo.",
    r"// Bloqueo no detectado": r"// Bloqueo no detectado.",
    r"// Bloqueo detectado": r"// Bloqueo detectado.",
    r"// Restriccion de permisos": r"// Restricción de permisos.",
    r"// Excepcion no controlada": r"// Excepción no controlada.",
    r"// Verificacion de autenticidad del evento": r"// Verificación de autenticidad del evento.",
    r"// Mitigacion de eventos duplicados": r"// Mitigación de eventos duplicados."
}

files_to_process = [
    "src/UAMotorsCADAlert/Config.cs",
    "src/UAMotorsCADAlert/Program.cs",
    "src/UAMotorsCADAlert/TrayApplicationContext.cs",
    "src/UAMotorsCADAlert/Forms/RegistrationForm.cs",
    "src/UAMotorsCADAlert/Services/DiscordService.cs",
    "src/UAMotorsCADAlert/Services/InstallerService.cs",
    "src/UAMotorsCADAlert/Services/MonitorService.cs",
    "src/UAMotorsCADAlert/Services/OtaUpdateService.cs",
    "src/UAMotorsCADAlert/Services/UserService.cs"
]

for file_path in files_to_process:
    if os.path.exists(file_path):
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        new_content = content
        for pattern, replacement in replacements.items():
            new_content = re.sub(pattern, replacement, new_content)
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)

print("Done")
