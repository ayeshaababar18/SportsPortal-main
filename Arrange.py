import os
    2 
    3 root_dir = "."
    4 output_file = "Codes.txt"
    5 extensions = {".cs", ".cshtml", ".css", ".js", ".json", ".csproj", ".sln"}
    6 ignore_dirs = {".vs", "bin", "obj", ".gemini", ".git", "lib"}
    7 
    8 with open(output_file, "w", encoding="utf-8") as outfile:
    9     for dirpath, dirnames, filenames in os.walk(root_dir):
   10         # Skip ignored directories
   11         dirnames[:] = [d for d in dirnames if d not in ignore_dirs]
   12 
   13         for filename in filenames:
   14             ext = os.path.splitext(filename)[1].lower()
   15             if ext in extensions:
   16                 file_path = os.path.join(dirpath, filename)
   17 
   18                 try:
   19                     with open(file_path, "r", encoding="utf-8") as infile:
   20                         content = infile.read()
   21 
   22                     outfile.write(f"========== {file_path} ==========\n")
   23                     outfile.write(content)
   24                     outfile.write("\n\n")
   25                 except Exception as e:
   26                     outfile.write(f"========== {file_path} ==========\n")
   27                     outfile.write(f"[Error reading file: {e}]\n\n")

