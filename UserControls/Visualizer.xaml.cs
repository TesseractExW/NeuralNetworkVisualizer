using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NVV2.UserControls
{
    /// <summary>
    /// Interaction logic for Visualizer.xaml
    /// </summary>
    public partial class Visualizer : UserControl
    {
        public delegate void ClickHandler(int x, int y);
        public List<ClickHandler> LeftClicked = new List<ClickHandler>();
        public List<ClickHandler> RightClicked = new List<ClickHandler>();
        public int dimensionX = 16;
        public int dimensionY = 16;
        private void MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(picture);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                foreach (ClickHandler clickHandler in LeftClicked) clickHandler((int)Math.Ceiling(p.X), (int)Math.Ceiling(p.Y));
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                foreach (ClickHandler clickHandler in RightClicked) clickHandler((int)Math.Ceiling(p.X), (int)Math.Ceiling(p.Y));
            }
        }
        public Visualizer()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(picture, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(picture, EdgeMode.Unspecified);
            bitmap = new WriteableBitmap(dimensionX, dimensionY, 96, 96, PixelFormats.Bgr32, null);
            picture.Source = bitmap;
            picture.MouseMove += new MouseEventHandler(MouseMove);
            picture.MouseLeftButtonDown += new MouseButtonEventHandler(
                (sender, e) => {
                    Point p = e.GetPosition(picture);
                    foreach (ClickHandler clickHandler in LeftClicked) clickHandler((int)Math.Ceiling(p.X), (int)Math.Ceiling(p.Y));
                }
            );
            picture.MouseRightButtonDown += new MouseButtonEventHandler(
                (sender, e) => {
                    Point p = e.GetPosition(picture);
                    foreach (ClickHandler clickHandler in RightClicked) clickHandler((int)Math.Ceiling(p.X), (int)Math.Ceiling(p.Y));
                }
            );
        }
        public WriteableBitmap bitmap;
        public void DrawPixel(int x,int y,int r,int g,int b)
        {
            try
            {
                bitmap.Lock();
                unsafe
                {
                    IntPtr pBackBuffer = bitmap.BackBuffer;
                    pBackBuffer += y * bitmap.BackBufferStride;
                    pBackBuffer += x * 4;

                    int color_data = r << 16;
                    color_data |= g << 8;
                    color_data |= b << 0;
                    *((int*)pBackBuffer) = color_data;
                }
                bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            }
            finally
            {
                bitmap.Unlock();
            }
        }
        public void Update()
        {
            picture.UpdateLayout();
        }
    }
}
