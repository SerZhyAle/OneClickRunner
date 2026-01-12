using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using OneClickRunner.Models;

namespace OneClickRunner.Services;

/// <summary>
/// Service for managing application configuration and persistence
/// </summary>
public class ConfigurationService
{
    private readonly string _scenariosPath;
    private List<AppItem> _appItems;

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "OneClickRunner");
        Directory.CreateDirectory(appFolder);
        _scenariosPath = Path.Combine(appFolder, "Scenarios");
        Directory.CreateDirectory(_scenariosPath);
        _appItems = new List<AppItem>();
        LoadConfiguration();
    }

    public List<AppItem> GetAllItems()
    {
        return new List<AppItem>(_appItems);
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
                System.Diagnostics.Debug.WriteLine($"Error loading {file}: {ex.Message}");
            }
        }

        // Add default calculator scenario if no items exist
        if (_appItems.Count == 0)
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
            System.Diagnostics.Debug.WriteLine($"Error saving {item.Filename}: {ex.Message}");
        }
    }
}
