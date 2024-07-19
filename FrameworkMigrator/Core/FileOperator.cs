using System.Diagnostics;

namespace FrameworkMigrator.Core
{
    internal class FileOperator : IFileOperator
    {
        private readonly IFileReplacer _fileReplacer;

        public FileOperator(IFileReplacer fileReplacer) => _fileReplacer = fileReplacer ?? throw new ArgumentNullException(nameof(fileReplacer));

        public async Task<IEnumerable<ReplaceResult>> ReplaceInFiles(IEnumerable<FileReplace> replacements, string folderPath)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

            if (replacements == null || !replacements.Any())
                return [];

            var results = new List<ReplaceResult>();
            foreach (var fileReplace in replacements)
            {
                var result = await _fileReplacer.Replace(fileReplace, folderPath);
                results.AddRange(result);
            }

            return results;
        }

        public Task CopyFileToDestFolder(string sourceFilePath, string destFolder, string workingDirectory)
        {
            var destination = string.IsNullOrWhiteSpace(destFolder) ? workingDirectory : Directory.GetDirectories(workingDirectory, destFolder, SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(destination))
                throw new DirectoryNotFoundException($"Folder not found: {destFolder}");

            var fileName = Path.GetFileName(sourceFilePath);
            var destFilePath = Path.Combine(destination, fileName);

            File.Copy(sourceFilePath, destFilePath, true);
            return Task.CompletedTask;
        }

        public Task OpenFileWithDefaultApp(string folderPath, string searchPattern)
        {
            var filePath = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(filePath))
                throw new FileNotFoundException($"File not found: {searchPattern}");

            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            return Task.CompletedTask;
        }

        public Task<bool> DirectoryExistsByPattern(string folderPath, string searchPattern) => Task.FromResult(Directory.GetDirectories(folderPath, searchPattern, SearchOption.AllDirectories).Any());
    }

    internal interface IFileOperator
    {
        /// <summary>
        /// Replaces the specified text in the files that match the specified patterns in the specified folder. If the folder is not found, an exception is thrown. If folder has subfolders, it will search recursively.
        /// </summary>
        /// <returns>The number of replacements made in each file.</returns>
        Task<IEnumerable<ReplaceResult>> ReplaceInFiles(IEnumerable<FileReplace> replacements, string folderPath);

        /// <summary>
        /// Copies the specified file to the specified destination folder.
        /// <param name="sourceFilePath"></param>
        /// <param name="destFolder"></param>
        /// <returns></returns>
        Task CopyFileToDestFolder(string sourceFilePath, string destFolder, string workingDirectory);

        Task OpenFileWithDefaultApp(string folderPath, string searchPattern);
        Task<bool> DirectoryExistsByPattern(string folderPath, string searchPattern);
    }
}
