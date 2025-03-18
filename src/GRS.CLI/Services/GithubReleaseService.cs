using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GRS.Services
{

    public interface IGithubReleaseService
    {
        Task<GitHubRelease> CheckForUpdate(string rawRepoUrl, string currentVersion = "");

        Task<GitHubRelease> CheckForUpdate(string owner, string repository, string currentVersion = "");
    }
    public class GithubReleaseService : IGithubReleaseService
    {
        private readonly HttpClient _httpClient;
        internal string UserAgent = "grs-cli-ua";

        public GithubReleaseService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        }
        public async Task<GitHubRelease> CheckForUpdate(string rawRepoUrl, string currentVersion = "")
        {
            var regex = new Regex(@"github\.com/([^/]+/[^/]+)", RegexOptions.IgnoreCase);
            var match = regex.Match(rawRepoUrl);
            string repo = "";
            if (match.Success)
            {
                repo = match.Groups[1].Value; // Retorna "owner/repo"
            }

            return await InternalCheckForUpdate(repo, currentVersion);

        }
        public async Task<GitHubRelease> CheckForUpdate(string owner, string repository, string currentVersion = "")
        {
            string repo = $"{owner}/{repository}"; // Nome do repositório no formato 'owner/repository'

            return await InternalCheckForUpdate(repo, currentVersion);
        }


        private async Task<GitHubRelease> InternalCheckForUpdate(string repo, string currentVersion)
        {
            // Displaying a formatted message using Spectre.Console
            var repoName = repo.Split('/').Last();
            AnsiConsole.WriteLine();
            // Displaying the formatted message in the console
            AnsiConsole.MarkupLineInterpolated($"[bold yellow]Checking for updates[/] for repository [bold cyan]{repoName}[/] (Current Version: [dim]{(string.IsNullOrEmpty(currentVersion) ? "N/A" : currentVersion)}[/]).");

            GitHubRelease releaseInfo = await GetLatestReleaseInfo(repo);

            if (releaseInfo == null)
            {
                // Using an icon and color for an error message
                AnsiConsole.MarkupLine("[bold red]Error:[/] Unable to fetch the latest release.");
            }
            else if (currentVersion != releaseInfo.Version)
            {
                // Using colors and version formatting with emphasis
                AnsiConsole.MarkupLine($"[yellow]New version available:[/] [bold green]{releaseInfo.Version}[/]");

                // Creating a table to display download links
                var table = new Table();
                table.AddColumn("[bold yellow]Release content[/]");

                foreach (var downloadUrl in releaseInfo.DownloadUrlList)
                {
                    table.AddRow(downloadUrl);
                }

                AnsiConsole.Write(table);
            }
            else
            {
                // Using green color to indicate the version is up-to-date
                AnsiConsole.MarkupLine("[bold green]You are already using the latest version.[/]");
            }

            return releaseInfo;
        }



        internal async Task<GitHubRelease> GetLatestReleaseInfo(string repo)
        {
            GitHubRelease result = null;
            try
            {
                string url = $"https://github.com/{repo}/releases/latest";

                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent); // User-Agent necessário

                var html = await _httpClient.GetStringAsync(url);

                // Regex para extrair a versão (tag) e o link do ativo
                var tagMatch = Regex.Match(html, @"\/releases\/tag\/(v?\d+\.\d+\.\d+)");
                // Regex para capturar todos os links de download


                string version = "";
                if (tagMatch.Success)
                {
                    version = tagMatch.Groups[1].Value;
                }


                List<string> downloadUrls = await GetAllDownloadLinks(repo, version);

                result = new GitHubRelease
                {
                    TagName = version,
                    DownloadUrlList = downloadUrls
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar a última release: {ex.Message}");
            }
            return result;
        }

        internal async Task<List<string>> GetAllDownloadLinks(string repo, string version)
        {
            List<string> downloadUrls = new List<string>();
            try
            {
                // A URL das assets expandidas
                string url = $"https://github.com/{repo}/releases/expanded_assets/{version}";

                // User-Agent necessário

                var html = await _httpClient.GetStringAsync(url);

                var assetMatches = Regex.Matches(html, @"href=\""/([^\""]+/releases/download/[^\""]+)\""");


                foreach (Match match in assetMatches)
                {
                    string downloadUrl = $"https://github.com/{match.Groups[1].Value}";
                    downloadUrls.Add(downloadUrl);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar links de download: {ex.Message}");
            }

            return downloadUrls;
        }

    }
    public class GitHubRelease
    {
        public string TagName { get; set; } // Nome do tag da release (ex.: "v3.0.5")
        public string Version => TagName;
        public List<string> DownloadUrlList { get; set; } // URL do ativo para download
    }
}
