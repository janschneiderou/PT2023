using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MemoryScript.xaml
    /// </summary>
    public partial class MemoryScript : UserControl
    {
        public delegate void ExitEvent(object sender, string x);
        public event ExitEvent exitEvent;

        #region thresholds

        double t_guesses = 0.7;
        double t_speed = 400;

        #endregion

        bool playing = false;

        string[] lines;

        string chars;
        string tempText;

        public MemoryScript()
        {
            InitializeComponent();

            WelcomePage.currentWord = 0;
            chars = "";

            SpeedSlider.Value = t_speed;
            try
            {

                Run run;
                lines = File.ReadAllLines(Environment.CurrentDirectory + "\\Script.txt");
                foreach (string s in lines)
                {


                    run = new Run(s);
                    run.Foreground = System.Windows.Media.Brushes.Black;
                    myTextBlock.Inlines.Add(run);
                    run = new Run(System.Environment.NewLine);
                    myTextBlock.Inlines.Add(run);


                }
            }
            catch
            {

            }
        }


        void start()
        {
            string text = "";
            foreach (Run r in myTextBlock.Inlines)
            {
                if (r.Text == text)
                {
                    r.Foreground = System.Windows.Media.Brushes.LightBlue;
                }
            }
        }

        public void startMemory()
        {
            while (playing)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(t_speed));
                int i=0;
                int j = 0;
                int characters=0;
                tempText = "";
                string trueText = "";
                foreach (string r in lines)
                {
                    if(i==WelcomePage.currentWord)
                    {
                        trueText=r;
                        characters = r.Length;
                        Random random = new Random(DateTime.Now.Millisecond);
                        int charNumber = random.Next(characters);
                        char ch = r[charNumber];
                        chars = chars + ch;
                        foreach(char c in r)
                        {
                            if(chars.IndexOf(c)==-1)
                            {
                                if (c.Equals(" "))
                                {
                                    tempText = tempText + " ";
                                }
                                else
                                {
                                    tempText = tempText + "_";
                                }
                                
                            }
                            else
                            {
                                tempText = tempText + c;
                            }
                        }

                        

                    }
                    i++;
                }
                j++;
                Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                    gameTextBlock.Text = tempText;

                }));

                
                if(chars.Length>=(int)(characters*t_guesses))
                {
                    if(WelcomePage.currentWord<myTextBlock.Inlines.Count)
                    {
                        chars = "";
                        WelcomePage.currentWord++;
                        Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {

                            Run run;
                            run = new Run(trueText);
                            run.Foreground = System.Windows.Media.Brushes.Red;
                            resultTextBlock.Inlines.Add(run);
                            run = new Run(System.Environment.NewLine);
                            resultTextBlock.Inlines.Add(run);

                        }));
             
                    }
                    else 
                    {
                        Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {

                           

                        }));
                        playing = false;
                    }
                }

                // myTextBlock.Inlines[WelcomePage.currentWord]
            }
        }

        public void recongnizedWord(string text)
        {
            if (WelcomePage.currentWord < SpeechToText.words.Count)
            {
                if (SpeechToText.words[WelcomePage.currentWord].Text == text)
                {
                    chars = "";
                    WelcomePage.currentWord++;
                    Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {

                        Run run;
                        run = new Run(text);
                        run.Foreground = System.Windows.Media.Brushes.Green;
                        resultTextBlock.Inlines.Add(run);
                        run = new Run(System.Environment.NewLine);
                        resultTextBlock.Inlines.Add(run);

                    }));
                    
                }
            }
            else 
            {
                Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {

                    
                    playing = false;

                }));
            }
        }


        private void Return_Click(object sender, RoutedEventArgs e)
        {
            WelcomePage.currentWord = 0;
            playing = false;
            exitEvent(this, "");
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            if(playing==false)
            {
                
                playing = true;
                resultTextBlock.Text = "";
                WelcomePage.currentWord = 0;
                myTextBlock.Visibility = Visibility.Collapsed;
                Thread thread = new Thread(startMemory);
                chars = "";
                thread.Start();
            }
            else
            {
                chars = "";
                
                playing=false;
                myTextBlock.Visibility=Visibility.Visible;
            }
            
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            t_speed = (double)e.NewValue;
        }
    }
}
