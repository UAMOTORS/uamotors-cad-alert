import os

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

with open("all_code.txt", "w", encoding="utf-8") as out:
    for file_path in files_to_process:
        if os.path.exists(file_path):
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            out.write(f"// --- {os.path.basename(file_path)} ---\n")
            out.write(content)
            out.write("\n\n")
