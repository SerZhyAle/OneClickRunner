using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OneClickRunner.Models;

namespace OneClickRunner.Services;

/// <summary>
/// Service for managing application configuration and persistence
/// </summary>
public class ConfigurationService
{
    private readonly string _configPath;
    private List<AppItem> _appItems;

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "OneClickRunner");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, "config.json");
        _appItems = new List<AppItem>();
        LoadConfiguration();
    }

    public List<AppItem> GetAllItems()
    {
        return new List<AppItem>(_appItems);
    }

    public void AddItem(AppItem item)
    {
        _appItems.Add(item);
        SaveConfiguration();
    }

    public void UpdateItem(AppItem item)
    {
        var index = _appItems.FindIndex(i => i.Id == item.Id);
        if (index >= 0)
        {
            _appItems[index] = item;
            SaveConfiguration();
        }
    }

    public void RemoveItem(Guid id)
    {
        _appItems.RemoveAll(i => i.Id == id);
        SaveConfiguration();
    }

    private void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                _appItems = JsonSerializer.Deserialize<List<AppItem>>(json) ?? new List<AppItem>();
            }
        }
        catch (Exception ex)
        {
            // Log error or handle it appropriately
            System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
        }
    }

    private void SaveConfiguration()
    {
        try
        {
            var json = JsonSerializer.Serialize(_appItems, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            // Log error or handle it appropriately
            System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }
}
