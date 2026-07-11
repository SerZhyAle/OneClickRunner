using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using OneClickRunner.Models;

namespace OneClickRunner.Services;

/// <summary>
/// Service for managing application configuration and persistence
/// </summary>
public class ConfigurationService
{
    private readonly string _scenariosPath;
    private readonly string _seedMarkerPath;
    private List<AppItem> _appItems;

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "OneClickRunner");
        Directory.CreateDirectory(appFolder);
        _scenariosPath = Path.Combine(appFolder, "Scenarios");
        Directory.CreateDirectory(_scenariosPath);
        _seedMarkerPath = Path.Combine(appFolder, ".initialized");
        _appItems = new List<AppItem>();
        LoadConfiguration();
    }

    public List<AppItem> GetAllItems()
    {
        return new List<AppItem>(_appItems);
    }

    public void Reload()
    {
        LoadConfiguration();
    }

    public void AddItem(AppItem item)
    {
        if (string.IsNullOrEmpty(item.Filename))
        {
            item.Filename = $"{item.Id}.xml";
        }
        _appItems.Add(item);
        SaveItem(item);
    }

    public void UpdateItem(AppItem item)
    {
        var index = _appItems.FindIndex(i => i.Id == item.Id);
        if (index >= 0)
        {
            _appItems[index] = item;
            SaveItem(item);
        }
    }

    public void RemoveItem(Guid id)
    {
        var item = _appItems.Find(i => i.Id == id);
        if (item != null)
        {
            File.Delete(Path.Combine(_scenariosPath, item.Filename));
            _appItems.Remove(item);
        }
    }

    private void LoadConfiguration()
    {
        _appItems.Clear();
        var xmlFiles = Directory.GetFiles(_scenariosPath, "*.xml");
        foreach (var file in xmlFiles)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(AppItem));
                using var stream = File.OpenRead(file);
                var item = (AppItem?)serializer.Deserialize(stream);
                if (item != null)
                {
                    item.Filename = Path.GetFileName(file);
                    _appItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log($"Error loading {file}: {ex.Message}");
            }
        }

        // Seed the sample scenario only on the very first run, not every time the folder is
        // empty - so a user who deletes every scenario keeps an empty list.
        if (_appItems.Count == 0 && !File.Exists(_seedMarkerPath))
        {
            var calc = new AppItem
            {
                Name = "Calculator",
                Path = "calc.exe",
                Arguments = "",
                WorkingDirectory = "",
                Filename = "run_windows_calculator.xml"
            };
            _appItems.Add(calc);
            SaveItem(calc);
        }

        MarkInitialized();
    }

    private void MarkInitialized()
    {
        try
        {
            if (!File.Exists(_seedMarkerPath))
            {
                File.WriteAllText(_seedMarkerPath, DateTime.Now.ToString("o"));
            }
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Error writing init marker: {ex.Message}");
        }
    }

    private void SaveItem(AppItem item)
    {
        try
        {
            var path = Path.Combine(_scenariosPath, item.Filename);
            var serializer = new XmlSerializer(typeof(AppItem));
            using var stream = File.Create(path);
            serializer.Serialize(stream, item);
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Error saving {item.Filename}: {ex.Message}");
        }
    }
}
