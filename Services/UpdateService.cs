using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Octokit;

namespace DungeonGame.Services
{
    public class UpdateService
    {
        private readonly string _currentVersion;
        private readonly string _repositoryOwner = "galihru";
        private readonly string _repositoryName = "dungeon";
        private readonly GitHubClient _gitHubClient;

        public UpdateService()
        {
            _currentVersion = GetCurrentVersion();
            _gitHubClient = new GitHubClient(new ProductHeaderValue("DungeonGame"));
        }

        private string GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly()
                          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                          .InformationalVersion ?? "1.0.0";
        }

        public async Task<UpdateInfo?> CheckForUpdatesAsync()
        {
            try
            {
                var releases = await _gitHubClient.Repository.Release
                    .GetAll(_repositoryOwner, _repositoryName);

                if (releases.Count == 0) return null;

                var latestRelease = releases[0];
                var latestVersion = CleanVersionString(latestRelease.TagName);
                var currentVersion = CleanVersionString(_currentVersion);

                if (IsNewerVersion(latestVersion, currentVersion))
                {
                    return new UpdateInfo
                    {
                        Version = latestVersion,
                        ReleaseNotes = latestRelease.Body,
                        DownloadUrl = GetWindowsAssetUrl(latestRelease),
                        ReleaseDate = latestRelease.CreatedAt.DateTime,
                        IsPrerelease = latestRelease.Prerelease
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
                return null;
            }
        }

        private string CleanVersionString(string version)
        {
            return version.TrimStart('v', 'V').Split('-')[0];
        }

        private bool IsNewerVersion(string latest, string current)
        {
            try
            {
                var latestVersion = Version.Parse(latest);
                var currentVersion = Version.Parse(current);
                return latestVersion > currentVersion;
            }
            catch
            {
                return string.Compare(latest, current, StringComparison.OrdinalIgnoreCase) > 0;
            }
        }

        private string? GetWindowsAssetUrl(Release release)
        {
            foreach (var asset in release.Assets)
            {
                var name = asset.Name.ToLower();
                if (name.Contains("windows") || name.Contains("win") ||
                    name.EndsWith(".exe") || name.EndsWith(".msi"))
                {
                    return asset.BrowserDownloadUrl;
                }
            }
            return release.Assets.Count > 0 ? release.Assets[0].BrowserDownloadUrl : null;
        }

        public async Task<bool> DownloadAndInstallUpdateAsync(string downloadUrl, IProgress<int>? progress = null)
        {
            try
            {
                var tempPath = Path.GetTempFileName();
                var extension = Path.GetExtension(downloadUrl) ?? ".exe";
                var tempFile = Path.ChangeExtension(tempPath, extension);

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0;
                    var downloadedBytes = 0L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            if (totalBytes > 0)
                            {
                                var progressPercentage = (int)((downloadedBytes * 100) / totalBytes);
                                progress?.Report(progressPercentage);
                            }
                        }
                    }
                }

                // Launch installer
                var startInfo = new ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true,
                    Verb = "runas" // Request admin privileges
                };

                Process.Start(startInfo);

                // Close current application
                System.Windows.Application.Current.Shutdown();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update installation failed: {ex.Message}");
                return false;
            }
        }
    }

    public class UpdateInfo
    {
        public string Version { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string? DownloadUrl { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool IsPrerelease { get; set; }
    }
}
