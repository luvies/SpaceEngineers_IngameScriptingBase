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
        const char Newline = '\n';
        static readonly CSharpMinifier.Minifier _minifier = new CSharpMinifier.Minifier(new CSharpMinifier.MinifierOptions()
        { // set up options specifically so we have nicer control
            LocalVarsCompressing = true,
            MembersCompressing = true,
            TypesCompressing = true,
            SpacesRemoving = true,
            CommentsRemoving = true,
            RegionsRemoving = true,
            MiscCompressing = true,
            NamespacesRemoving = true,
            PublicCompressing = false,
            ToStringMethodsRemoving = false,
            UselessMembersCompressing = true,
            EnumToIntConversion = true
        });
        const string _minifyFormatBlock = "public class Program {{{0}\n}}"; // otherwise minifier removes everything (public keeps name in check)
        static readonly Regex _minifyExtract = new Regex(@"^public class Program ?{(.*)}$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        static readonly CustomBlock[] customBlocks =
        {
            new CustomBlock("-", "-", buf => ""), // remove all block
            new CustomBlock("m", "m", buf => // minify block
            {
                // split to allow nicer debugging
                string blk = string.Format(_minifyFormatBlock, buf); // format into class block to enable proper output
                blk = _minifier.MinifyFromString(blk); // minify block
                blk = _minifyExtract.Match(blk).Groups[1].Value; // extract from class block
                return blk;
            })
        };

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

            char prev = '\0'; // the previous character
            StringBuilder buffer = new StringBuilder(); // the buffer for multiline comment internals
            StringBuilder customBlockBuffer = new StringBuilder(); // the buffer to store the contents of a custom block
            StringBuilder builder = new StringBuilder(); // the current built file

            bool inString = false; // whether we are in a string (preserve everything unless we exit)
            bool inSingleComment = false; // whether we are in a single comment (preserve if keep single comments, ignore multiline comments)
            bool inMultiComment = false; // whether we are in a multiline comment (preserve if keep multi comments, if contents match custom block then switch into/out of)
            bool multiCommentJust_In = false; // whether we just entered a multiline comment
            bool multiCommentJust_Out = false; // whether we just exited a multiline comment
            bool inCustomBlock = false; // whether we are in a custom block (all contents is buffered and processed later)
            CustomBlock currentBlock = null; // the custom block we are currently in
            bool inWhiteSpace = false; // whether we are in whitespace (ignore other whitespace if not preserve)
            foreach (char ch in content) // loop through content
            {
                if (!inString) // if not in a string, then do normal processing
                {
                    if (inMultiComment) // if in a multiline comment
                    {
                        if (!multiCommentJust_In && prev == '*') // if we just encountered a '*' char (so we can detect '*/' which would close the comment)
                        {
                            switch (ch)
                            {
                                case '/': // if the multiline block was closed
                                    if (inCustomBlock) // if we are in a custom block
                                    {
                                        if (currentBlock.ShouldClose(buffer.ToString())) // if the content of the multicomment is the close tag
                                        {
                                            currentBlock.ProcessBlock(ref builder, ref customBlockBuffer); // process custom block content
                                            customBlockBuffer.Clear(); // clear the buffer
                                            currentBlock = null; // unset the current block
                                            inCustomBlock = false; // we are no longer in a custom block
                                        }
                                    }
                                    else // this was a standard multiline comment
                                    {
                                        string buf = buffer.ToString(); // get the comment content
                                        foreach (var block in customBlocks) // check if the content matches any custom block openenings
                                            if (block.ShouldOpen(buf))
                                            {
                                                currentBlock = block;
                                                break;
                                            }
                                        if (currentBlock != null) // if the comment should open a custom block, do it
                                            inCustomBlock = true;
                                        if (!inCustomBlock && !settings.RemoveMultiComments) // if this wasn't a custom block opening, process normal multiline comment
                                            builder.Append(string.Format("/*{0}*/", buffer.ToString()));
                                    }

                                    buffer.Clear(); // clear multiline comment buffer
                                    inMultiComment = false; // no longer in a multiline comment
                                    multiCommentJust_Out = true; // we just exited
                                    break;
                                default: // we are in the content of the multiline still
                                    buffer.Append(prev); // append the ignore previous char
                                    if (ch != '*') // ignore '*' to stop duplication (since this switch will be ran next char)
                                        buffer.Append(ch);
                                    break;
                            }
                        }
                        else if (multiCommentJust_In || ch != '*') // if we just entered a multiline comment or the char is not '*'
                        { // this stops detection of '/*/' as a complete multiline comment
                            buffer.Append(ch);
                        }

                        multiCommentJust_In = false; // we have been in a multiline comment for at least 1 char now
                    }
                    else if (inSingleComment) // if in a single line comment
                    {
                        if (ch == Newline) // if we are at a newline char, exit the comment
                        {
                            if (!settings.RemoveSingleComments) // if we should keep the comment, add it and the newline
                            {
                                builder.Append(buffer.ToString());
                                builder.Append(ch);
                            }
                            else if (!settings.RemoveNewlines) // otherwise if we are keeping newlines, add just that
                                builder.Append(ch);
                            buffer.Clear(); // clear the single line buffer
                            inSingleComment = false; // no longer in a single line comment
                            inWhiteSpace = true; // we are now in whitespace
                        }
                        else // otherwise buffer the char
                            buffer.Append(ch);
                    }
                    else // we are in normal code space
                    {
                        if (!multiCommentJust_Out && prev == '/') // if we haven't just left a multiline comment and we have found a '/'
                        { // this stops '/**//' being counted as a multiline and then a single line comment
                            switch (ch)
                            {
                                case '*': // if a multiline comment was opened
                                    inMultiComment = true; // now in a multiline comment
                                    multiCommentJust_In = true; // we just got in (means that the '*' char is ignored next iteration)
                                    break;
                                case '/': // if a single line comment was opened
                                    if (inCustomBlock) // if in a custom block add it to the block buffer
                                    {
                                        customBlockBuffer.Append(prev);
                                        customBlockBuffer.Append(ch);
                                    }
                                    else // otherwise we are now in a single line comment, so add the '//' to the buffer
                                    {
                                        inSingleComment = true;
                                        buffer.Append(prev);
                                        buffer.Append(ch);
                                    }
                                    break;
                                default:
                                    if (inCustomBlock) // if in a custom block, add the chars to the block buffer
                                    {
                                        customBlockBuffer.Append(prev);
                                        customBlockBuffer.Append(ch);
                                    }
                                    else // otherwise we just encountered the divide operator, so add it and this char
                                    {
                                        builder.Append(prev);
                                        builder.Append(ch);
                                    }
                                    break;
                            }
                        }
                        else if (ch != '/') // if we are not on a '/' char (for comment detection)
                        {
                            if (inCustomBlock) // if in a custom block, add it to the block buffer
                                customBlockBuffer.Append(ch);
                            else // otherwise we are in normal code, and need to deal with whitespace before adding it to the builder
                            {
                                if (inWhiteSpace && !IsWhitespace(ch)) // if we are in whitespace and we isn't enough whitespace char
                                {
                                    if (ch != Newline) // if this is a newline, still add it but say that we are still in whitespace
                                        inWhiteSpace = false;
                                    builder.Append(ch);
                                }
                                else if (!inWhiteSpace) // otherwise if in non-whitespace, work out if we just entered whitespace
                                {
                                    char c = ch;
                                    if (ch == Newline && settings.RemoveNewlines)
                                        c = ' ';
                                    builder.Append(c);
                                    if ((IsWhitespace(c) || c == Newline) && settings.RemoveWhitespace)
                                        inWhiteSpace = true;
                                }
                            }
                        }

                        multiCommentJust_Out = false; // we have been out of a multiline comment for at least 1 char now
                    }
                }
                else
                    builder.Append(ch); // append the char regarless if in a string

                // if we are in a custom block, multiline comment, or single line comment, then we aren't going to be entering a proper string
                // also, if we are in a string but have just encountered a '\"', then ignore it as an ending (doesn't support @"..." strings yet)
                if (!inCustomBlock && !inMultiComment && !inSingleComment && (ch == '"' && (!inString || prev != '\\')))
                    inString = !inString; // switch in a out of a string

                prev = ch; // set the previous char
            }

            // output the built program to the output file
            File.WriteAllText(Path.Combine(settings.FileOutPath, file), builder.ToString().Trim());
        }

        static bool IsWhitespace(char c) => char.IsWhiteSpace(c) && c != Newline; // whether the char is whitespace or not (but ignore newlines as whitespace due to separate handling)

    }
}
