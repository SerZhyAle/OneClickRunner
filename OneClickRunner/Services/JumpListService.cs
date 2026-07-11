using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Shell;
using OneClickRunner.Models;

namespace OneClickRunner.Services;

/// <summary>
/// Builds and applies the taskbar Jump List - the app's launcher UI. One <see cref="JumpTask"/>
/// per scenario (<c>Arguments = /run:{id}</c>) plus Settings and Exit. The scenario list is supplied
/// by a callback so this service stays free of configuration and UI concerns.
/// </summary>
public sealed class JumpListService
{
    private readonly Func<IReadOnlyList<AppItem>> _getItems;

    public JumpListService(Func<IReadOnlyList<AppItem>> getItems)
    {
        _getItems = getItems;
    }

    /// <summary>Rebuild the Jump List from the current scenario list and apply it.</summary>
    public void Rebuild()
    {
        try
        {
            LoggingService.Log("Building jump list");
            var jumpList = new JumpList
            {
                ShowFrequentCategory = false,
                ShowRecentCategory = false,
            };

            var items = _getItems();
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            LoggingService.Log($"Found {items.Count} items, exe path: {exePath}");

            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                LoggingService.Log("Exe path not found or invalid");
                return;
            }

            foreach (var item in items)
            {
                var icon = ScenarioIconResolver.Resolve(item);
                var jumpTask = new JumpTask
                {
                    Title = item.Name,
                    Description = $"Run {item.Name}",
                    ApplicationPath = exePath,
                    Arguments = $"/run:{item.Id}",
                    IconResourcePath = icon.Path,
                    IconResourceIndex = icon.Index,
                };
                jumpList.JumpItems.Add(jumpTask);
                LoggingService.Log($"Added jump task: {item.Name}");
            }

            jumpList.JumpItems.Add(new JumpTask
            {
                Title = "Settings",
                Description = "Open OneClickRunner Settings",
                ApplicationPath = exePath,
                Arguments = "/settings",
            });

            jumpList.JumpItems.Add(new JumpTask
            {
                Title = "Exit",
                Description = "Close OneClickRunner",
                ApplicationPath = exePath,
                Arguments = "/exit",
            });

            JumpList.SetJumpList(System.Windows.Application.Current, jumpList);
            jumpList.Apply();
            LoggingService.Log("Jump list applied successfully");
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Error building jump list: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
