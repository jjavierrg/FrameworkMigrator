using FracturedJson;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FrameworkMigrator.Core
{
    internal interface IFileReplacer
    {
        Task<IEnumerable<ReplaceResult>> Replace(FileReplace fileReplace, string folder);
    }

    internal class FileReplacer : IFileReplacer
    {
        private readonly Formatter _jsonFormatter;

        public FileReplacer()
        {
            var options = new FracturedJsonOptions
            {
                CommentPolicy = CommentPolicy.Preserve,
                CommentPadding = false,
                MaxInlineComplexity = 0,
                MaxTableRowComplexity = -1,
                MaxCompactArrayComplexity = -1,
            };
            _jsonFormatter = new Formatter { Options = options };
        }

        public async Task<IEnumerable<ReplaceResult>> Replace(FileReplace fileReplace, string folder)
        {
            var files = Directory.GetFiles(folder, fileReplace.FilePattern, SearchOption.AllDirectories);
            var replaceResults = new List<ReplaceResult>();

            foreach (var file in files)
            {
                var replaceResult = await ReplaceInFile(fileReplace, file);

                if (replaceResult.ReplacementsMade > 0)
                    replaceResults.Add(replaceResult);
            }

            return replaceResults;
        }

        private async Task<ReplaceResult> ReplaceInFile(FileReplace fileReplace, string filePath)
        {
            var text = await File.ReadAllTextAsync(filePath);
            var replacementsMade = 0;

            foreach (var replacement in fileReplace.Replacements)
            {
                if (!string.IsNullOrWhiteSpace(replacement.ReplaceIfRegex))
                {
                    var conditionalRegex = new Regex(replacement.ReplaceIfRegex, RegexOptions.Multiline);
                    var match = conditionalRegex.Match(text);

                    if (!match.Success)
                        continue;
                }

                var regex = new Regex(replacement.OldRegexValue, RegexOptions.Multiline);
                var newValue = string.Join("\r\n", replacement.NewValues);
                var newText = regex.Replace(text, newValue);

                if (newText != text)
                    replacementsMade++;

                text = newText;
            }

            if (replacementsMade > 0)
                await SaveAndFormatFile(filePath, text, fileReplace.FileFormat);

            var fileName = Path.GetFileName(filePath);
            return new ReplaceResult { FileName = fileName, ReplacementsMade = replacementsMade };
        }

        private async Task SaveAndFormatFile(string filePath, string text, string? fileFormat)
        {
            text = fileFormat switch
            {
                FileFormat.Json => _jsonFormatter.Reformat(text),
                FileFormat.Xml => XDocument.Parse(text).ToString(),
                _ => text
            };

            await File.WriteAllTextAsync(filePath, text);
        }
    }

    internal static class FileFormat
    {
        public const string Json = "Json";
        public const string Xml = "Xml";
    }

    internal class FileReplace
    {
        public string FilePattern { get; set; } = string.Empty;
        public IEnumerable<Replace> Replacements { get; set; } = [];
        public string? FileFormat { get; set; }
    }

    internal class Replace
    {
        public string OldRegexValue { get; set; } = string.Empty;
        public string ReplaceIfRegex { get; set; } = string.Empty;
        public IEnumerable<string> NewValues { get; set; } = [];
    }

    internal class ReplaceResult
    {
        public string FileName { get; set; } = string.Empty;
        public int ReplacementsMade { get; set; } = 0;
    }
}
