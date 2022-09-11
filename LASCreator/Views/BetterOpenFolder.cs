using Avalonia.Controls;

namespace LASCreator.Views
{
    public partial class BetterOpenFolder : BetterOpen
    {
        public string Path { get; private set; } = string.Empty;

        private readonly OpenFolderDialog openDialog = new();

        public BetterOpenFolder()
        {
            InitializeComponent();
        }

        protected override async void Open()
        {
            string? newPath = await openDialog.ShowAsync(new Window());
            if (newPath == null) return;
            Path = newPath;
            TextBox.Text = Path;
        }
    }
}
