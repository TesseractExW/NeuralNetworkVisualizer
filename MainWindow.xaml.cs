using NVV2.UserControls;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Input;

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
            int n = 32 * 32;
            float[] input_values = new float[n];
            float[] weights = new float[n * 4];

            Visualizer[] arr = [ node_0, node_1, node_2, node_3 ];
            int _i = 0;
            foreach (Visualizer control in arr)
            {
                for (int i = n * _i; i < n * (_i + 1); i++)
                {
                    weights[i] = 0;
                }
                control.DrawFromArray(weights[(n * _i)..(n * (_i + 1))]);
                _i++;
            }
            inputNode.ClearScreen();
            node_0.DrawFromArray(weights[0..n],10);
            node_1.DrawFromArray(weights[n..(2*n)],10);
            node_2.DrawFromArray(weights[(2*n)..(3*n)],10);
            node_3.DrawFromArray(weights[(3*n)..(4*n)],10);
            inputNode.LeftClicked.Add((int x, int y) =>
            {
                Action<int, int,float> sub_func = (_x, _y, d) =>
                {
                    if (_x >= 0 && _y >= 0 && _x < inputNode.dimensionX && _y < inputNode.dimensionY)
                    {
                        int val = (int)(d * 255);
                        inputNode.DrawPixel(_x, _y, val, val, val);
                        input_values[_y * inputNode.dimensionX + _x] = d;
                    }
                };
                int r_x = (int)(x / inputNode.ActualWidth * inputNode.dimensionX);
                int r_y = (int)(y / inputNode.ActualHeight * inputNode.dimensionY);
                sub_func(r_x, r_y, 1);
                sub_func(r_x + 1, r_y + 1, 1f);
                sub_func(r_x + 1, r_y - 1, 1f);
                sub_func(r_x - 1, r_y + 1, 1f);
                sub_func(r_x - 1, r_y - 1, 1f);

                sub_func(r_x, r_y + 1, 1f);
                sub_func(r_x, r_y - 1, 1f);
                sub_func(r_x+1, r_y, 1f);
                sub_func(r_x-1, r_y, 1f);
                inputNode.DrawFromArray(input_values);
            });
            inputNode.RightClicked.Add((int x, int y) =>
            {
                for (int i = 0; i < input_values.Length; i++)
                {
                    input_values[i] = 0;
                }
                inputNode.DrawFromArray(input_values);
            });

            apply_input.Click += (s,e)=>
            {
                float[] prediction = [ 0, 0, 0, 0 ];
                for (int _i = 0; _i < 4; _i++)
                {
                    for (int i = n * _i; i < n * (_i + 1); i++)
                    {
                        prediction[_i] += weights[i] * input_values[i - n * _i];
                    }
                    prediction[_i] = 1.0f / (MathF.Exp(-prediction[_i]) + 1);
                }
                Debug.WriteLine(string.Join(", ", prediction));
                predictor.Content = "Prediction : #" + Array.IndexOf(prediction, prediction.Max());
            };
            optimize_button.Click += (s, e) =>
            {
                int _n = 0;
                try
                {
                    _n = Convert.ToInt32(actual_answer.Text);
                }
                catch (Exception ex)
                {
                    return;
                }
                if (_n < 0 || _n >= 4)
                    return;
                float[] expectation = [ 0, 0, 0, 0 ];
                expectation[_n] = 1;
                float[] prediction = [ 0, 0, 0, 0 ];
                float[] new_weights = new float[weights.Length];
                float def_cost = 0;
                // default cost
                for (int _i = 0; _i < 4; _i++)
                {
                    for (int i = n * _i; i < n * (_i + 1); i++)
                    {
                        prediction[_i] += weights[i] * input_values[i - n * _i];
                    }
                    prediction[_i] = 1.0f / (MathF.Exp(-prediction[_i]) + 1);
                }
                for (int i = 0 ; i < 4; i++)
                {
                    def_cost += MathF.Pow(prediction[i] - expectation[i], 2);
                }
                // new cost
                float h = 0.0001f;
                for (int w = 0; w < weights.Length; w++)
                {
                    weights[w] += h;
                    float cost = 0;
                    prediction = [0, 0, 0, 0];
                    for (int _i = 0; _i < 4; _i++)
                    {
                        for (int i = n * _i; i < n * (_i + 1); i++)
                        {
                            prediction[_i] += weights[i] * input_values[i - n * _i];
                        }
                        prediction[_i] = 1.0f / (MathF.Exp(-prediction[_i]) + 1);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        cost += MathF.Pow(prediction[i] - expectation[i], 2);
                    }
                    weights[w] -= h;
                    new_weights[w] = weights[w] - 0.1f*(cost - def_cost) / h;
                }
                Debug.WriteLine(string.Join(", ", prediction));
                new_weights.CopyTo(weights, 0);
                node_0.DrawFromArray(weights[0..n],10);
                node_1.DrawFromArray(weights[n..(2 * n)],10);
                node_2.DrawFromArray(weights[(2 * n)..(3 * n)],10);
                node_3.DrawFromArray(weights[(3 * n)..(4 * n)],10);
            };
            /*CompositionTarget.Rendering += (e, o) =>
            {

            };*/
        }
    }
}