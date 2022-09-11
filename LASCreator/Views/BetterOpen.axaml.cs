using Avalonia;
using Avalonia.Controls;

namespace LASCreator.Views
{
    public partial class BetterOpen : UserControl
    {
        public static readonly StyledProperty<string> BetterNameProperty = AvaloniaProperty.Register<BetterOpen, string>(nameof(BetterName));

        public string BetterName
        {
            get { return GetValue(BetterNameProperty); }
            set { SetValue(BetterNameProperty, value); }
        }

        public BetterOpen()
        {
            InitializeComponent();
            Height = 60;
            MinHeight = Height;
            MaxHeight = Height;
        }

        protected virtual void Open() { }

        protected void Clear()
        {
            TextBox.Clear();
        }
    }
}
