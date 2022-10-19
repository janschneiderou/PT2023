using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for WorkingWithScript.xaml
    /// </summary>
    public partial class WorkingWithScript : UserControl
    {
        public delegate void ExitEvent(object sender, string x);
        public event ExitEvent exitEvent;

        public WorkingWithScript()
        {
            InitializeComponent();

            try
            {
                //Run run;
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\Script.txt");
                foreach (string s in lines)
                {
                    scriptText.Text = scriptText.Text + s + System.Environment.NewLine;

                    //run = new Run(s);
                    //run.Foreground = System.Windows.Media.Brushes.Black;
                    //scriptText.Inlines.Add(run);
                    //run = new Run(System.Environment.NewLine);
                    //scriptText.Inlines.Add(run);


                }
            }
            catch
            {

            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(MainWindow.scriptPath, scriptText.Text);
            exitEvent(this, "");
        }
    }
}
