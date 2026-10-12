using MShrink.ViewModels;

namespace MShrink.Views
{
    public partial class ThemeSettingsBox
    {
        public ThemeSettingsBox()
        {
            InitializeComponent();
            DataContext = App.GetService<ThemeSettingsViewModel>();
        }
    }
}
