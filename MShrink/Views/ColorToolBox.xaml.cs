using MShrink.ViewModels;

namespace MShrink.Views
{
    public partial class ColorToolBox
    {
        public ColorToolBox()
        {
            InitializeComponent();
            this.DataContext = App.GetService<ColorToolBoxViewModel>();
        }
    }
}
