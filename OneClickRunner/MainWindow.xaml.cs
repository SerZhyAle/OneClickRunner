using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using OneClickRunner.Models;
using OneClickRunner.Services;
using OneClickRunner.Windows;

namespace OneClickRunner;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ConfigurationService _configService;
    private readonly AutostartService _autostartService;
    private bool _isExiting = false;

    public MainWindow()
    {
        InitializeComponent();
        _configService = new ConfigurationService();
        _autostartService = new AutostartService();
        
        LoadAppItems();
        AutostartCheckBox.IsChecked = _autostartService.IsAutostartEnabled();

        LoggingService.Log("MainWindow constructor completed");
    }

    private void LoadAppItems()
    {
        AppListView.ItemsSource = _configService.GetAllItems();
        
        // Refresh jump list when items change
        if (System.Windows.Application.Current is App app)
        {
            app.RefreshJumpList();
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Add button clicked");
        var dialog = new AppItemDialog();
        if (dialog.ShowDialog() == true && dialog.AppItem != null)
        {
            _configService.AddItem(dialog.AppItem);
            LoadAppItems();
        }
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Import button clicked");
        
        // Open dialog in Desktop or Documents folder, NOT in Scenarios folder
        string initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        
        // If Desktop is empty, try Documents
        if (!Directory.Exists(initialDirectory) || Directory.GetFiles(initialDirectory, "*.xml").Length == 0)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (Directory.Exists(documentsPath))
            {
                initialDirectory = documentsPath;
            }
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            Title = "Select XML scenario file to import",
            InitialDirectory = initialDirectory
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Models.AppItem));
                using var stream = File.OpenRead(dialog.FileName);
                var importedItem = (Models.AppItem?)serializer.Deserialize(stream);
                if (importedItem != null)
                {
                    // Generate new ID to avoid conflicts
                    importedItem.Id = Guid.NewGuid();
                    // Set filename for saving
                    importedItem.Filename = $"{importedItem.Id}.xml";
                    
                    _configService.AddItem(importedItem);
                    LoadAppItems();
                    
                    LoggingService.Log($"Imported item: {importedItem.Name}");
                    System.Windows.MessageBox.Show($"Successfully imported '{importedItem.Name}'", "Import Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LoggingService.Log("Import failed: Invalid XML format");
                    System.Windows.MessageBox.Show("Failed to import: Invalid XML format", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log($"Import error: {ex.Message}");
                System.Windows.MessageBox.Show($"Failed to import: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RunButton_Click(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Run button clicked");
        if (AppListView.SelectedItem is AppItem selectedItem)
        {
            LoggingService.Log($"Running item: {selectedItem.Name}");
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = selectedItem.Path,
                    Arguments = selectedItem.Arguments,
                    UseShellExecute = true
                };

                if (selectedItem.RunAsAdmin)
                {
                    startInfo.Verb = "runas";
                }

                if (!string.IsNullOrWhiteSpace(selectedItem.WorkingDirectory))
                {
                    startInfo.WorkingDirectory = selectedItem.WorkingDirectory;
                }
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                LoggingService.Log($"Run error: {ex.Message}");
                System.Windows.MessageBox.Show($"Failed to run '{selectedItem.Name}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            LoggingService.Log("Run failed: No item selected");
            System.Windows.MessageBox.Show("Please select an item to run.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Edit button clicked");
        if (AppListView.SelectedItem is AppItem selectedItem)
        {
            LoggingService.Log($"Editing item: {selectedItem.Name}");
            var itemToEdit = new AppItem
            {
                Id = selectedItem.Id,
                Name = selectedItem.Name,
                Path = selectedItem.Path,
                Arguments = selectedItem.Arguments,
                WorkingDirectory = selectedItem.WorkingDirectory,
                RunAsAdmin = selectedItem.RunAsAdmin
            };

            var dialog = new AppItemDialog(itemToEdit);
            if (dialog.ShowDialog() == true && dialog.AppItem != null)
            {
                _configService.UpdateItem(dialog.AppItem);
                LoadAppItems();
            }
        }
        else
        {
            LoggingService.Log("Edit failed: No item selected");
            System.Windows.MessageBox.Show("Please select an item to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Remove button clicked");
        if (AppListView.SelectedItem is AppItem selectedItem)
        {
            LoggingService.Log($"Removing item: {selectedItem.Name}");
            var result = System.Windows.MessageBox.Show($"Are you sure you want to remove '{selectedItem.Name}'?", 
                "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _configService.RemoveItem(selectedItem.Id);
                LoadAppItems();
            }
            else
            {
                LoggingService.Log("Remove cancelled by user");
            }
        }
        else
        {
            LoggingService.Log("Remove failed: No item selected");
            System.Windows.MessageBox.Show("Please select an item to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void AutostartCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Autostart checkbox changed");
        try
        {
            if (AutostartCheckBox.IsChecked == true)
            {
                LoggingService.Log("Enabling autostart");
                _autostartService.EnableAutostart();
            }
            else
            {
                LoggingService.Log("Disabling autostart");
                _autostartService.DisableAutostart();
            }
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Autostart error: {ex.Message}");
            System.Windows.MessageBox.Show($"Failed to update autostart setting: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            AutostartCheckBox.IsChecked = _autostartService.IsAutostartEnabled();
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LoggingService.Log("=== EXIT BUTTON CLICKED START ===");
            LoggingService.Log($"Sender: {sender?.GetType().Name}");
            LoggingService.Log($"RoutedEventArgs: {e?.GetType().Name}");
            LoggingService.Log($"Current thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            LoggingService.Log($"Application.Current is null: {System.Windows.Application.Current == null}");
            LoggingService.Log($"_isExiting before: {_isExiting}");
            
            var result = System.Windows.MessageBox.Show("Are you sure you want to exit OneClickRunner?", 
                "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            LoggingService.Log($"MessageBox result: {result}");
            
            if (result == MessageBoxResult.Yes)
            {
                LoggingService.Log("User confirmed exit - setting _isExiting = true");
                _isExiting = true;
                LoggingService.Log($"_isExiting after: {_isExiting}");
                
                LoggingService.Log("About to call Application.Current.Shutdown()");
                System.Windows.Application.Current.Shutdown();
                LoggingService.Log("Shutdown() called successfully");
            }
            else
            {
                LoggingService.Log("User cancelled exit");
            }
            
            LoggingService.Log("=== EXIT BUTTON CLICKED END ===");
        }
        catch (Exception ex)
        {
            LoggingService.Log($"EXCEPTION in ExitButton_Click: {ex.Message}");
            LoggingService.Log($"StackTrace: {ex.StackTrace}");
            System.Windows.MessageBox.Show($"Error during exit: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyAndHideButton_Click(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Apply and Hide button clicked");
        Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        LoggingService.Log($"Window_Closing event - _isExiting: {_isExiting}");
        
        if (_isExiting)
        {
            LoggingService.Log("Application is exiting - allowing window to close");
            // Allow window to close
            e.Cancel = false;
        }
        else
        {
            LoggingService.Log("Settings window closing - minimizing");
            // Minimize window instead of closing
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }
    }
}