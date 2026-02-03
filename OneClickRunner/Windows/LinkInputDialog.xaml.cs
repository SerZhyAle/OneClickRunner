using System.Windows;

namespace OneClickRunner.Windows
{
    public partial class LinkInputDialog : Window
    {
        public string Link { get; private set; }

        public LinkInputDialog()
        {
            InitializeComponent();
            LinkTextBox.Focus();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LinkTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please enter a link.", "yt-dlp Download", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Link = LinkTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
