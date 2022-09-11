using Avalonia.Controls;

namespace LASCreator.Views
{
    public partial class Material : UserControl
    {
        public int Key { get; private set; }

        public Material()
        {
            InitializeComponent();
        }

        public Material(int key)
        {
            InitializeComponent();
            Key = key;
            MaterialDiffuse.Filters = MainWindow.textureFilter;
            MaterialNormal.Filters = MainWindow.textureFilter;
            MaterialIllumination.Filters = MainWindow.textureFilter;
        }

        private void Close()
        {
            MainWindow.Instance.Remove(Key);
        }
    }
}
