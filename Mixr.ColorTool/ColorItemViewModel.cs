using System.Windows.Media;

namespace Mixr.ColorTool
{
    public class ColorItemViewModel
    {
        public ColorItemViewModel() { }

        public string Name { get; set; }
        public string Opacity { get; set; }
        public SolidColorBrush Color { get; set; }
    }
}
