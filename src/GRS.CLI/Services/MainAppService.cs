using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GRS.RunCommand;
using static System.Net.WebRequestMethods;

namespace GRS.Services
{
    public class MainAppService
    {
        private readonly IGithubReleaseService _githubReleaseService;
        private readonly UpdateService _updateService;

        public MainAppService(IGithubReleaseService githubReleaseService, UpdateService updateService)
        {
            _githubReleaseService = githubReleaseService;
            _updateService = updateService;
        }
        public async Task Run(string rawRepoUrl = null,
                              string currentVersion = null,
                              string saveDirectory = null,
                              bool forceUpdate = false)
        {
            if (rawRepoUrl != null)
            {
                var githubRelease = await _githubReleaseService.CheckForUpdate(rawRepoUrl: rawRepoUrl);
                await _updateService.InternalValidateAndApplyUpdate(githubRelease, currentVersion, saveDirectory, forceUpdate);
            }
        }

        public async Task Run(string owner = null, string repository = null,
                              string currentVersion = null,
                              string saveDirectory = null,
                              bool forceUpdate = false)
        {
            if (owner != null && repository != null)
            {
                var githubRelease = await _githubReleaseService.CheckForUpdate(owner: owner, repository: repository);
                await _updateService.InternalValidateAndApplyUpdate(githubRelease, currentVersion, saveDirectory, forceUpdate);
            }


            //https://github.com/curl/curl](https://github.com/curl/curl)
            //
            //await _githubReleaseService.CheckForUpdate(owner: "sensepost", repository: "gowitness");
        }

    }
}
