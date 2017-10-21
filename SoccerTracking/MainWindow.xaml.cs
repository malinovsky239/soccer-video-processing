using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Rectangle = System.Drawing.Rectangle;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace SoccerTracking
{
    public partial class MainWindow : Window
    {
        private enum ResizingMode
        {
            None,
            ToRight,
            ToBottom
        }

        private ResizingMode _resizingMode;
        private System.Windows.Point _lastPosition;
        private const int MarginForAngles = 16;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _resizingMode == ResizingMode.None)
                DragMove();
        }

        private void StartResizeWidth(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
            _lastPosition = e.GetPosition(this);
            if (((Line)sender).Name == "RightBorder")
            {
                _resizingMode = ResizingMode.ToRight;
            }
            if (((Line)sender).Name == "BottomBorder")
            {
                _resizingMode = ResizingMode.ToBottom;
            }
        }

        private void StopResizeWidth(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            _resizingMode = ResizingMode.None;
        }

        private void Window_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_resizingMode != ResizingMode.None)
            {
                var newPosition = e.GetPosition(this);
                switch (_resizingMode)
                {
                    case ResizingMode.ToRight:
                        Width += newPosition.X - _lastPosition.X;
                        break;

                    case ResizingMode.ToBottom:
                        Height += newPosition.Y - _lastPosition.Y;
                        break;
                }
                _lastPosition = newPosition;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                DraggableAnchor.Visibility = Visibility.Collapsed;
                DispatcherTimer dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 999);
                dispatcherTimer.Start();
                TakeScreenshot();
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //TakeScreenshot();
        }

        private void TakeScreenshot()
        {
            var topLeft = PointToScreen(new System.Windows.Point(MarginForAngles, MarginForAngles));
            var bottomRight = PointToScreen(new System.Windows.Point(Width - MarginForAngles, Height - MarginForAngles));
            var area = new Rectangle(new System.Drawing.Point((int)topLeft.X, (int)topLeft.Y), new System.Drawing.Size((int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y));
            var bmpScreenshot = new Bitmap(area.Width, area.Height);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(area.X, area.Y, 0, 0, area.Size, CopyPixelOperation.SourceCopy);

            Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(bmpScreenshot);
            var imageCopy = imageCV.Clone();

            CvInvoke.CvtColor(imageCopy, imageCopy, ColorConversion.Bgr2Gray);
            CvInvoke.Canny(imageCopy, imageCopy, 10, 100);

            var res = CvInvoke.HoughLinesP(imageCopy, 1, 0.01, 10, 100);
            foreach (var line in res)
            {
                CvInvoke.Line(imageCV, line.P1, line.P2, new MCvScalar(0, 255, 0), 2);
            }

            var image = new Image { Source = Helper.EmguCVToBitmapSource(imageCV), Margin = new Thickness(MarginForAngles) };
            Grid.Children.Add(image);
        }
    }
}
