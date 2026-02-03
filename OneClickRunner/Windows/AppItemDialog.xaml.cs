using System.IO;
using System.Windows;
using OneClickRunner.Models;

namespace OneClickRunner.Windows;

public partial class AppItemDialog : Window
{
    public AppItem? AppItem { get; private set; }

    public AppItemDialog(AppItem? existingItem = null)
    {
        InitializeComponent();
        
        if (existingItem != null)
        {
            AppItem = existingItem;
            NameTextBox.Text = existingItem.Name;
            PathTextBox.Text = existingItem.Path;
            ArgumentsTextBox.Text = existingItem.Arguments;
            WorkingDirectoryTextBox.Text = existingItem.WorkingDirectory;
            RunAsAdminCheckBox.IsChecked = existingItem.RunAsAdmin;
            Title = "Edit Application";
        }
        else
        {
            AppItem = new AppItem();
            Title = "Add Application";
        }
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var scenariosPath = Path.Combine(appDataPath, "OneClickRunner", "Scenarios");

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Applications and Scripts (*.exe;*.bat;*.cmd;*.ps1;*.py;*.vbs;*.sh)|*.exe;*.bat;*.cmd;*.ps1;*.py;*.vbs;*.sh|Executable Files (*.exe)|*.exe|Batch Files (*.bat;*.cmd)|*.bat;*.cmd|PowerShell Scripts (*.ps1)|*.ps1|Python Scripts (*.py)|*.py|VBScript Files (*.vbs)|*.vbs|Shell Scripts (*.sh)|*.sh|All Files (*.*)|*.*",
            Title = "Select Application or Script",
            InitialDirectory = scenariosPath
        };

        if (dialog.ShowDialog() == true)
        {
            PathTextBox.Text = dialog.FileName;
        }
    }

    private void BrowseWorkingDirButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Working Directory"
        };

        if (dialog.ShowDialog() == true)
        {
            WorkingDirectoryTextBox.Text = dialog.FolderName;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter a name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(PathTextBox.Text))
        {
            System.Windows.MessageBox.Show("Please select an application or script path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (AppItem != null)
        {
            AppItem.Name = NameTextBox.Text.Trim();
            AppItem.Path = PathTextBox.Text.Trim();
            AppItem.Arguments = ArgumentsTextBox.Text.Trim();
            AppItem.WorkingDirectory = WorkingDirectoryTextBox.Text.Trim();
            AppItem.RunAsAdmin = RunAsAdminCheckBox.IsChecked ?? false;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
