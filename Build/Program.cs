using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;
using System.Collections;

namespace Build
{
    class Program
    {
        private static Regex RgxCustomCommentBlock = new Regex(Properties.Resources.CustomCommentBlock, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex RgxMultiComment = new Regex(Properties.Resources.MultiComment, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex RgxNewlines = new Regex(Properties.Resources.Newlines, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RgxSingleComment = new Regex(Properties.Resources.SingleComment, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RgxWhitespace = new Regex(Properties.Resources.Whitespace, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        private static string CurrentDir = Directory.GetCurrentDirectory();

        private static class Settings
        {
            public const string FileLocation = "settings.xml";
            public const string SettingsRoot_Tag = "settings";
            public const int SettingValueTabSize = 4;
            public const string SettingValueMessage = "{0}{1} set to {2}";
            public const string TagSkipUnknownMessage = "Skipping unknown tag '{0}'";
            public const string TagSkipBoolMessage = "Skipping tag '{0}' because value is not of type 'bool'";
            public const string TagSkipStrMessage = "Skipping tag '{0}' due to lack of value";

            public static string FileInPath = Path.Combine("..", "..", "..", "Scripts");
            public const string FileInPath_Tag = "path_files_in";

            public static string FileOutPath = Path.Combine(FileInPath, "out");
            public const string FileOutPath_Tag = "path_files_out";

            public static List<string> IgnoreFiles = new List<string>(); // BaseProgram.cs is ignored using the default settings file
            public const string IgnoreFiles_Tag = "ignore_files";
            public const string IgnoreFileItem_Tag = "file";

            public static bool RemoveMultiComments = true;
            public const string RemoveMultiComments_Tag = "remove_multiline_comments";

            public static bool RemoveNewlines = false;
            public const string RemoveNewlines_Tag = "remove_newlines";

            public static bool RemoveSingleComments = true;
            public const string RemoveSingleComments_Tag = "remove_singleline_comments";

            public static bool RemoveWhitespace = true;
            public const string RemoveWhitespace_Tag = "remove_whitespace";
        }

        /// <summary>
        /// A simulation class for python3 range class.
        /// </summary>
        /// <remarks>
        /// It's likely that some of it is unnecessary, however I might use it in the future.
        /// (taken from https://docs.python.org/3/library/stdtypes.html#range )
        /// </remarks>
        class Range : IEnumerable<int>
        {
            private int start = 0;
            private int stop;
            private int step = 1;

            public Range(int stop)
            {
                this.stop = stop;
            }

            public Range(int start, int stop)
            {
                this.start = start;
                this.stop = stop;
            }

            public Range(int start, int stop, int step)
            {
                this.start = start;
                this.stop = stop;
                if (step == 0)
                    throw new ArgumentException("Step cannot be 0");
                this.step = step;
            }

            public int this[int i]
            {
                get
                {
                    if (i < 0)
                        throw new IndexOutOfRangeException();
                    int value = start + (step * i);
                    if ((value > 0 && value < stop) || (value < 0 && value > stop))
                        return value;
                    throw new IndexOutOfRangeException();
                }
            }

            public IEnumerator<int> GetEnumerator()
            {
                for (int i = start; i < stop; i += step)
                    yield return i;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public bool Contains(int value)
            {
                if (value > 0)
                    return start <= value && value < stop;
                else
                    return stop < value && value <= start;
            }

            public static bool operator ==(Range left, Range right)
            {
                return left.start == right.start && left.stop == right.stop && left.step == right.step;
            }

            public static bool operator !=(Range left, Range right)
            {
                return !(left == right);
            }

            public override bool Equals(object obj)
            {
                //       
                // See the full list of guidelines at
                //   http://go.microsoft.com/fwlink/?LinkID=85237  
                // and also the guidance for operator== at
                //   http://go.microsoft.com/fwlink/?LinkId=85238
                //

                if (obj == null || (GetType() != obj.GetType() && !(obj is IEnumerable<int>)))
                {
                    return false;
                }

                if (GetType() == obj.GetType())
                    return this == (Range)obj;
                else
                    return this.SequenceEqual((IEnumerable<int>)obj);
            }

            public override int GetHashCode()
            {
                // TODO: write your implementation of GetHashCode() here
                // throw new NotImplementedException();
                return base.GetHashCode();
            }
        }

        static void Main(string[] args)
        {
            LoadSettings();
            Console.WriteLine();

            if (!Directory.Exists(Settings.FileInPath))
            {
                Console.WriteLine("File in path '{0}' not found, are you sure it exists? (relative directories allowed)", Settings.FileInPath);
                Pause();
                return;
            }
            if (!Directory.Exists(Settings.FileOutPath))
                Directory.CreateDirectory(Settings.FileOutPath);

            List<string> files = new List<string>();
            foreach (var file in Directory.GetFiles(Settings.FileInPath, "*.cs"))
            {
                string fname = Path.GetFileName(file);
                if (!Settings.IgnoreFiles.Contains(fname))
                {
                    Console.WriteLine("Found '{0}'", fname);
                    files.Add(fname);
                }
            }

            Console.WriteLine("Found {0} files, processing...", files.Count);

            foreach (string fname in files)
            {
                string content;
                content = File.ReadAllText(Path.Combine(Settings.FileInPath, fname));

                List<Regex> rgxes = new List<Regex>();
                rgxes.Add(RgxCustomCommentBlock);
                if (Settings.RemoveMultiComments)
                    rgxes.Add(RgxMultiComment);
                if (Settings.RemoveSingleComments)
                    rgxes.Add(RgxSingleComment);

                List<Range> charIgnores = new List<Range>();
                foreach (Regex rx in rgxes)
                    foreach (Match match in rx.Matches(content))
                        charIgnores.Add(new Range(match.Index, match.Index + match.Length));
                StringBuilder contentBuilder = new StringBuilder(content);
                foreach (Range range in charIgnores)
                    foreach (int i in range)
                        contentBuilder[i] = ' ';
                content = contentBuilder.ToString();

                if (Settings.RemoveWhitespace)
                    content = RgxWhitespace.Replace(content, " ");
                if (Settings.RemoveNewlines)
                    content = RgxNewlines.Replace(content, " ");
                else if (Settings.RemoveWhitespace && !Settings.RemoveNewlines)
                    content = RgxNewlines.Replace(content, "\r\n");

                File.WriteAllText(Path.Combine(Settings.FileOutPath, fname), content.Trim());
            }

            Pause();
        }

        private static void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void LoadSettings()
        {
            if (File.Exists(Settings.FileLocation))
            {
                try
                {
                    XDocument xdoc = XDocument.Load(Settings.FileLocation);
                    foreach (XElement xel in xdoc.Root.Elements())
                    {
                        if (xel.Name == Settings.FileInPath_Tag)
                        {
                            if (!string.IsNullOrEmpty(xel.Value))
                                Settings.FileInPath = xel.Value;
                            else
                                Console.WriteLine(Settings.TagSkipStrMessage, Settings.FileInPath_Tag);
                        }
                        else if (xel.Name == Settings.FileOutPath_Tag)
                        {
                            if (!string.IsNullOrEmpty(xel.Value))
                                Settings.FileOutPath = xel.Value;
                            else
                                Console.WriteLine(Settings.TagSkipStrMessage, Settings.FileOutPath_Tag);
                        }
                        else if (xel.Name == Settings.IgnoreFiles_Tag)
                        {
                            Settings.IgnoreFiles.Clear();
                            foreach (XElement xelFile in xel.Elements())
                            {
                                if (xelFile.Name == Settings.IgnoreFileItem_Tag)
                                {
                                    if (!string.IsNullOrEmpty(xelFile.Value))
                                        Settings.IgnoreFiles.Add(xelFile.Value);
                                    else
                                        Console.WriteLine(Settings.TagSkipStrMessage, Settings.IgnoreFileItem_Tag);
                                }
                                else
                                    Console.WriteLine(Settings.TagSkipUnknownMessage, xelFile.Name);
                            }
                        }
                        else if (xel.Name == Settings.RemoveMultiComments_Tag)
                        {
                            if (bool.TryParse(xel.Value, out bool value))
                                Settings.RemoveMultiComments = value;
                            else
                                Console.WriteLine(Settings.TagSkipBoolMessage, Settings.RemoveMultiComments_Tag);
                        }
                        else if (xel.Name == Settings.RemoveNewlines_Tag)
                        {
                            if (bool.TryParse(xel.Value, out bool value))
                                Settings.RemoveNewlines = value;
                            else
                                Console.WriteLine(Settings.TagSkipBoolMessage, Settings.RemoveNewlines_Tag);
                        }
                        else if (xel.Name == Settings.RemoveSingleComments_Tag)
                        {
                            if (bool.TryParse(xel.Value, out bool value))
                                Settings.RemoveSingleComments = value;
                            else
                                Console.WriteLine(Settings.TagSkipBoolMessage, Settings.RemoveSingleComments_Tag);
                        }
                        else if (xel.Name == Settings.RemoveWhitespace_Tag)
                        {
                            if (bool.TryParse(xel.Value, out bool value))
                                Settings.RemoveWhitespace = value;
                            else
                                Console.WriteLine(Settings.TagSkipBoolMessage, Settings.RemoveWhitespace_Tag);
                        }
                        else
                            Console.WriteLine(Settings.TagSkipUnknownMessage, xel.Name);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load setting ({0}), generating new settings using defaults", ex.Message);
                    GenerateNewSettings();
                }
            }
            else
            {
                Console.WriteLine("No settings file found, generating one using default settings");
                GenerateNewSettings();
            }

            Console.WriteLine("Loaded settings and using the following values:");
            string tabstr = new string(' ', Settings.SettingValueTabSize);
            Console.WriteLine(Settings.SettingValueMessage, tabstr, Settings.FileInPath_Tag, Settings.FileInPath);
            Console.WriteLine(Settings.SettingValueMessage, tabstr, Settings.FileOutPath_Tag, Settings.FileOutPath);
            Console.WriteLine(Settings.SettingValueMessage, tabstr, Settings.IgnoreFiles_Tag, string.Join(",", Settings.IgnoreFiles));
            Console.WriteLine(Settings.SettingValueMessage, tabstr, Settings.RemoveMultiComments_Tag, Settings.RemoveMultiComments);
            Console.WriteLine(Settings.SettingValueMessage, tabstr, Settings.RemoveNewlines_Tag, Settings.RemoveNewlines);
            Console.WriteLine(Settings.SettingValueMessage, tabstr, Settings.RemoveSingleComments_Tag, Settings.RemoveSingleComments);
            Console.WriteLine(Settings.SettingValueMessage, tabstr, Settings.RemoveWhitespace_Tag, Settings.RemoveWhitespace);
        }

        private static void GenerateNewSettings()
        {
            Settings.IgnoreFiles.Add("BaseProgram.cs");
            List<XElement> ignoreFiles = new List<XElement>();
            foreach (string ignoreFile in Settings.IgnoreFiles)
                ignoreFiles.Add(new XElement(Settings.IgnoreFileItem_Tag, ignoreFile));
            XDocument xdoc = new XDocument(
                new XElement(Settings.SettingsRoot_Tag,
                    new XElement(Settings.FileInPath_Tag, Settings.FileInPath),
                    new XElement(Settings.FileOutPath_Tag, Settings.FileOutPath),
                    new XElement(Settings.IgnoreFiles_Tag, ignoreFiles),
                    new XElement(Settings.RemoveMultiComments_Tag, Settings.RemoveMultiComments),
                    new XElement(Settings.RemoveNewlines_Tag, Settings.RemoveNewlines),
                    new XElement(Settings.RemoveSingleComments_Tag, Settings.RemoveSingleComments),
                    new XElement(Settings.RemoveWhitespace_Tag, Settings.RemoveWhitespace)
                ));
            xdoc.Save(Settings.FileLocation);
        }
    }
}
