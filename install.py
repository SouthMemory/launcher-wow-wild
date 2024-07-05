import os
import zipfile
import xml.etree.ElementTree as ET

# 设置基础URL和updates目录路径
updates_dir = "updates"
addons_dir = os.path.join(updates_dir, "Interface/Addons")
output_file = os.path.join(updates_dir, "filelist.conf")
executable_path = "AmarothLauncher/bin/Release/DiDiLauncher.exe"
zip_file_path = os.path.join(updates_dir, "Launcher.zip")
project_file_path = "AmarothLauncher/AmarothLauncher.csproj"
version_file_path = os.path.join(updates_dir, "launcherversion.conf")
excluded_dirs = [addons_dir, ]


files_excludes = [
    "filelist.conf",
    "hello.jpg",
    "changelog.xml",
    "launcherversion.conf",
    "Launcher.zip",
    "hello.png"
]

# 创建函数来获取updates目录下的所有文件及其相对路径
def get_files_in_directory(directory, excluded_dirs):
    files_list = []
    for root, _, files in os.walk(directory):
        print(root)
        # 检查当前目录是否在排除列表中
        if "Addons" in root:
            continue
        for file in files:
            if file in files_excludes:
                continue
            print(file)
            file_path = os.path.join(root, file)
            relative_path = os.path.relpath(file_path, directory)
            files_list.append(relative_path.replace("\\", "/"))
    return files_list

# 压缩可执行文件
def create_zip_file(zip_path, file_to_zip):
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        zipf.write(file_to_zip, os.path.basename(file_to_zip))
    print(f"Created zip file: {zip_path}")

# 压缩指定目录并返回ZIP文件路径
def create_zip_from_directory(directory, zip_name):
    zip_path = os.path.join(updates_dir, f"{zip_name}.zip")
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for root, _, files in os.walk(directory):
            for file in files:
                file_path = os.path.join(root, file)
                arcname = os.path.relpath(file_path, os.path.dirname(directory))
                zipf.write(file_path, arcname)
    print(f"Created zip file from directory: {zip_path}")
    return zip_path

# 读取项目文件中的版本号
def read_version_from_csproj(csproj_path):
    tree = ET.parse(csproj_path)
    root = tree.getroot()
    namespace = {'msbuild': 'http://schemas.microsoft.com/developer/msbuild/2003'}
    version_element = root.find('msbuild:PropertyGroup/msbuild:ApplicationVersion', namespace)
    if version_element is not None:
        return version_element.text
    else:
        raise ValueError("ApplicationVersion not found in the project file.")

# 将版本号写入到launcherversion.conf
def write_version_to_conf(version, conf_path):
    with open(conf_path, "w") as f:
        f.write(version)
    print(f"Version {version} has been written to {conf_path}")

# 记录已打包的目录

# 压缩addons目录下的每个子目录为单独的zip文件，并记录已打包的目录
if os.path.exists(addons_dir):
    for addon in os.listdir(addons_dir):
        addon_path = os.path.join(addons_dir, addon)
        if os.path.isdir(addon_path):
            zip_name = f"Interface/Addons/{addon}"
            zip_path = create_zip_from_directory(addon_path, zip_name)
            web_rel_path = os.path.relpath(zip_path, updates_dir).replace("\\", "/")
            local_rel_path = "/Interface/Addons/"
            with open(output_file, "w") as f:
                f.write(f"{web_rel_path};0;{local_rel_path};\n")

# 获取updates目录下的所有文件，排除已打包的目录
files = get_files_in_directory(updates_dir, excluded_dirs)
print(files)

# 将文件添加到filelist.conf
with open(output_file, "a") as f:
    for file in files:
        web_rel_path = file
        optional = 0  # 默认文件不是可选的，如果需要可选文件可以根据情况修改
        local_rel_path = "/" + os.path.dirname(file) + "/"
        if local_rel_path == "//":
            local_rel_path = "/"
        f.write(f"{web_rel_path};{optional};{local_rel_path};\n")
    f.write("realmlist.wtf;0;/Data/zhCN/;\n")

print(f"File list has been generated and written to {output_file}")

# 创建 Launcher.zip
create_zip_file(zip_file_path, executable_path)

# 读取版本号并写入到launcherversion.conf
version = read_version_from_csproj(project_file_path)
write_version_to_conf(version, version_file_path)
