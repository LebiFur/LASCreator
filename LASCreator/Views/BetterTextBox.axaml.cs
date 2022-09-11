using Avalonia;
using Avalonia.Controls;

namespace LASCreator.Views
{
    public partial class BetterTextBox : UserControl
    {
        public static readonly StyledProperty<string> BetterNameProperty = AvaloniaProperty.Register<BetterTextBox, string>(nameof(BetterName));
        public static readonly StyledProperty<string> BetterWatermarkProperty = AvaloniaProperty.Register<BetterTextBox, string>(nameof(BetterWatermark));

        public string BetterName
        {
            get { return GetValue(BetterNameProperty); }
            set { SetValue(BetterNameProperty, value); }
        }

        public string BetterWatermark
        {
            get { return GetValue(BetterWatermarkProperty); }
            set { SetValue(BetterWatermarkProperty, value); }
        }

        public string Text
        {
            get { return TextBox.Text; }
        }

        public BetterTextBox()
        {
            InitializeComponent();
            Height = 60;
            MinHeight = Height;
            MaxHeight = Height;
        }
    }
}
