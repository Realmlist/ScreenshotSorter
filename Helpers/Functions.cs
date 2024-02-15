using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Runtime.CompilerServices;
using System.Text;
using Image = SixLabors.ImageSharp.Image;

namespace ScreenshotSorter;

internal class Functions
{
    // Folder picker & returns path to Entry in params
    public static async Task BrowseFolder(Entry entry)
    {
        var folder = await FolderPicker.PickAsync(default);

        if (folder.IsSuccessful && folder.Folder.Path != null)
        {
            entry.Text = folder.Folder.Path;
        }
        else
        {
            throw new ArgumentException($"The folder was not picked with error: {folder.Exception.Message}");
        }
    }

    public static async Task Sort(string screenshotsFolder, bool chbMove, string? targetFolder, int monthNotation, bool sortByDay, bool cleanUp, bool convertToWebp)
    {
        if (string.IsNullOrEmpty(screenshotsFolder))
        {
            throw new ArgumentException($"'{nameof(screenshotsFolder)}' cannot be null or empty.", nameof(screenshotsFolder));
        }

        if (string.IsNullOrEmpty(targetFolder) || chbMove == false)
        {
            targetFolder = screenshotsFolder;
        }
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"screenshotsFolder: {screenshotsFolder}");
        System.Diagnostics.Debug.WriteLine($"targetFolder: {targetFolder}");
#endif
        // Get the files which should be moved, without folders
        string[] extensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp" };
        var files = new DirectoryInfo(screenshotsFolder)
            .EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(file => extensions.Contains(file.Extension.ToLower()))
            .ToList();

        foreach (var file in files)
        {
            // Get year and month of the file
            int year = file.LastWriteTime.Year;
            int intMonth = file.LastWriteTime.Month;
            string stringMonth = file.LastWriteTime.ToString("MMMM");
            int day = file.LastWriteTime.Day;

            // 0 = 01
            // 1 = January
            // 2 = 01 - January

            string monthNotationString = "";
            switch (monthNotation)
            {
                case 0:
                    monthNotationString = intMonth.ToString();
                    break;
                case 1:
                    monthNotationString = stringMonth;
                    break;
                case 2:
                    monthNotationString = $"{intMonth} - {stringMonth}";
                    break;
            }

            // Set Directory Path
            string directory;
            if (sortByDay)
            {
                directory = Path.Combine(targetFolder, year.ToString(), $"{monthNotationString}", $"{day}");
            }
            else
            {
                directory = Path.Combine(targetFolder, year.ToString(), $"{monthNotationString}");
            }
            

            // Create directory if it doesn't exist
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Move File to new location
            string destinationFile = Path.Combine(directory, file.Name);
            File.Move(file.FullName, destinationFile);
        }

        if (cleanUp)
        {
            await Cleanup(screenshotsFolder, targetFolder);
        }

        if (convertToWebp)
        {
            await ConvertToWebp(targetFolder);
        }

    }

    public static async Task Cleanup(string screenshotsFolder, string targetFolder)
    {
        // Delete empty folders in single directory
        var cleanupTask = Task.WhenAll(
            CleanupDirectoryAsync(screenshotsFolder)
        );
        // Delete empty folders in both directories
        var cleanupTasks = Task.WhenAll(
            CleanupDirectoryAsync(screenshotsFolder),
            CleanupDirectoryAsync(targetFolder)
        );

        try
        {
            if(screenshotsFolder == targetFolder)
            {
                await cleanupTask;
            }
            else
            {
                await cleanupTasks;
            }
            
        }
        catch (AggregateException ex)
        {
            // Handle exceptions from directory deletion
            foreach (var innerEx in ex.InnerExceptions)
            {
                throw new ArgumentException($"Error while deleting directory: {innerEx.Message}");
            }
        }
    }


    public static async Task CleanupDirectoryAsync(string path)
    {
        var directories = new DirectoryInfo(path)
            .EnumerateDirectories("*", SearchOption.AllDirectories)
            .Where(dir => !Directory.EnumerateFileSystemEntries(dir.FullName).Any())
            .ToList();

        var deleteTasks = directories.Select(directory => Task.Run(() =>
        {
            try
            {

                Directory.Delete(directory.FullName, false); // Set recursive to true if you want to delete non-empty directories.
            }
            catch (Exception ex)
            {
                // Handle individual directory deletion exceptions, if needed
                throw new ArgumentException($"Error while deleting directory: {ex.Message}");
            }
        }));

        await Task.WhenAll(deleteTasks);
    }

    public static async Task ConvertToWebp(string targetFolder)
    {
        // Get the files which should be moved, without folders
        string[] extensions = { ".png", ".jpg", ".jpeg", ".bmp" };
        var files = new DirectoryInfo(targetFolder)
            .EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(file => extensions.Contains(file.Extension.ToLower()))
            .ToList();

        foreach (var file in files)
        {
            using var image = await Image.LoadAsync(file.FullName);
            
            var outputFile = $"{file.DirectoryName}\\{Path.GetFileNameWithoutExtension(file.FullName)}.webp";

#if DEBUG
            System.Diagnostics.Debug.WriteLine(outputFile);
#endif

            while (File.Exists(outputFile))
            {
                // Generate 4 random characters
                string randomChars = GenerateRandomChars(4);

                // Append the random characters to the filename (before the extension)
                string outputFileNameWithoutExtension = Path.GetFileNameWithoutExtension(outputFile);
                string extension = Path.GetExtension(outputFile);
                outputFile = $"{file.DirectoryName}\\{outputFileNameWithoutExtension}_{randomChars}{extension}";
            }

            await image.SaveAsWebpAsync(outputFile, new WebpEncoder()
            {
                Method = WebpEncodingMethod.BestQuality,
                NearLossless = true,
                NearLosslessQuality = 60
            });
      
        }

        static string GenerateRandomChars(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var randomChars = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                randomChars.Append(chars[random.Next(chars.Length)]);
            }
            return randomChars.ToString();
        }
    }
}
