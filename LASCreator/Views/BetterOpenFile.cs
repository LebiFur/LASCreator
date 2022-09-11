using Avalonia.Controls;
using System.Collections.Generic;

namespace LASCreator.Views
{
    public partial class BetterOpenFile : BetterOpen
    {
        public string Path { get; private set; } = string.Empty;

        private readonly OpenFileDialog openDialog = new();

        public List<FileDialogFilter>? Filters
        {
            get { return openDialog.Filters; }
            set { openDialog.Filters = value; }
        }

        public BetterOpenFile()
        {
            InitializeComponent();
        }

        protected override async void Open()
        {
            string[]? files = await openDialog.ShowAsync(new Window());
            if (files == null || files.Length == 0) return;
            Path = files[0];
            TextBox.Text = Path;
        }
    }
}
