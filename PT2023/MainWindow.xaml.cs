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

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;

namespace PT2023
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region variables that can be seen everywhere

        public static int selectedCamera=0;

        #endregion

        public static string scriptPath="";

 //       private SpeechToText speechToText;

        public VolumeAnalysis volumeAnalysis;

        WelcomePage welcomePage;

        public MainWindow()
        {
            InitializeComponent();

            this.WindowState=  System.Windows.WindowState.Maximized;

            scriptPath = Environment.CurrentDirectory + "\\Scripts\\Script.txt";
            if (!File.Exists(scriptPath))
            {
                createSampleScriptFile();
            }

            welcomePage = new WelcomePage();    
            MainGrid.Children.Add(welcomePage);

            
        }
       
        
        public void createSampleScriptFile()
        {
            using (StreamWriter sw = File.CreateText(scriptPath))
            {
                sw.WriteLine("Hello}");
                sw.WriteLine("This is an example of a script");
                sw.WriteLine("Writing a script of what you want to say is a good practice while preparing a presentation");
                sw.WriteLine("Try doing it here");
                sw.WriteLine("you just need to add every sentence in a different line");
                sw.WriteLine("Have fun using the Presentation Trainer");
            }
        }

      
    }
}
