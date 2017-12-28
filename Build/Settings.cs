using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Build
{
    class Settings
    {
        const string FileLocation = "settings.xml";
        const string SettingsRoot_Tag = "settings";
        const int SettingValueTabSize = 4;
        const string SettingValueMessage = "{0}{1} set to {2}";
        const string TagSkipUnknownMessage = "Skipping unknown tag '{0}'";
        const string TagSkipBoolMessage = "Skipping tag '{0}' because value is not of type 'bool'";
        const string TagSkipStrMessage = "Skipping tag '{0}' due to lack of value";

        /// <summary>
        /// The directory to read the files in.
        /// </summary>
        /// <remarks>
        /// Defaults to '{base folder path}/Scripts'.
        /// </remarks>
        public string FileInPath { get; private set; }
        const string FileInPath_Tag = "path_files_in";

        /// <summary>
        /// The directory to write the files out.
        /// </summary>
        /// <remarks>
        /// Defaults to '{base folder path}/Scripts/out'.
        /// </remarks>
        public string FileOutPath { get; private set; }
        const string FileOutPath_Tag = "path_files_out";

        /// <summary>
        /// The files to ignore when building
        /// </summary>
        /// <remarks>
        /// Defaults to a list containing just 'BaseProgram.cs'.
        /// </remarks>
        public List<string> IgnoreFiles { get; private set; }
        const string IgnoreFiles_Tag = "ignore_files";
        const string IgnoreFileItem_Tag = "file";

        /// <summary>
        /// Whether to remove multline comments.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        public bool RemoveMultiComments { get; private set; }
        const string RemoveMultiComments_Tag = "remove_multiline_comments";

        /// <summary>
        /// Whether to remove newlines.
        /// </summary>
        /// <remarks>
        /// Defaults to false.
        /// </remarks>
        public bool RemoveNewlines { get; private set; }
        const string RemoveNewlines_Tag = "remove_newlines";

        /// <summary>
        /// Whether to remove single line comments.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        public bool RemoveSingleComments { get; private set; }
        const string RemoveSingleComments_Tag = "remove_singleline_comments";

        /// <summary>
        /// Whether to remove whitespace.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        public bool RemoveWhitespace { get; private set; }
        const string RemoveWhitespace_Tag = "remove_whitespace";

        public Settings()
        {
            FileInPath = Path.Combine("..", "..", "..", "Scripts");
            FileOutPath = Path.Combine(FileInPath, "out");
            IgnoreFiles = new List<string>()
            {
                "BaseProgram.cs"
            };
            RemoveMultiComments = true;
            RemoveNewlines = false;
            RemoveSingleComments = true;
            RemoveWhitespace = true;
        }

        public static Settings LoadSettings()
        {
            Settings settings = new Settings();
            if (File.Exists(FileLocation))
            {
                try
                {
                    XDocument xdoc = XDocument.Load(FileLocation);
                    foreach (XElement xel in xdoc.Root.Elements())
                    {
                        switch (xel.Name.LocalName)
                        {
                            case FileInPath_Tag:
                                if (!string.IsNullOrEmpty(xel.Value))
                                    settings.FileInPath = xel.Value;
                                else
                                    Console.WriteLine(TagSkipStrMessage, FileInPath_Tag);
                                break;
                            case FileOutPath_Tag:
                                if (!string.IsNullOrEmpty(xel.Value))
                                    settings.FileOutPath = xel.Value;
                                else
                                    Console.WriteLine(TagSkipStrMessage, FileOutPath_Tag);
                                break;
                            case IgnoreFiles_Tag:
                                settings.IgnoreFiles.Clear();
                                foreach (XElement xelFile in xel.Elements())
                                {
                                    if (xelFile.Name == IgnoreFileItem_Tag)
                                    {
                                        if (!string.IsNullOrEmpty(xelFile.Value))
                                            settings.IgnoreFiles.Add(xelFile.Value);
                                        else
                                            Console.WriteLine(TagSkipStrMessage, IgnoreFileItem_Tag);
                                    }
                                    else
                                        Console.WriteLine(TagSkipUnknownMessage, xelFile.Name);
                                }
                                break;
                            case RemoveMultiComments_Tag:
                                {
                                    if (bool.TryParse(xel.Value, out bool value))
                                        settings.RemoveMultiComments = value;
                                    else
                                        Console.WriteLine(TagSkipBoolMessage, RemoveMultiComments_Tag);
                                }
                                break;
                            case RemoveNewlines_Tag:
                                {
                                    if (bool.TryParse(xel.Value, out bool value))
                                        settings.RemoveNewlines = value;
                                    else
                                        Console.WriteLine(TagSkipBoolMessage, RemoveNewlines_Tag);
                                }
                                break;
                            case RemoveSingleComments_Tag:
                                {
                                    if (bool.TryParse(xel.Value, out bool value))
                                        settings.RemoveSingleComments = value;
                                    else
                                        Console.WriteLine(TagSkipBoolMessage, RemoveSingleComments_Tag);
                                }
                                break;
                            case RemoveWhitespace_Tag:
                                {
                                    if (bool.TryParse(xel.Value, out bool value))
                                        settings.RemoveWhitespace = value;
                                    else
                                        Console.WriteLine(TagSkipBoolMessage, RemoveWhitespace_Tag);
                                }
                                break;
                            default:
                                Console.WriteLine(TagSkipUnknownMessage, xel.Name);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load setting ({0}), generating new settings using defaults", ex.Message);
                    settings = GenerateNewSettings();
                }
            }
            else
            {
                Console.WriteLine("No settings file found, generating one using default settings");
                settings = GenerateNewSettings();
            }

            Console.WriteLine("Loaded settings and using the following values:");
            string tabstr = new string(' ', SettingValueTabSize);
            Console.WriteLine(SettingValueMessage, tabstr, FileInPath_Tag, settings.FileInPath);
            Console.WriteLine(SettingValueMessage, tabstr, FileOutPath_Tag, settings.FileOutPath);
            Console.WriteLine(SettingValueMessage, tabstr, IgnoreFiles_Tag, string.Join(",", settings.IgnoreFiles));
            Console.WriteLine(SettingValueMessage, tabstr, RemoveMultiComments_Tag, settings.RemoveMultiComments);
            Console.WriteLine(SettingValueMessage, tabstr, RemoveNewlines_Tag, settings.RemoveNewlines);
            Console.WriteLine(SettingValueMessage, tabstr, RemoveSingleComments_Tag, settings.RemoveSingleComments);
            Console.WriteLine(SettingValueMessage, tabstr, RemoveWhitespace_Tag, settings.RemoveWhitespace);

            return settings;
        }

        public static Settings GenerateNewSettings()
        {
            Settings defSettings = new Settings();
            List<XElement> ignoreFiles = new List<XElement>();
            foreach (string ignoreFile in defSettings.IgnoreFiles)
                ignoreFiles.Add(new XElement(IgnoreFileItem_Tag, ignoreFile));
            XDocument xdoc = new XDocument(
                new XElement(SettingsRoot_Tag,
                    new XElement(FileInPath_Tag, defSettings.FileInPath),
                    new XElement(FileOutPath_Tag, defSettings.FileOutPath),
                    new XElement(IgnoreFiles_Tag, ignoreFiles),
                    new XElement(RemoveMultiComments_Tag, defSettings.RemoveMultiComments),
                    new XElement(RemoveNewlines_Tag, defSettings.RemoveNewlines),
                    new XElement(RemoveSingleComments_Tag, defSettings.RemoveSingleComments),
                    new XElement(RemoveWhitespace_Tag, defSettings.RemoveWhitespace)
                ));
            xdoc.Save(FileLocation);
            return defSettings;
        }
    }
}
