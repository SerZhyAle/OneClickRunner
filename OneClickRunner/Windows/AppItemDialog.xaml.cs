using System.IO;
using System.Windows;
using System.Windows.Controls;
using OneClickRunner.Models;

namespace OneClickRunner.Windows;

public partial class AppItemDialog : Window
{
    private const int TypeIndexExecutable = 0;
    private const int TypeIndexYtDlp = 1;

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
            YtDlpOutputFolderTextBox.Text = existingItem.YtDlpOutputFolder;
            YtDlpFormatTextBox.Text = existingItem.YtDlpFormat;
            TypeComboBox.SelectedIndex = existingItem.Type == ScenarioType.YtDlp ? TypeIndexYtDlp : TypeIndexExecutable;
            Title = "Edit Application";
        }
        else
        {
            AppItem = new AppItem();
            TypeComboBox.SelectedIndex = TypeIndexExecutable;
            Title = "Add Application";
        }

        UpdatePanelVisibility();
    }

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePanelVisibility();

    private void UpdatePanelVisibility()
    {
        // Named fields may not exist yet during the first SelectionChanged in InitializeComponent.
        if (ExecutablePanel == null || YtDlpPanel == null)
        {
            return;
        }
        var isYtDlp = TypeComboBox.SelectedIndex == TypeIndexYtDlp;
        ExecutablePanel.Visibility = isYtDlp ? Visibility.Collapsed : Visibility.Visible;
        YtDlpPanel.Visibility = isYtDlp ? Visibility.Visible : Visibility.Collapsed;
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

    private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Download Folder"
        };

        if (dialog.ShowDialog() == true)
        {
            YtDlpOutputFolderTextBox.Text = dialog.FolderName;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter a name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (AppItem == null)
        {
            return;
        }

        var isYtDlp = TypeComboBox.SelectedIndex == TypeIndexYtDlp;
        AppItem.Name = NameTextBox.Text.Trim();

        if (isYtDlp)
        {
            AppItem.Type = ScenarioType.YtDlp;
            // Path is not used to launch a yt-dlp scenario; a friendly label keeps the list readable.
            AppItem.Path = "yt-dlp";
            AppItem.Arguments = string.Empty;
            AppItem.WorkingDirectory = string.Empty;
            AppItem.RunAsAdmin = false;
            AppItem.YtDlpOutputFolder = YtDlpOutputFolderTextBox.Text.Trim();
            AppItem.YtDlpFormat = YtDlpFormatTextBox.Text.Trim();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please select an application or script path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AppItem.Type = ScenarioType.Executable;
            AppItem.Path = PathTextBox.Text.Trim();
            AppItem.Arguments = ArgumentsTextBox.Text.Trim();
            AppItem.WorkingDirectory = WorkingDirectoryTextBox.Text.Trim();
            AppItem.RunAsAdmin = RunAsAdminCheckBox.IsChecked ?? false;
            AppItem.YtDlpOutputFolder = string.Empty;
            AppItem.YtDlpFormat = string.Empty;
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
