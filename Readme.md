# Framework Migrator

This is a tool to help you migrate your azure function projects from previous versions of the .NET framework to the new isolated process model of .NET 8.

This tool will download the repository, create a new branch with the changes and stage the changes for commit. It will replace old references to the Azure Functions SDK with the new ones and update the project files to use the new isolated process model as described in the [official documentation](https://learn.microsoft.com/en-us/azure/azure-functions/migrate-dotnet-to-isolated-model?tabs=net8).

## Usage

To use the tool, just execute the console application and provide the following arguments:

- `--path` or `-p`: The path to your git parent directory.
- `--branch` or `-b`: The branch that will be created with the changes.
- RepoUrl: The URL of the repository to migrate

Example:

```shell
FrameworkMigrator.exe --path "C:\Users\user\source\repos" --branch "migrate-to-net8" <RepoUrl>
```

## Customization

If you need to customize the migration process, you can modify the **Replaces.json** file. This file contains the replacements that will be made in the project files.
