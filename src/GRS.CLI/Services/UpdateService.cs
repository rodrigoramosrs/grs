using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GRS.Services
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient = new();

        public async Task InternalValidateAndApplyUpdate(
                                GitHubRelease githubReleaseInfo = null,
                                string currentVersion = null,
                                string saveDirectory = null,
                                bool forceUpdate = false)
        {
            if (githubReleaseInfo == null)
            {
                AnsiConsole.MarkupLine("[red]No release information provided.[/]");
                return;
            }

            if (!forceUpdate && !IsNewVersion(githubReleaseInfo.Version, currentVersion))
            {
                AnsiConsole.MarkupLine("[yellow]No update required. The current version is up-to-date.[/]");
                return;
            }

            var downloadUrl = SelectDownloadUrl(githubReleaseInfo);

            if (string.IsNullOrEmpty(downloadUrl))
            {
                AnsiConsole.MarkupLine("[red]No compatible download URL found.[/]");
                return;
            }

            if (string.IsNullOrEmpty(saveDirectory))
            {
                AnsiConsole.MarkupLine("[red]Save directory not specified.[/]");
                return;
            }


            else if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            try
            {
                string backupZipPath = Path.Combine(saveDirectory, $"backup-before-update-{DateTime.Now:yyyyMMddHHmmssfff}.zip");
                string downloadedPath = Path.Combine(saveDirectory, Path.GetFileName(downloadUrl));

                // 1. Criar backup da pasta atual
                AnsiConsole.MarkupLine("[cyan]Step 1 - Creating backup...[/]");
                CreateBackupZip(saveDirectory, backupZipPath);

                // 2. Baixar nova versão
                AnsiConsole.MarkupLine("[cyan]Step 2 - Downloading update...[/]");
                await DownloadFileAsync(downloadUrl, downloadedPath);

                // 3. Se for zip, extrair
                if (downloadUrl.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine("[cyan]Step 3 - Extracting update...[/]");
                    ExtractZip(downloadedPath, saveDirectory);

                    File.Delete(downloadedPath);
                }

                AnsiConsole.MarkupLine("[green]Update successfully applied![/]");
                AnsiConsole.MarkupLine($"[gray]backup: {backupZipPath}[/]");

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred during the update process: {ex.Message}[/]");
            }
        }

        private string SelectDownloadUrl(GitHubRelease githubReleaseInfo)
        {
            var currentOSKeywords = GetCurrentOSKeywords();
            var currentArchKeywords = GetCurrentArchitectureKeywords();

            return githubReleaseInfo.DownloadUrlList?
                .FirstOrDefault(url =>
                    currentOSKeywords.Any(keyword => url.Contains(keyword, StringComparison.OrdinalIgnoreCase)) &&
                    currentArchKeywords.Any(keyword => url.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                ?? githubReleaseInfo.DownloadUrlList?.FirstOrDefault();
        }

        // Métodos auxiliares para validar OS e arquitetura
        private IEnumerable<string> GetCurrentOSKeywords()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new[] { "win", "windows" };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return new[] { "linux", "ubuntu", "debian" };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return new[] { "mac", "osx", "darwin" };
            return Array.Empty<string>();
        }

        private IEnumerable<string> GetCurrentArchitectureKeywords()
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.X86 => new[] { "x86", "32bit" },
                Architecture.X64 => new[] { "x64", "amd64", "64bit" },
                Architecture.Arm => new[] { "arm", "armv7" },
                Architecture.Arm64 => new[] { "arm64", "aarch64" },
                _ => Array.Empty<string>()
            };
        }

        private bool IsNewVersion(string newVersion, string currentVersion)
        {
            return string.Compare(newVersion, currentVersion, StringComparison.OrdinalIgnoreCase) > 0;
        }

        private void CreateBackupZip(string sourceDirectory, string destinationZipPath)
        {
            if (!Directory.Exists(sourceDirectory) || Directory.EnumerateFileSystemEntries(sourceDirectory).GetEnumerator().MoveNext() == false) return;

            string tempDirectory = Path.Combine(Environment.CurrentDirectory, "grs-temp");

            try
            {
                // Create temporary directory
                if (!Directory.Exists(tempDirectory))
                    Directory.CreateDirectory(tempDirectory);

                // Copy files to the temporary directory
                foreach (string filePath in Directory.GetFiles(sourceDirectory))
                {
                    if (Regex.IsMatch(Path.GetFileName(filePath), "backup-before-update.*\\.zip")) continue;

                    string destinationPath = Path.Combine(tempDirectory, Path.GetFileName(filePath));
                    File.Copy(filePath, destinationPath);
                }

                // Create the zip file in the backup directory
                ZipFile.CreateFromDirectory(tempDirectory, destinationZipPath, CompressionLevel.Optimal, true);
            }
            finally
            {
                // Delete the temporary directory
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }

        }

        private async Task DownloadFileAsync(string url, string destinationPath)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs);
        }

        private void ExtractZip(string zipPath, string destinationDirectory)
        {
            //if (Directory.Exists(destinationDirectory))
            //{
            //    Directory.Delete(destinationDirectory, true);
            //}

            ZipFile.ExtractToDirectory(zipPath, destinationDirectory, overwriteFiles: true);
        }



    }
}
