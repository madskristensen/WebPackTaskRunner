using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace WebPackTaskRunner
{
    internal static class TaskRunnerFileBindingsHelper
    {
        internal const string _bindingElementName = "binding";
        internal readonly static string _configRegex = "///\\s*<" + _bindingElementName + ".*/>";

        public static bool SaveBindings(string configPath, string bindingsXml)
        {
            IVsTextView configTextView = VsTextViewUtil.FindTextViewFor(configPath);

            if (configTextView != null)
            {
                return SaveBindingsToTextEditor(configTextView, bindingsXml);
            }
            else
            {
                return SaveBindingsToFile(configPath, bindingsXml);
            }
        }

        private static bool SaveBindingsToTextEditor(IVsTextView configTextView, string bindingsXml)
        {
            string newConfigLineText = "/// " + bindingsXml;

            int configLineIndex;
            string oldConfigLineText;

            if (FindConfigLine(configTextView, out configLineIndex, out oldConfigLineText))
            {
                // Don't remove the newline at the end
                oldConfigLineText = oldConfigLineText.TrimEnd();

                return ReplaceConfigLine(configTextView, configLineIndex, oldConfigLineText.Length, newConfigLineText);
            }
            else
            {
                return InsertConfigLine(configTextView, 0, newConfigLineText + "\r\n");
            }
        }

        private static bool FindConfigLine(IVsTextView configTextView, out int configLineIndex, out string configLineText)
        {
            configLineText = null;

            for (configLineIndex = 0;
                 String.IsNullOrWhiteSpace(configLineText);
                 configLineIndex++)
            {
                int hr = configTextView.GetTextStream(
                    configLineIndex,
                    0,
                    configLineIndex + 1,
                    0,
                    out configLineText);

                if (hr != VSConstants.S_OK || configLineText == null)
                {
                    break;
                }

                if (IsConfigLine(configLineText))
                {
                    return true;
                }
            }

            configLineIndex = -1;
            configLineText = null;
            return false;
        }

        private static bool ReplaceConfigLine(IVsTextView textView, int lineIndex, int lengthToReplace, string newText)
        {
            EditPoint editPoint;

            if (GetEditPointAtBeginningOfLine(textView, lineIndex, out editPoint))
            {
                try
                {
                    editPoint.ReplaceText(lengthToReplace, newText, Flags: 0);

                    return true;
                }
                catch
                {
                    // DTE methods throw exceptions on failure. Catch and fall
                    // through to the error case.
                }
            }

            return false;
        }

        private static bool InsertConfigLine(IVsTextView textView, int lineIndex, string insertText)
        {
            EditPoint editPoint;

            if (GetEditPointAtBeginningOfLine(textView, lineIndex, out editPoint))
            {
                try
                {
                    editPoint.Insert(insertText);

                    return true;
                }
                catch
                {
                    // DTE methods throw exceptions on failure. Catch and fall
                    // through to the error case.
                }
            }

            return false;
        }

        private static bool GetEditPointAtBeginningOfLine(IVsTextView textView, int lineIndex, out EditPoint editPoint)
        {
            IVsTextLines textLines;

            int hr = textView.GetBuffer(out textLines);
            if (hr == VSConstants.S_OK && textLines != null)
            {
                object editPointObject;

                hr = textLines.CreateEditPoint(lineIndex, 0, out editPointObject);
                if (hr == VSConstants.S_OK)
                {
                    editPoint = editPointObject as EditPoint;

                    return editPoint != null;
                }
            }

            editPoint = null;
            return false;
        }

        private static bool SaveBindingsToFile(string configPath, string bindingsXml)
        {
            string oldContent = string.Empty;

            try
            {
                if (File.Exists(configPath))
                {
                    oldContent = File.ReadAllText(configPath);
                }

                string configText;

                if (!RemoveBindingsXml(oldContent, out configText))
                {
                    configText = oldContent;
                }

                File.WriteAllText(configPath, WriteMetaDataToWebPackConfigText(configText, bindingsXml));

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string WriteMetaDataToWebPackConfigText(string configText, string bindingsXml)
        {
            StringBuilder text = new StringBuilder();

            text.Append("/// ");
            text.Append(bindingsXml);
            text.Append("\r\n");
            text.Append(configText);

            return text.ToString();
        }

        public static string LoadBindings(string configPath)
        {
            try
            {
                using (var stream = new StreamReader(configPath))
                {
                    return ReadBindingsFromStream(stream);
                }
            }
            catch
            {
                return null;
            }
        }

        internal static bool IsConfigLine(string line)
        {
            Match match = Regex.Match(line, _configRegex);

            return match.Success;
        }

        private static void FindFirstNonWhitespaceLine(string configText, ref int start, ref int end)
        {
            for (start = 0; start < configText.Length; start++)
            {
                if (!char.IsWhiteSpace(configText[start]))
                {
                    // Found first non-whitespace character
                    break;
                }
            }

            for (end = start + 1; end < configText.Length; end++)
            {
                if (configText[end] == '\n' ||
                    configText[end] == '\r')
                {
                    end--;

                    break;
                }
            }
        }

        internal static bool FindConfigLine(string configText, out int start, out int end)
        {
            start = 0;
            end = 0;

            if (string.IsNullOrEmpty(configText))
            {
                return false;
            }

            // Get first non-whitespace line
            FindFirstNonWhitespaceLine(configText, ref start, ref end);

            string possibleConfigLine = configText.Substring(start, end - start + 1);

            if (IsConfigLine(possibleConfigLine))
            {
                return true;
            }

            // Did not find the config line
            return false;
        }

        private static string ReadBindingsFromStream(TextReader reader)
        {
            string xmlText = null;
            string configText = reader.ReadToEnd();
            int start, end;

            if (FindConfigLine(configText, out start, out end))
            {
                // Extract config text
                string configComment = configText.Substring(start, end - start + 1);

                // Remove leading "///"
                xmlText = configComment.Substring(3);

            }

            return xmlText;
        }

        /// <summary>
        /// Returns true if bindings XML comment was successfully removed from the config
        /// text.
        /// </summary>
        private static bool RemoveBindingsXml(string configText, out string newConfigText)
        {
            newConfigText = configText;

            int start, end;

            if (FindConfigLine(configText, out start, out end))
            {
                StringBuilder updatedConfig = new StringBuilder();

                if (start > 0)
                {
                    updatedConfig.Append(configText.Substring(0, start));
                }

                // Skip past whitespace
                for (end++; end < configText.Length; end++)
                {
                    if (configText[end] != '\n' &&
                        configText[end] != '\r')
                    {
                        break;
                    }
                }

                if (end < configText.Length - 1)
                {
                    updatedConfig.Append(configText.Substring(end));
                }

                newConfigText = updatedConfig.ToString();

                return true;
            }

            // Did not find config line.
            return false;
        }
    }
}
