using System.Diagnostics;

namespace FrameworkMigrator.Command
{
    internal class CommandExecutor : ICommandExecutor
    {
        public async Task<string> ExecuteCommandAsync(string command, string arguments, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"Error al ejecutar comando git: {error}");

            return output;
        }
    }

    internal interface ICommandExecutor
    {
        Task<string> ExecuteCommandAsync(string command, string arguments, string workingDirectory);
    }
}
