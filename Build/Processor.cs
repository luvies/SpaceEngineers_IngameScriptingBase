using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Build
{
    class Processor
    {
        const string CustomCommentBlockOpen = "-"; // usage: /*<string>*/
        const string CustomCommentBlockClose = "-"; // usage: /*<string>*/
        const char Newline = '\n';

        public static void Process(Settings settings)
        {
            List<string> files = new List<string>();
            foreach (var file in Directory.GetFiles(settings.FileInPath, "*.cs"))
            {
                string fname = Path.GetFileName(file);
                if (!settings.IgnoreFiles.Contains(fname))
                {
                    Console.WriteLine("Found '{0}'", fname);
                    files.Add(fname);
                }
            }

            Console.WriteLine("Found {0} files, processing...", files.Count);

            foreach (string fname in files)
                ProcessFile(fname, settings);
        }

        /// <summary>
        /// Processes the file using the settings.
        /// </summary>
        /// <param name="file">The file to process.</param>
        /// <param name="settings">The current settings configuration.</param>
        static void ProcessFile(string file, Settings settings)
        {
            string content = File.ReadAllText(Path.Combine(settings.FileInPath, file)).Replace("\r", "");

            char prev = '\0';
            StringBuilder buffer = new StringBuilder();
            StringBuilder builder = new StringBuilder();

            bool inString = false;
            bool inSingleComment = false;
            bool inMultiComment = false;
            bool multiCommentJust_In = false;
            bool multiCommentJust_Out = false;
            bool inCustomBlock = false;
            bool inWhiteSpace = false;
            foreach (char ch in content)
            {
                if (!inString)
                {
                    if (inMultiComment)
                    {
                        if (!multiCommentJust_In && prev == '*')
                        {
                            switch (ch)
                            {
                                case '/':
                                    string check = inCustomBlock ? CustomCommentBlockClose : CustomCommentBlockOpen;
                                    if (buffer.ToString() == check)
                                    {
                                        inCustomBlock = !inCustomBlock;
                                    }
                                    else if (!inCustomBlock && !settings.RemoveMultiComments)
                                        builder.Append(string.Format("/*{0}*/", buffer.ToString()));

                                    buffer.Clear();
                                    inMultiComment = false;
                                    multiCommentJust_Out = true;
                                    break;
                                default:
                                    buffer.Append(prev);
                                    if (ch != '*')
                                        buffer.Append(ch);
                                    break;
                            }
                        }
                        else if (multiCommentJust_In || ch != '*')
                        {
                            buffer.Append(ch);
                        }

                        multiCommentJust_In = false;
                    }
                    else if (inSingleComment)
                    {
                        if (ch == Newline)
                        {
                            if (!settings.RemoveSingleComments)
                            {
                                builder.Append(buffer.ToString());
                                builder.Append(ch);
                            }
                            else if (!settings.RemoveNewlines)
                                builder.Append(ch);
                            buffer.Clear();
                            inSingleComment = false;
                            inWhiteSpace = true;
                        }
                        else
                            buffer.Append(ch);
                    }
                    else
                    {
                        if (!multiCommentJust_Out && prev == '/')
                        {
                            switch (ch)
                            {
                                case '*':
                                    inMultiComment = true;
                                    multiCommentJust_In = true;
                                    break;
                                case '/':
                                    if (!inCustomBlock)
                                    {
                                        inSingleComment = true;
                                        buffer.Append(prev);
                                        buffer.Append(ch);
                                    }
                                    break;
                                default:
                                    if (!inCustomBlock)
                                    {
                                        builder.Append(prev);
                                        builder.Append(ch);
                                    }
                                    break;
                            }
                        }
                        else if (!inCustomBlock && ch != '/')
                        {
                            if (inWhiteSpace && !IsWhitespace(ch))
                            {
                                if (ch != Newline)
                                    inWhiteSpace = false;
                                builder.Append(ch);
                            }
                            else if (!inWhiteSpace)
                            {
                                char c = ch;
                                if (ch == Newline && settings.RemoveNewlines)
                                    c = ' ';
                                builder.Append(c);
                                if ((IsWhitespace(c) || c == Newline) && settings.RemoveWhitespace)
                                    inWhiteSpace = true;
                            }
                        }

                        multiCommentJust_Out = false;
                    }
                }
                else
                    builder.Append(ch);

                if (!inCustomBlock && !inMultiComment && !inSingleComment && (ch == '"' && (!inString || prev != '\\')))
                    inString = !inString;

                prev = ch;
            }

            File.WriteAllText(Path.Combine(settings.FileOutPath, file), builder.ToString().Trim());
        }

        static bool IsWhitespace(char c) => char.IsWhiteSpace(c) && c != Newline;

    }
}
