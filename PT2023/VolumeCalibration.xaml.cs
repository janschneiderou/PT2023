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

namespace PT2023
{
    /// <summary>
    /// Interaction logic for VolumeCalibration.xaml
    /// </summary>
    public partial class VolumeCalibration : UserControl
    {

       

        Line loud;
        Line soft;
        Line speaking;
        int volumeCounter = 0;
        double[] lineHeighs = new double[100];
        public VolumeCalibration()
        {
            InitializeComponent();

         

            LoudSpeakingSlider.Value = VolumeAnalysis.t_loudSpeakingThreshold;
            SoftSpeakingSlider.Value = VolumeAnalysis.t_softSpeakingThreshold;
            isSpeakingSlider.Value = VolumeAnalysis.t_isSpeakingThreshold;

        }

        private void LoudSpeakingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VolumeAnalysis.t_loudSpeakingThreshold = (int)LoudSpeakingSlider.Value;

            MyCanvas.Children.Clear();

            displayLines();
        }

        private void SoftSpeakingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VolumeAnalysis.t_softSpeakingThreshold= (int)SoftSpeakingSlider.Value;

            MyCanvas.Children.Clear();

            displayLines();
        }

        private void isSpeakingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           
            VolumeAnalysis.t_isSpeakingThreshold= (int)isSpeakingSlider.Value;

            speakingThresholdLabel.Content = (int)isSpeakingSlider.Value;

            MyCanvas.Children.Clear();

            displayLines();
        }

        private void displayLines()
        {
            loud = new Line();
            loud.Visibility = System.Windows.Visibility.Visible;
            loud.StrokeThickness = 4;
            loud.Stroke = System.Windows.Media.Brushes.Blue;
            loud.X1 = 0;
            loud.X2 = MyCanvas.Width;
            loud.Y1 = MyCanvas.Height - (VolumeAnalysis.t_loudSpeakingThreshold * 1.9);
            loud.Y2 = MyCanvas.Height - (VolumeAnalysis.t_loudSpeakingThreshold * 1.9);
            MyCanvas.Children.Add(loud);


            soft = new Line();
            soft.Visibility = System.Windows.Visibility.Visible;
            soft.StrokeThickness = 4;
            soft.Stroke = System.Windows.Media.Brushes.Red;
            soft.X1 = 0;
            soft.X2 = MyCanvas.Width;
            soft.Y1 = MyCanvas.Height - (VolumeAnalysis.t_softSpeakingThreshold * 1.9);
            soft.Y2 = MyCanvas.Height - (VolumeAnalysis.t_softSpeakingThreshold * 1.9);
            MyCanvas.Children.Add(soft);

            speaking = new Line();
            speaking.Visibility = System.Windows.Visibility.Visible;
            speaking.StrokeThickness = 4;
            speaking.Stroke = System.Windows.Media.Brushes.Orange;
            speaking.X1 = 0;
            speaking.X2 = MyCanvas.Width;
            speaking.Y1 = MyCanvas.Height - (VolumeAnalysis.t_isSpeakingThreshold * 1.9);
            speaking.Y2 = MyCanvas.Height - (VolumeAnalysis.t_isSpeakingThreshold * 1.9);
            MyCanvas.Children.Add(speaking);
        }

        public void showWave()
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate
            {




                MyCanvas.Children.Clear();

                shiftLines(VolumeAnalysis.currentAudioLevel);

            

                volumeCounter++;
                if (volumeCounter > 99)
                {
                    volumeCounter = 0;
                }


            }));
        }

        private void shiftLines(double peakValue)
        {

            volumeLabel.Content = "Volume = " + peakValue;

            for (int i = 0; i < 100; i++)
            {
                if (i != 0)
                {
                    lineHeighs[i] = lineHeighs[i - 1];
                }
                else
                {
                    lineHeighs[i] = MyCanvas.Height - (peakValue*1.9);
                }

            }
            MyCanvas.Children.Clear();
            for (int i = 0; i < 100; i++)
            {

                Line line = new Line();

                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 3;
                line.Stroke = System.Windows.Media.Brushes.Black;
                line.X1 = 0 + i * 4;
                line.X2 = 0 + i * 4;
                line.Y1 = MyCanvas.Height;
                line.Y2 = lineHeighs[i];

                MyCanvas.Children.Add(line);
            }
            displayLines();
        }

     


    }
}
