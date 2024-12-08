using Microsoft.Win32;
using NVV2.UserControls;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NVV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int n = 32 * 32;
        float[] input_values = new float[n];
        float[] weights = new float[n * 4];
        float[] biases = { 0, 0, 0, 0 };
        float[] velocities = new float[n * 4];
        public MainWindow()
        {
            InitializeComponent();
            Random rnd = new Random();
            Visualizer[] arr = [ node_0, node_1, node_2, node_3 ];
            int _i = 0;
            foreach (Visualizer control in arr)
            {
                for (int i = n * _i; i < n * (_i + 1); i++)
                {
                    weights[i] = 0;
                }
                control.DrawFromArray(weights[(n * _i)..(n * (_i + 1))], biases[_i]);
                _i++;
            }
            inputNode.ClearScreen();
            node_0.DrawFromArray(weights[0..n], biases[0]);
            node_1.DrawFromArray(weights[n..(2*n)], biases[1]);
            node_2.DrawFromArray(weights[(2*n)..(3*n)], biases[2]);
            node_3.DrawFromArray(weights[(3*n)..(4*n)], biases[3]);
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
                inputNode.DrawFromArray(input_values, 0);
            });
            inputNode.RightClicked.Add((int x, int y) =>
            {
                for (int i = 0; i < input_values.Length; i++)
                {
                    input_values[i] = 0;
                }
                inputNode.DrawFromArray(input_values, 0);
            });
            void Prediction(float[] prediction)
            {
                for (int _i = 0; _i < 4; _i++)
                {
                    for (int i = n * _i; i < n * (_i + 1); i++)
                    {
                        prediction[_i] += weights[i] * input_values[i - n * _i];
                    }
                    prediction[_i] += biases[_i];
                    prediction[_i] = 1.0f / (MathF.Exp(-prediction[_i]) + 1);
                }
            }
            apply_input.Click += (s,e)=>
            {
                float[] prediction = { 0, 0, 0, 0 };
                Prediction(prediction);
                Debug.WriteLine(string.Join(", ", prediction));
                predictor.Content = "Prediction : #" + Array.IndexOf(prediction, prediction.Max());
            };
            open_button.Click += (s, e) =>
            {
                float scale_ = 10;
                try
                {
                    scale_ = Convert.ToSingle(scale.Text);
                }
                catch (Exception)
                {

                }
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Weights data file (*.wdf)|*.wdf";
                openFileDialog.FilterIndex = 1;
                if (openFileDialog.ShowDialog() == true)
                {
                    string[] strings = File.ReadAllText(openFileDialog.FileName).Split(", ");
                    for (int i = 0; i < weights.Length; i++)
                    {
                        weights[i] = float.Parse(strings[i]);
                    }
                    for (int i = weights.Length; i < strings.Length; i++)
                    {
                        biases[i - weights.Length] = float.Parse(strings[i]);
                    }
                    node_0.DrawFromArray(weights[0..n], scale_, biases[0]);
                    node_1.DrawFromArray(weights[n..(2 * n)], scale_, biases[1]);
                    node_2.DrawFromArray(weights[(2 * n)..(3 * n)], scale_, biases[2]);
                    node_3.DrawFromArray(weights[(3 * n)..(4 * n)], scale_, biases[3]);

                    TextNode0.Content = "Node #0 bias : " + biases[0].ToString();
                    TextNode1.Content = "Node #1 bias : " + biases[1].ToString();
                    TextNode2.Content = "Node #2 bias : " + biases[2].ToString();
                    TextNode3.Content = "Node #3 bias : " + biases[3].ToString();
                }
            };
            save_button.Click += (s, e) =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Weights data file (*.wdf)|*.wdf";
                saveFileDialog.DefaultExt = "wdf";
                saveFileDialog.AddExtension = true;
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllText(saveFileDialog.FileName, string.Join(", ", weights)+", "+ string.Join(", ", biases));
            };
            optimize_button.Click += (s, e) =>
            {
                int _n = 0;
                float lr = 0.1f;
                float scale_ = 10;
                try
                {
                    _n = Convert.ToInt32(actual_answer.Text);
                    lr = Convert.ToSingle(learning_rate.Text);
                    scale_ = Convert.ToSingle(scale.Text);
                }
                catch (Exception)
                {
                    return;
                }
                if (_n < 0 || _n >= 4)
                    return;
                float[] expectation = [ 0, 0, 0, 0 ];
                expectation[_n] = 1;
                float[] prediction = [ 0, 0, 0, 0 ];
                Prediction(prediction);
                float[] new_weights = new float[weights.Length];
                float[] new_biases = new float[biases.Length];
                for (int i = 0;i < biases.Length; i++)
                {
                    new_biases[i] = biases[i] - lr * 1 / biases.Length * (prediction[i] - expectation[i]);
                }
                
                for (int j = 0; j < biases.Length; j++)
                {
                    for (int i = n*j; i < (j+1)*n; i++)
                    {
                        new_weights[i] += weights[i] - lr * 1 / biases.Length * (prediction[j] - expectation[j]) * input_values[i % n];
                    }
                }
                Debug.WriteLine(string.Join(", ", prediction));
                new_weights.CopyTo(weights, 0);
                new_biases.CopyTo(biases, 0);
                node_0.DrawFromArray(weights[0..n], scale_, biases[0]);
                node_1.DrawFromArray(weights[n..(2 * n)], scale_, biases[1]);
                node_2.DrawFromArray(weights[(2 * n)..(3 * n)], scale_, biases[2]);
                node_3.DrawFromArray(weights[(3 * n)..(4 * n)], scale_, biases[3]);

                TextNode0.Content = "Node #0 bias : " + biases[0].ToString();
                TextNode1.Content = "Node #1 bias : " + biases[1].ToString();
                TextNode2.Content = "Node #2 bias : " + biases[2].ToString();
                TextNode3.Content = "Node #3 bias : " + biases[3].ToString();
            };
            /*CompositionTarget.Rendering += (e, o) =>
            {

            };*/
        }

        private void learning_rate_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void scale_TextChanged(object sender, TextChangedEventArgs e)
        {
            float scale_ = 10;
            try
            {
                scale_ = Convert.ToSingle(scale.Text);
                node_0.DrawFromArray(weights[0..n], scale_, biases[0]);
                node_1.DrawFromArray(weights[n..(2 * n)], scale_, biases[1]);
                node_2.DrawFromArray(weights[(2 * n)..(3 * n)], scale_, biases[2]);
                node_3.DrawFromArray(weights[(3 * n)..(4 * n)], scale_, biases[3]);
            }
            catch (Exception)
            {

            }


        }
    }
}