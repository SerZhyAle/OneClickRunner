using System.Diagnostics;
using System.Windows;
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

    public MainWindow()
    {
        InitializeComponent();
        _configService = new ConfigurationService();
        _autostartService = new AutostartService();
        
        LoadAppItems();
        AutostartCheckBox.IsChecked = _autostartService.IsAutostartEnabled();
    }

    private void LoadAppItems()
    {
        AppListView.ItemsSource = _configService.GetAllItems();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AppItemDialog();
        if (dialog.ShowDialog() == true && dialog.AppItem != null)
        {
            _configService.AddItem(dialog.AppItem);
            LoadAppItems();
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (AppListView.SelectedItem is AppItem selectedItem)
        {
            var itemToEdit = new AppItem
            {
                Id = selectedItem.Id,
                Name = selectedItem.Name,
                Path = selectedItem.Path,
                Arguments = selectedItem.Arguments,
                WorkingDirectory = selectedItem.WorkingDirectory
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
            System.Windows.MessageBox.Show("Please select an item to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (AppListView.SelectedItem is AppItem selectedItem)
        {
            var result = System.Windows.MessageBox.Show($"Are you sure you want to remove '{selectedItem.Name}'?", 
                "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _configService.RemoveItem(selectedItem.Id);
                LoadAppItems();
            }
        }
        else
        {
            System.Windows.MessageBox.Show("Please select an item to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void AutostartCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AutostartCheckBox.IsChecked == true)
            {
                _autostartService.EnableAutostart();
            }
            else
            {
                _autostartService.DisableAutostart();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to update autostart setting: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            AutostartCheckBox.IsChecked = _autostartService.IsAutostartEnabled();
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Hide window instead of closing
        e.Cancel = true;
        Hide();
    }
}