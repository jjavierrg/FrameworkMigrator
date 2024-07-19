using FrameworkMigrator.Command;
using FrameworkMigrator.Core;
using FrameworkMigrator.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FrameworkMigrator.Registry
{
    internal static class Registry
    {
        public static void RegisterDependencies(this IServiceCollection services)
        {
            services.AddTransient<ILogger, ConsoleLogger>();
            services.AddTransient<ICommandExecutor, CommandExecutor>();
            services.AddTransient<IGitClient, GitClient>();
            services.AddTransient<IFileOperator, FileOperator>();
            services.AddTransient<IFileReplacer, FileReplacer>();
            services.AddTransient<Migrator>();
        }
    }
}
