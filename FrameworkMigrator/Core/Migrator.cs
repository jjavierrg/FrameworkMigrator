using FrameworkMigrator.Command;
using Microsoft.Extensions.Logging;
using static FrameworkMigrator.Program;

namespace FrameworkMigrator.Core
{
    internal class Migrator(ILogger logger, IGitClient gitClient, IFileOperator fileOperator)
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IGitClient _gitClient = gitClient ?? throw new ArgumentNullException(nameof(gitClient));
        private readonly IFileOperator _fileOperator = fileOperator ?? throw new ArgumentNullException(nameof(fileOperator));

        public async Task ExecuteAsync(Options options)
        {
            var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var strWorkPath = Path.GetDirectoryName(strExeFilePath) ?? "";

            var jsonFile = $"{strWorkPath}\\Replaces.json";
            var jsonContent = File.ReadAllText(jsonFile);
            var replacements = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<FileReplace>>(jsonContent) ?? [];

            Console.ResetColor();

            if (string.IsNullOrWhiteSpace(options.RepoUrl))
            {
                Console.WriteLine("Introduce la URL del repositorio Git:");
                options.RepoUrl = Console.ReadLine() ?? "";
            }

            try
            {
                _gitClient.ReposFolder = options.RepoFolder;
                _gitClient.RepoUrl = options.RepoUrl;

                _logger.LogInformation("Obteniendo la última versión repositorio...");
                var projectFolder = await _gitClient.DownloadRepository();

                _logger.LogInformation($"Creando rama {options.BranchName}...");
                await _gitClient.CreateBranchOnLocalAndRemote(options.BranchName);

                _logger.LogInformation("Copiando nuevos archivos...");
                await _fileOperator.CopyFileToDestFolder($"{strWorkPath}\\Assets\\.gitignore", "", projectFolder);
                var existsServiceFolder = await _fileOperator.DirectoryExistsByPattern(projectFolder, "src\\*Service");
                var folder = existsServiceFolder ? "src\\*Service" : "src\\*";
                await _fileOperator.CopyFileToDestFolder($"{strWorkPath}\\Assets\\Program.cs", folder, projectFolder);

                _logger.LogInformation("Realizando cambios en el código...");
                var replacementsMade = await _fileOperator.ReplaceInFiles(replacements, projectFolder);
                replacementsMade.ToList().GroupBy(x => x.FileName).ToList().ForEach(r => _logger.LogInformation($"    Replacements made in {r.Key}: {r.Sum(x => x.ReplacementsMade)}"));

                _logger.LogInformation("Añadiendo cambios al stage...");
                await _gitClient.StageFiles();

                _logger.LogInformation("Abriendo solución...");
                await _fileOperator.OpenFileWithDefaultApp(projectFolder, "*.sln");

                _logger.LogInformation("Proceso finalizado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ejecutar migración: {ex.Message}");
            }
        }
    }
}
