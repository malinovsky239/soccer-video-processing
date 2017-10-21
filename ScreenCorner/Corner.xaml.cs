using System.Windows;
using System.Windows.Controls;

namespace ScreenCorner
{
    public partial class Corner
    {
        public Corner()
        {
            InitializeComponent();
        }

        public int Rotation
        {
            get { return (int)GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); }
        }

        public static readonly DependencyProperty RotationProperty
            = DependencyProperty.Register(
                  "Rotation",
                  typeof(int),
                  typeof(Corner),
                  new FrameworkPropertyMetadata(0)
              );
    }
}
