using Avalonia.Controls;

namespace LASCreator.Views
{
    public partial class PopUp : Window
    {
        public PopUp()
        {
            InitializeComponent();
        }

        public PopUp(string title, string message)
        {
            InitializeComponent();

            CanResize = false;
            Width = 300;
            Height = 100;
            MinWidth = Width;
            MaxWidth = Width;
            MinHeight = Height;
            MaxHeight = Height;

            Title = title;
            MessageText.Text = message;
        }
    }
}
