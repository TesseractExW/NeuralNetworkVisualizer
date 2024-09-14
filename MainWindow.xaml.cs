using NVV2.UserControls;
using System.Diagnostics;
using System.Windows;

namespace NVV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Random rnd = new Random();
            float[] input_values = new float[16 * 16];
            inputNode.LeftClicked.Add((int x, int y) =>
            {
                int r_x = (int)(x / inputNode.ActualWidth * inputNode.dimensionX);
                int r_y = (int)(y / inputNode.ActualHeight * inputNode.dimensionY);
                if (r_x < inputNode.dimensionX && r_y < inputNode.dimensionY)
                {
                    inputNode.DrawPixel(r_x, r_y, 255, 255, 255);
                    input_values[r_y * inputNode.dimensionX + r_x] = 1.0f;
                    Debug.WriteLine("Set pixel at (" + r_x.ToString() + " , " + r_y.ToString() + ") : " + input_values[r_y * inputNode.dimensionX + r_x].ToString());
                }
            });
            inputNode.RightClicked.Add((int x, int y) =>
            {
                int r_x = (int)(x / inputNode.ActualWidth * inputNode.dimensionX);
                int r_y = (int)(y / inputNode.ActualHeight * inputNode.dimensionY);
                if (r_x < inputNode.dimensionX && r_y < inputNode.dimensionY)
                {
                    inputNode.DrawPixel(r_x, r_y, 0, 0, 0);
                    input_values[r_y * inputNode.dimensionX + r_x] = 0.0f;
                    Debug.WriteLine("Delete pixel at (" + r_x.ToString() + " , " + r_y.ToString() + ") : " + input_values[r_y * inputNode.dimensionX + r_x].ToString());
                }
            });
            Visualizer[] arr = { node_0, node_1, node_2, node_3 };
            foreach (Visualizer control in arr)
            {
                for (int i = 0; i < inputNode.dimensionX; i++)
                {
                    for (int j = 0; j < inputNode.dimensionY; j++)
                    {
                        int val = (int)(255 * rnd.NextDouble());
                        control.DrawPixel(i, j, val, val, val);
                    }
                }
            }
            /*CompositionTarget.Rendering += (e, o) =>
            {

            };*/
        }
    }
}