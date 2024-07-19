using CommandLine;
using FrameworkMigrator.Core;
using FrameworkMigrator.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace FrameworkMigrator
{
    internal class Program
    {
        public class Options
        {
            [Option('p', "path", Required = true, HelpText = "Set your parent repo folder")]
            public string RepoFolder { get; set; } = string.Empty;

            [Option('b', "Branch", Required = true, HelpText = "Git branch name to create")]
            public string BranchName { get; set; } = string.Empty;

            [Value(0, MetaName = "RepoUrl", HelpText = "Set the URL for the repository", Required = true)]
            public string RepoUrl { get; set; } = string.Empty;
        }

        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.RegisterDependencies();

            var serviceProvider = services.BuildServiceProvider();

            await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(opt =>
            {
                var migrator = serviceProvider.GetRequiredService<Migrator>();
                return migrator.ExecuteAsync(opt);
            });
        }
    }
}
