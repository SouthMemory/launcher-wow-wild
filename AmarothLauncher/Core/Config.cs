using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace AmarothLauncher.Core
{
    /// <summary>
    /// Singleton responsible for getting config settings or generating default ones.
    /// </summary>
    public class Config
    {
        private static Config instance;

        // Launcher's version.
        public double version = 20240101.0;
        public bool isDefaultConfigUsed { get; private set; }
        XmlDocument xml = new XmlDocument();
        XmlDocument defaultXml = new XmlDocument();
        OutputWriter o = OutputWriter.Instance;

        private Config()
        {
            Initialize();
        }

        public static Config Instance
        {
            get
            {
                if (instance == null)
                    instance = new Config();
                return instance;
            }
        }
        /// <summary>
        /// Generates object XML structure of LauncherConfig.xml.
        /// If reading of XML failes, default settings are used.
        /// </summary>
        private void Initialize()
        {
            isDefaultConfigUsed = false;
            GenerateDefault();
            if (!File.Exists("LauncherConfig.xml"))
                UseDefault();
            else
            {
                StreamReader sr = new StreamReader("LauncherConfig.xml");
                string xmlString = sr.ReadToEnd();
                sr.Close();
                xml.LoadXml(xmlString);
            }
        }

        /// <summary>
        /// Use default config XML as current config XML.
        /// </summary>
        private void UseDefault()
        {
            xml = defaultXml;
            // o.Messagebox(SubElText("Messages", "XmlNotOpened"));

            // Save default config as a new config XML. Use for generating XML to be able to edit it afterwards, do NOT have this uncommented in releases!
            // SaveDefault();

            isDefaultConfigUsed = true;
        }

        /// <summary>
        /// Outputs whole config content. For debugging only.
        /// </summary>
        public void OutputContent()
        {
            o.Output("Outputing all values set in Config for debugging purpouses. * marks attributes (followed by their names), \"\" mark values.");
            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                o.Output(node.Name, true);
                foreach (XmlNode att in node.ChildNodes)
                    o.Output("* " + att.Name + " - \"" + att.InnerText + "\"");
            }
        }

        /// <summary>
        /// Returns an inner text of given subelement of given element. If anything isn't found, error message is shown and an empty string returned.
        /// </summary>
        public string SubElText(string elementName, string subElementName)
        {
            if (xml.GetElementsByTagName(elementName).Count > 0)
            {
                foreach (XmlNode node in xml.GetElementsByTagName(elementName)[0].ChildNodes)
                    if (node.Name == subElementName)
                        return node.InnerText;
            }
            else
                o.Output(elementName + " element was not found in config. This may cause critical errors.");

            o.Output(subElementName + " attribute was not found in config. This may cause critical errors.");
            return "";
        }

        /// <summary>
        /// Return an inner text of given element in config XML. If not found, return empty string and output error.
        /// </summary>
        public string InnText(string elementName)
        {
            if (xml.GetElementsByTagName(elementName).Count > 0)
                return xml.GetElementsByTagName(elementName)[0].InnerText;
            else
            {
                o.Output(elementName + " element was not found in config. This may cause critical errors.");
                return "";
            }
        }

        #region Default config generation...
        /// <summary>
        /// Generate a new default config. Will be used only if no config is found.
        /// </summary>
        private void GenerateDefault()
        {
            XmlDeclaration declaration = defaultXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = defaultXml.DocumentElement;
            defaultXml.InsertBefore(declaration, root);
            XmlElement configs = defaultXml.CreateElement("Config");
            XmlComment comment = defaultXml.CreateComment("This is a config file for Launcher. Feel free to edit whatever you want or even translate Launcher to your native language. Pay close attention to comments and documentation.");
            defaultXml.AppendChild(comment);
            defaultXml.AppendChild(configs);

            MainSettingsDefault();
            PathConfigDefault();
            MainWindowDefault();
            ChangelogEditorDefault();
            ChangelogBrowserDefault();
            FTPLoginWindowDefault();
            MessagesDefault();
        }

        /// <summary>
        /// Adds a new subnode under a node, with given name and inner text.
        /// </summary>
        private void AddSubnodeDefault(XmlNode node, string name, string value)
        {
            AddSubnodeDefault(node, name, value, "");
        }

        /// <summary>
        /// Adds a new subnode under a node, with given name and inner text. Also creates a given comment, if it isn't empty.
        /// </summary>
        private void AddSubnodeDefault(XmlNode node, string name, string value, string comment)
        {
            XmlNode newNode = defaultXml.CreateElement(name);
            if (comment != "" && comment != null)
            {
                XmlComment newComment = defaultXml.CreateComment(name + ": " + comment);
                node.AppendChild(newComment);
            }
            newNode.InnerText = value;
            node.AppendChild(newNode);
        }

        /// <summary>
        /// Default configuration of paths to web.
        /// </summary>
        private void MainSettingsDefault()
        {
            XmlComment comment = defaultXml.CreateComment("Main settings of an application.");
            XmlNode node = defaultXml.CreateElement("Main");

            AddSubnodeDefault(node, "DeleteCache", "1", "1 or 0. If 1, Cache folder is always deleted.");
            AddSubnodeDefault(node, "KeepBackups", "1", "1 or 0. If 1, .ext_ files are being kept as backups. Recommended 1.");
            AddSubnodeDefault(node, "KeepBlizzlikeMPQs", "1", "1 or 0. If 1, blizzlike MPQs in Data are ignored by Launcher. Otherwise they are handled in a same manner as custom one. Recommended 1.");
            AddSubnodeDefault(node, "ForcedRealmlist", "1", "1 or 0. If 1, realmlist.wtf will always be updated to match realmlist.wtf in FilesRootPath.");
            AddSubnodeDefault(node, "FileProcessingOutputs", "1", "1 or 0. If 1, messages about downloading and unziping files will be shown.");

            defaultXml.DocumentElement.AppendChild(comment);
            defaultXml.DocumentElement.AppendChild(node);
        }

        /// <summary>
        /// Default configuration of paths to web.
        /// </summary>
        private void PathConfigDefault()
        {
            XmlComment comment = defaultXml.CreateComment("Paths to files and folders Launcher will work with. They are commonly all !CASE SENSITIVE!");
            XmlNode node = defaultXml.CreateElement("Paths");

            AddSubnodeDefault(node, "FilelistPath", "https://www.wild-wow.com/updates/filelist.conf", "Path to text filelist.");
            AddSubnodeDefault(node, "VersionPath", "https://www.wild-wow.com/updates/launcherversion.conf", "Path to text file which contains your Lancher's current version (version is a double value with . as separator!");
            AddSubnodeDefault(node, "LauncherPath", "https://www.wild-wow.com/updates/Launcher.zip", "Path to a zip file with Launcher files - used if Launcher finds itself outdated.");
            AddSubnodeDefault(node, "FilesRootPath", "https://www.wild-wow.com/updates/", "Path to folder with files. Paths in filelist are relative to this path.");
            AddSubnodeDefault(node, "ChangelogPath", "https://www.wild-wow.com/updates/changelog.xml", "!HTTP! path to changelog XML file.");
            AddSubnodeDefault(node, "ChangelogFTPPath", "ftp://ftp.example.com//www/files/", "!Full! !FTP! path to folder in which changelog is. Notice that //www/ part. You may want to use an IP instead of a domain name.");
            AddSubnodeDefault(node, "Webpage", "https://www.wild-wow.com", "URL which is to be opened when user clicks on Project webpage button.");
            AddSubnodeDefault(node, "Registration", "https://www.wild-wow.com", "URL which is to be opened when user clicks on Registration button.");
            AddSubnodeDefault(node, "Instructions", "https://www.wild-wow.com/launchermanual/", "URL which is to be opened when user clicks on Launcher manual button.");
            AddSubnodeDefault(node, "HelloImage", "https://www.wild-wow.com/updates/hello.png", "URL to image which is to be displayed in Main window (latest news image). Clicking on it opens a changelog browser.");

            defaultXml.DocumentElement.AppendChild(comment);
            defaultXml.DocumentElement.AppendChild(node);
        }

        /// <summary>
        /// Default configuration of main window.
        /// </summary>
        private void MainWindowDefault()
        {
            XmlComment comment = defaultXml.CreateComment("Visual settings for Main Window.");
            XmlNode node = defaultXml.CreateElement("MainWindow");

            AddSubnodeDefault(node, "WindowName", "wild-wow Launcher " + version.ToString("F", CultureInfo.InvariantCulture), "Name of main window. Change this to match your project's name.");
            AddSubnodeDefault(node, "OutputBox", "Text output:");
            AddSubnodeDefault(node, "OptionalBox", "可选更新:");
            AddSubnodeDefault(node, "CheckForUpdatesButton", "检查更新");
            AddSubnodeDefault(node, "UpdateButton", "更新");
            AddSubnodeDefault(node, "WebpageButton", "网站");
            AddSubnodeDefault(node, "RegistrationButton", "注册");
            AddSubnodeDefault(node, "LauncherInstructionsButton", "手册");
            AddSubnodeDefault(node, "DeleteBackupsButton", "删除备份文件");
            AddSubnodeDefault(node, "ChangelogEditorButton", "编辑更新日志");
            AddSubnodeDefault(node, "ChangelogBrowserButton", "查看更新日志");
            AddSubnodeDefault(node, "LaunchButton", "启动游戏");
            AddSubnodeDefault(node, "ProgressText", "正在下载: ");
            AddSubnodeDefault(node, "ProgressSeparator", " / ");
            AddSubnodeDefault(node, "DownloadSpeedUnits", "/s, ");
            AddSubnodeDefault(node, "remaining", "剩余");
            AddSubnodeDefault(node, "downloaded", "已下载, ");
            AddSubnodeDefault(node, "ToolTipTotalSize", "Total size: ");
            AddSubnodeDefault(node, "PanelTotalSize", "Total size of outdated:");
            AddSubnodeDefault(node, "LabelTotalSizeOpt", "Chosen optionals: ");
            AddSubnodeDefault(node, "LabelTotalSizeNonOpt", "Non-optionals: ");
            AddSubnodeDefault(node, "second", " s ");
            AddSubnodeDefault(node, "minute", " m ");
            AddSubnodeDefault(node, "hour", " h ");
            AddSubnodeDefault(node, "Complete", "下载完成!");
            AddSubnodeDefault(node, "Errors", "出错了!");

            defaultXml.DocumentElement.AppendChild(comment);
            defaultXml.DocumentElement.AppendChild(node);
        }

        /// <summary>
        /// Default configuration of changelog editor.
        /// </summary>
        private void ChangelogEditorDefault()
        {
            XmlComment comment = defaultXml.CreateComment("Visual settings for Changelog Editor. A lot of those are used by Changelog Browser as well.");
            XmlNode node = defaultXml.CreateElement("ChangelogEditor");

            AddSubnodeDefault(node, "WindowName", "Changelog Editor");
            AddSubnodeDefault(node, "ChangelogEntries", "Changelog entries:");
            AddSubnodeDefault(node, "DateColumn", "Date");
            AddSubnodeDefault(node, "HeadingColumn", "Heading");
            AddSubnodeDefault(node, "Date", "Date:");
            AddSubnodeDefault(node, "DateFormat", "dd.MM.yyyy hh:mm", "Carefully with this. MM for months, mm for minutes. You can use your own format, but it must be correct. Changelog's data must also be compatible with this, if your changelog isn't empty when this is being changed!");
            AddSubnodeDefault(node, "PictureURL", "Picture URL:");
            AddSubnodeDefault(node, "Heading", "Heading:");
            AddSubnodeDefault(node, "PicturePreview", "Picture preview:");
            AddSubnodeDefault(node, "Description", "Description:");
            AddSubnodeDefault(node, "EditEntryButton", "Edit entry");
            AddSubnodeDefault(node, "DeleteEntryButton", "Delete entry");
            AddSubnodeDefault(node, "CreateEntryButton", "Create entry");
            AddSubnodeDefault(node, "SaveEntryButton", "Save entry");
            AddSubnodeDefault(node, "TestPictureButton", "Test picture");
            AddSubnodeDefault(node, "CancelButton", "Cancel changes");
            AddSubnodeDefault(node, "SaveButton", "Save changelog");

            defaultXml.DocumentElement.AppendChild(comment);
            defaultXml.DocumentElement.AppendChild(node);
        }

        /// <summary>
        /// Default configuration of changelog browser. A lot of settings are being used from changelog editor.
        /// </summary>
        private void ChangelogBrowserDefault()
        {
            XmlComment comment = defaultXml.CreateComment("Visual settings for Changelog Browser.");
            XmlNode node = defaultXml.CreateElement("ChangelogBrowser");

            AddSubnodeDefault(node, "WindowName", "Changelog Browser");
            AddSubnodeDefault(node, "InfoText", "Click on an entry in entries list in order to display it.");
            AddSubnodeDefault(node, "Picture", "Picture:");

            defaultXml.DocumentElement.AppendChild(comment);
            defaultXml.DocumentElement.AppendChild(node);
        }

        /// <summary>
        /// Default configuration of FTP login window.
        /// </summary>
        private void FTPLoginWindowDefault()
        {
            XmlComment comment = defaultXml.CreateComment("Visual settings for authentization dialog window for Changelog Editor.");
            XmlNode node = defaultXml.CreateElement("FTPLoginWindow");

            AddSubnodeDefault(node, "WindowName", "Login to FTP");
            AddSubnodeDefault(node, "Login", "Login:");
            AddSubnodeDefault(node, "Password", "Password:");
            AddSubnodeDefault(node, "OKButton", "OK");
            AddSubnodeDefault(node, "CancelButton", "Cancel");

            defaultXml.DocumentElement.AppendChild(comment);
            defaultXml.DocumentElement.AppendChild(node);
        }

        /// <summary>
        /// 错误（和其他）消息。如果你想直接输出某些内容到消息后面，请在消息后保留空格（比如文件名）。
        /// </summary>
        private void MessagesDefault()
        {
            XmlComment comment = defaultXml.CreateComment("各种可能由启动器输出的消息。");
            XmlNode node = defaultXml.CreateElement("Messages");

            AddSubnodeDefault(node, "HelloMessage", "欢迎来到wow wild:https://www.wild-wow.com", "请将这条信息保留在这里。如果你想添加任何内容，请添加在原始消息之后。");
            AddSubnodeDefault(node, "XmlNotOpened", "启动器使用默认参数。如果\"更新\"按钮不可用，请尝试关闭启动器后重新打开。");
            AddSubnodeDefault(node, "ChangelogNotOpened", "无法打开网页上的更新日志文件。");
            AddSubnodeDefault(node, "ChangelogNotLoaded", "无法加载更新日志数据。请联系支持。");
            AddSubnodeDefault(node, "ChangelogEmpty", "警告：更新日志为空。你当前正在创建一个新的。");
            AddSubnodeDefault(node, "InvalidFtpPassword", "登录密码组合不正确, 或FTP路径到更新日志不正确。");
            AddSubnodeDefault(node, "PictureNotOpened", "无法打开给定URL的图片。URL似乎无效。");
            AddSubnodeDefault(node, "ChangelogNotUploaded", "无法上传更新日志。备份XML文件可以在启动器目录中找到。");
            AddSubnodeDefault(node, "ChangelogUploadOK", "更新日志已成功更新。");
            AddSubnodeDefault(node, "UnZipingFileError", "解压文件失败：");
            AddSubnodeDefault(node, "DownloadingFrom", "正在从以下地址下载文件：");
            AddSubnodeDefault(node, "DownloadingTo", "下载到：");
            AddSubnodeDefault(node, "UnzipingFile", "正在解压文件：");
            AddSubnodeDefault(node, "FileDeletingError", "文件删除失败：");
            AddSubnodeDefault(node, "WowExeMissing", "未找到Wow.exe");
            AddSubnodeDefault(node, "DataDirMissing", "未找到数据目录！");
            AddSubnodeDefault(node, "BlizzlikeMpqMissing", "未能找到关键文件：");
            AddSubnodeDefault(node, "LauncherNotInWowDir", "你的魔兽世界客户端要么已损坏，要么启动器不在魔兽世界根目录。");
            AddSubnodeDefault(node, "FilelistOpeningFailed", "启动器无法打开网上的文件列表。检查你的网络连接，或联系支持团队。错误信息：");
            AddSubnodeDefault(node, "FilelistReadingFailed", "网络上的文件列表无效。联系你的支持团队。");
            AddSubnodeDefault(node, "FileOnsWebMissing", "无法找到文件的大小。文件可能在网上服务器上丢失。");
            AddSubnodeDefault(node, "WebRealmlistMissing", "无法在网上找到realmlist.wtf文件。无法验证realmlist。");
            AddSubnodeDefault(node, "RealmlistMissing", "本地的realmlist.wtf似乎丢失了。如果是这样,请创建一个新的。");
            AddSubnodeDefault(node, "OptionalsPresetLoadFailed", "你没有保存可选组的选择，或者它们的列表已更改。请在点击更新按钮前注意可选文件复选框列表。");
            AddSubnodeDefault(node, "DownloadError", "在下载以下文件时发生错误：");
            AddSubnodeDefault(node, "FileDownloadError", "有些文件显然没有成功下载。重新运行更新检查和更新。");
            AddSubnodeDefault(node, "HelloImageNotLoaded", "新闻图片无法加载。");
            AddSubnodeDefault(node, "VersionNotVerified", "启动器无法验证是否为最新。如果这个问题持续存在，请通知你的支持团队。");
            AddSubnodeDefault(node, "VersionNotVerifiedFileNotFound", "未找到VersionNotVerifiedFileNotFound。");
            AddSubnodeDefault(node, "VersionNotVerifiedFileParseError", "VersionNotVerifiedFileParseError。");
            AddSubnodeDefault(node, "CouldNotBeUpdated", "启动器尝试了自我更新，但没有成功。如果问题持续存在，请尝试重新运行启动器并联系你的支持团队。");
            AddSubnodeDefault(node, "OutdatedLauncher", "网上似乎有一个新版本的启动器。启动器将尝试更新然后重启。");
            AddSubnodeDefault(node, "LauncherUpdated", "启动器已成功更新。请再次运行启动器。");
            AddSubnodeDefault(node, "Removing", "正在移除文件：");

            defaultXml.DocumentElement.AppendChild(comment);
            defaultXml.DocumentElement.AppendChild(node);
        }


        /// <summary>
        /// Save default XML as new LauncherConfig.xml, overwrite an old one.
        /// </summary>
        private void SaveDefault()
        {
            TextWriter tw = new StreamWriter("LauncherConfig.xml", false, Encoding.UTF8);
            defaultXml.Save(tw);
            tw.Close();
        }
        #endregion
    }
}