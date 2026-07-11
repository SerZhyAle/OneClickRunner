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
    private string _searchText = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
        _configService = new ConfigurationService();
        _autostartService = new AutostartService();
        
        VersionText.Text = VersionService.DisplayVersion;
        Title = $"OneClickRunner Settings — {VersionService.DisplayVersion}";

        LoadAppItems();
        AutostartCheckBox.IsChecked = _autostartService.IsAutostartEnabled();

        LoggingService.Log("MainWindow constructor completed");
    }

    private void LoadAppItems()
    {
        AppListView.ItemsSource = _configService.GetAllItems();

        // The default view is per-collection-instance; ItemsSource is replaced on every reload,
        // so re-apply the live search filter each time.
        var view = System.Windows.Data.CollectionViewSource.GetDefaultView(AppListView.ItemsSource);
        if (view != null)
        {
            view.Filter = FilterScenario;
        }

        // Refresh jump list when items change
        if (System.Windows.Application.Current is App app)
        {
            app.RefreshJumpList();
        }
    }

    private bool FilterScenario(object obj)
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            return true;
        }
        if (obj is not AppItem item)
        {
            return false;
        }
        return item.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
            || item.Path.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
    }

    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // May fire during InitializeComponent before the list exists.
        if (AppListView?.ItemsSource == null)
        {
            return;
        }
        _searchText = SearchBox.Text ?? string.Empty;
        System.Windows.Data.CollectionViewSource.GetDefaultView(AppListView.ItemsSource)?.Refresh();
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
            LaunchItem(selectedItem);
        }
        else
        {
            LoggingService.Log("Run failed: No item selected");
            System.Windows.MessageBox.Show("Please select an item to run.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void AppListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Run only when an actual row was double-clicked, not the header or empty space.
        var source = e.OriginalSource as System.Windows.DependencyObject;
        while (source != null && source is not System.Windows.Controls.ListViewItem)
        {
            source = System.Windows.Media.VisualTreeHelper.GetParent(source);
        }

        if (source is System.Windows.Controls.ListViewItem && AppListView.SelectedItem is AppItem item)
        {
            LoggingService.Log("Item double-clicked");
            LaunchItem(item);
        }
    }

    private void AppListView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (AppListView.SelectedItem is not AppItem)
        {
            return;
        }
        switch (e.Key)
        {
            case System.Windows.Input.Key.Enter:
                RunButton_Click(sender, e);
                e.Handled = true;
                break;
            case System.Windows.Input.Key.F2:
                EditButton_Click(sender, e);
                e.Handled = true;
                break;
            case System.Windows.Input.Key.Delete:
                RemoveButton_Click(sender, e);
                e.Handled = true;
                break;
        }
    }

    private void ListViewItem_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.ListViewItem row)
        {
            row.IsSelected = true;
        }
    }

    private void CloneButton_Click(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Clone button clicked");
        if (AppListView.SelectedItem is AppItem selectedItem)
        {
            // Copy every field (Type/Order/yt-dlp options included), then make it a distinct new item.
            var clone = selectedItem.Clone();
            clone.Id = Guid.NewGuid();
            clone.Filename = string.Empty; // AddItem assigns a fresh filename and appends the order
            clone.Name = $"{selectedItem.Name} (copy)";

            _configService.AddItem(clone);
            LoadAppItems();
            LoggingService.Log($"Cloned '{selectedItem.Name}' -> '{clone.Name}'");
        }
        else
        {
            LoggingService.Log("Clone failed: No item selected");
            System.Windows.MessageBox.Show("Please select an item to clone.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void LaunchItem(AppItem item)
    {
        LoggingService.Log($"Running item: {item.Name}");
        var result = ScenarioLauncher.Launch(item, PromptForYtDlpLink);
        if (!result.Success && !result.Cancelled)
        {
            System.Windows.MessageBox.Show($"Failed to run '{item.Name}': {result.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>Shows the link prompt for a yt-dlp scenario; returns null when cancelled.</summary>
    private string? PromptForYtDlpLink()
    {
        var dialog = new LinkInputDialog { Owner = this };
        return dialog.ShowDialog() == true ? dialog.Link : null;
    }

    private void MoveUpButton_Click(object sender, RoutedEventArgs e) => MoveSelected(-1);

    private void MoveDownButton_Click(object sender, RoutedEventArgs e) => MoveSelected(+1);

    private void MoveSelected(int direction)
    {
        if (AppListView.SelectedItem is AppItem selectedItem)
        {
            var id = selectedItem.Id;
            _configService.MoveItem(id, direction);
            LoadAppItems();
            ReselectById(id);
            LoggingService.Log($"Moved '{selectedItem.Name}' ({(direction < 0 ? "up" : "down")})");
        }
        else
        {
            System.Windows.MessageBox.Show("Please select an item to move.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// <summary>Restore selection after LoadAppItems replaces the item source with fresh objects.</summary>
    private void ReselectById(Guid id)
    {
        foreach (var obj in AppListView.Items)
        {
            if (obj is AppItem item && item.Id == id)
            {
                AppListView.SelectedItem = obj;
                AppListView.ScrollIntoView(obj);
                break;
            }
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Export button clicked");
        if (AppListView.SelectedItem is not AppItem selectedItem)
        {
            System.Windows.MessageBox.Show("Please select an item to export.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var suggestedName = SanitizeFileName(selectedItem.Name);
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            Title = "Export scenario to XML",
            FileName = string.IsNullOrWhiteSpace(suggestedName) ? "scenario.xml" : suggestedName + ".xml",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(AppItem));
            using var stream = File.Create(dialog.FileName);
            serializer.Serialize(stream, selectedItem);
            LoggingService.Log($"Exported '{selectedItem.Name}' -> {dialog.FileName}");
            System.Windows.MessageBox.Show($"Exported '{selectedItem.Name}'.", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Export error: {ex.Message}");
            System.Windows.MessageBox.Show($"Failed to export: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name.Trim();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        LoggingService.Log("Edit button clicked");
        if (AppListView.SelectedItem is AppItem selectedItem)
        {
            LoggingService.Log($"Editing item: {selectedItem.Name}");
            // Edit a full copy (all fields, incl. Type/Order/yt-dlp) so a cancel leaves the original
            // untouched and an OK preserves fields the dialog does not surface.
            var itemToEdit = selectedItem.Clone();

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

    private void ExitButton_Click(object sender, RoutedEventArgs e) => RequestExit();

    /// <summary>
    /// The single confirmed-exit path: confirm, set the exit flag so <see cref="Window_Closing"/>
    /// allows the window to close, then shut the application down. Called by the Exit button and the
    /// tray menu's Exit item.
    /// </summary>
    public void RequestExit()
    {
        try
        {
            LoggingService.Log($"RequestExit invoked (_isExiting={_isExiting})");
            var result = System.Windows.MessageBox.Show("Are you sure you want to exit OneClickRunner?",
                "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoggingService.Log("User confirmed exit - shutting down");
                _isExiting = true;
                System.Windows.Application.Current?.Shutdown();
            }
            else
            {
                LoggingService.Log("User cancelled exit");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Log($"EXCEPTION in RequestExit: {ex.Message}\n{ex.StackTrace}");
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