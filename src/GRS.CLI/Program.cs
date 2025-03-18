using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GRS.Services;

namespace GRS
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Exibe cabeçalho estilizado
            DisplayHeader();

            var services = new ServiceCollection(); // Inicia IServiceCollection
            ConfigureServices(services); // Configura os serviços

            var app = new CommandApp(new TypeRegistrar(services));

            app.Configure(config =>
            {
                config.AddCommand<RunCommand>("update")
                      .WithDescription("Run the application with specific parameters.")
                      .WithExample(new[] { "update", "--url", "https://github.com/user/repo" })
                      .WithExample(new[] { "update", "--owner", "user", "--repo", "repository" });
            });

            return await app.RunAsync(args);
        }

        static void DisplayHeader()
        {
            AnsiConsole.Write(
                new FigletText("Github Release Sync")
                    .Centered()
                    .Color(Color.Aqua));

            AnsiConsole.MarkupLine("[bold cyan]Stay updated with [bold yellow]GitHub Release Sync (GRS)[/][/]\n" +
                      "[dim]Ensure your software is always running the latest version with seamless updates directly from GitHub releases. Stay ahead with [bold green]GRS[/] for effortless version management.[/]");

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule().RuleStyle("gray"));
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainAppService>();
            services.AddTransient<IGithubReleaseService, GithubReleaseService>();
            services.AddTransient<UpdateService>();

        }
    }

    public class RunCommand : AsyncCommand<RunCommand.Settings>
    {
        private readonly MainAppService _appService;

        public RunCommand(MainAppService appService)
        {
            _appService = appService;
        }

        public class Settings : CommandSettings
        {
            [CommandOption("--url <RAWREPOURL>")]
            [Description("The raw repository URL.")]
            public string? RawRepoUrl { get; set; }

            [CommandOption("--owner <OWNER>")]
            [Description("The repository owner.")]
            public string? Owner { get; set; }

            [CommandOption("--repo <REPOSITORY>")]
            [Description("The repository name.")]
            public string? Repository { get; set; }

            [CommandOption("--current-version <VERSION>")]
            [Description("The current version of the software.")]
            public string? CurrentVersion { get; set; }

            [CommandOption("--save-dir <DIRECTORY>")]
            [Description("The directory where the new version will be saved.")]
            public string? SaveDirectory { get; set; }

            [CommandOption("--force")]
            [Description("Force update regardless of the current version.")]
            public bool ForceUpdate { get; set; }

            public override ValidationResult Validate()
            {
                if (!string.IsNullOrWhiteSpace(RawRepoUrl) && string.IsNullOrWhiteSpace(Owner) && string.IsNullOrWhiteSpace(Repository))
                {
                    return ValidationResult.Success();
                }

                if (!string.IsNullOrWhiteSpace(Owner) && !string.IsNullOrWhiteSpace(Repository) && string.IsNullOrWhiteSpace(RawRepoUrl))
                {
                    return ValidationResult.Success();
                }

                return ValidationResult.Error("[red]Provide either --url or both --owner and --repo parameters.[/]");
            }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.RawRepoUrl))
            {
                AnsiConsole.MarkupLine($"[green]Running with Repo URL: {settings.RawRepoUrl}[/]");
            }
            else if (!string.IsNullOrWhiteSpace(settings.Owner) && !string.IsNullOrWhiteSpace(settings.Repository))
            {
                AnsiConsole.MarkupLine($"[green]Running with Owner: {settings.Owner} and Repository: {settings.Repository}[/]");
            }
            AnsiConsole.MarkupLine($"[yellow]Current Version: {settings.CurrentVersion ?? "N/A"}[/]");
            AnsiConsole.MarkupLine($"[blue]Save Directory: {settings.SaveDirectory ?? "N/A"}[/]");
            AnsiConsole.MarkupLine($"[red]Force Update: {settings.ForceUpdate}[/]");

            if (!string.IsNullOrWhiteSpace(settings.RawRepoUrl))
            {
                await _appService.Run(rawRepoUrl: settings.RawRepoUrl,
                                    currentVersion: settings.CurrentVersion ?? "",
                                    saveDirectory: Path.GetFullPath(settings.SaveDirectory ?? ""),
                                    forceUpdate: settings.ForceUpdate);

            }
            else if (!string.IsNullOrWhiteSpace(settings.Owner) && !string.IsNullOrWhiteSpace(settings.Repository))
            {
                await _appService.Run(owner: settings.Owner, repository: settings.Repository,
                                    currentVersion: settings.CurrentVersion,
                                    saveDirectory: settings.SaveDirectory,
                                    forceUpdate: settings.ForceUpdate);
            }

            return 0;
        }
    }

    public sealed class TypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceCollection _services;

        public TypeRegistrar(IServiceCollection services)
        {
            _services = services;
        }

        public ITypeResolver Build()
        {
            return new TypeResolver(_services.BuildServiceProvider());
        }

        public void Register(Type service, Type implementation)
        {
            _services.AddSingleton(service, implementation);
        }

        public void RegisterInstance(Type service, object implementation)
        {
            _services.AddSingleton(service, implementation);
        }

        public void RegisterLazy(Type service, Func<object> factory)
        {
            _services.AddSingleton(service, provider => factory());
        }
    }

    public sealed class TypeResolver : ITypeResolver
    {
        private readonly ServiceProvider _provider;

        public TypeResolver(ServiceProvider provider)
        {
            _provider = provider;
        }

        public object? Resolve(Type type)
        {
            return _provider.GetService(type);
        }
    }

}







