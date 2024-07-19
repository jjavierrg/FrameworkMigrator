namespace FrameworkMigrator.Command
{
    internal class GitClient(ICommandExecutor commandExecutor) : IGitClient
    {
        private readonly ICommandExecutor _commandExecutor = commandExecutor ?? throw new ArgumentNullException(nameof(commandExecutor));

        public string ReposFolder { get; set; }
        public string RepoUrl { get; set; }

        private string RepoFolder
        {
            get
            {
                var repoName = RepoUrl.Split("/").Last().Replace(".git", "");
                return Path.Combine(ReposFolder, repoName);
            }
        }
        public Task StageFiles() => commandExecutor.ExecuteCommandAsync("git", "add .", RepoFolder);

        public async Task<string> DownloadRepository()
        {
            var gitCommand = Directory.Exists(RepoFolder) ? PullRepository() : CloneRepository();
            await gitCommand;

            return RepoFolder;
        }
        public async Task CreateBranchOnLocalAndRemote(string branchName)
        {
            await commandExecutor.ExecuteCommandAsync("git", $"checkout -f -b {branchName}", RepoFolder);
            await commandExecutor.ExecuteCommandAsync("git", $"push origin {branchName}", RepoFolder);
        }
        private Task CloneRepository() => commandExecutor.ExecuteCommandAsync("git", $"clone {RepoUrl}", ReposFolder);
        private async Task PullRepository()
        {
            await commandExecutor.ExecuteCommandAsync("git", $"fetch", RepoFolder);
            await commandExecutor.ExecuteCommandAsync("git", $"switch -f main", RepoFolder);
            await commandExecutor.ExecuteCommandAsync("git", "pull origin", RepoFolder);
        }
    }

    internal interface IGitClient
    {
        string ReposFolder { get; set; }
        string RepoUrl { get; set; }

        Task CreateBranchOnLocalAndRemote(string branchName);
        Task<string> DownloadRepository();
        Task StageFiles();
    }
}
