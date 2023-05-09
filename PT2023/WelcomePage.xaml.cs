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
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;

namespace PT2023
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : UserControl
    {

        FilterInfoCollection connectedDevices;
        public  VolumeCalibration volumeCalibration;
        public PracticeMode practiceMode;
        public WorkingWithScript workingWithScript;
        public MemoryScript memoryScript;
        public MemoryScript2 memoryScript2;
        public PracticeSentences practiceSentences;
        public UserManagement userManagement;
        public ReviewPractice reviewPractice;

        #region speech stuff
        private SpeechToText speechToText;
        public static int currentWord = 0;

        #endregion

        public VolumeAnalysis volumeAnalysis;

        public WelcomePage()
        {
            InitializeComponent();

            volumeAnalysis = new VolumeAnalysis();

           // initSpeech();

            connectedDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo fi in connectedDevices)
            {
                
                cameraSelector.Items.Add(fi.Name);
            }
            cameraSelector.SelectedIndex = 0;
            volumeCalibration = new VolumeCalibration();
            myGrid.Children.Add(volumeCalibration);
            volumeCalibration.Margin = new Thickness(0, 30, 0, 50);
            volumeCalibration.Visibility = Visibility.Collapsed;
            volumeCalibration.VerticalAlignment = VerticalAlignment.Bottom;
            volumeCalibration.HorizontalAlignment = HorizontalAlignment.Left;

            addUserMagangement();
            
        }

        #region Speech Analysis Init and delete
        public void initSpeech()
        {

            speechToText = new SpeechToText();
            speechToText.speechRecognizedEvent += SpeechRecognition_speechRecognizedEvent;
            speechToText.volumeReceivedEvent += SpeechRecognition_volumeReceivedEvent;
        }

        private void SpeechRecognition_volumeReceivedEvent(object sender, int audioLevel)
        {
            VolumeAnalysis.currentAudioLevel = audioLevel;
            if (volumeCalibration.Visibility == Visibility.Visible)
            {
                volumeCalibration.showWave();
            }
        }

        private void SpeechRecognition_speechRecognizedEvent(object sender, string text)
        {
           
            if(practiceMode!=null)
            {
                if(practiceMode.Visibility==Visibility.Visible)
                {
                    practiceMode.recognizedWord(text);
                }
                
            }

            if(memoryScript!=null)
            {
                if(memoryScript.Visibility==Visibility.Visible)
                {
                    memoryScript.recongnizedWord(text);
                }
                
            }
            if(practiceSentences!=null)
            {
                if(practiceSentences.Visibility==Visibility.Visible)
                {
                    practiceSentences.recognizedWord(text);
                }
                
            }
            if (memoryScript2 != null)
            {
                if (memoryScript2.Visibility == Visibility.Visible)
                {
                    memoryScript2.recongnizedWord(text);
                }

            }


        }

        public void restartSpeech()
        {
           
           
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                initSpeech();
            }));
        }


        #endregion

        private void cameraSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.selectedCamera = cameraSelector.SelectedIndex;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            if(volumeCalibration.Visibility != Visibility.Visible)
            {
                
                volumeCalibration.Visibility = Visibility.Visible;

            }
            else
            {
                volumeCalibration.Visibility = Visibility.Collapsed;
            }
            
        }

        #region handling selections

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Grid_for_Mode_Selection.Visibility = Visibility.Collapsed;
            addPracticeMode();
        }


        private void addUserMagangement()
        {
            userManagement = new UserManagement();
            myGrid.Children.Add(userManagement);
            userManagement.Margin = new Thickness(0, 0, 0, 0);
            userManagement.VerticalAlignment = VerticalAlignment.Center;
            userManagement.HorizontalAlignment = HorizontalAlignment.Center;
            userManagement.Visibility = Visibility.Visible;
            userManagement.exitEvent += UserManagement_exitEvent;
            Grid_for_Mode_Selection.Visibility = Visibility.Collapsed;
        }

        

        private void addPracticeMode()
        {
            practiceMode = new PracticeMode();
            if(checkBoxScript.IsChecked==true)
            {
                practiceMode.setWithScript();
            }
            if(checkBoxSkeleton.IsChecked==true)
            {
                practiceMode.showSkeleton = true;
            }
            myGrid.Children.Add(practiceMode);
           
            
            practiceMode.Margin = new Thickness(0, 0, 0, 0);
            practiceMode.VerticalAlignment = VerticalAlignment.Center;
            practiceMode.HorizontalAlignment = HorizontalAlignment.Center;
            practiceMode.Visibility = Visibility.Visible;
            WelcomePage.currentWord = 0;
            practiceMode.exitEvent += PracticeMode_exitEvent;
        }

        private void UserManagementButton_Click(object sender, RoutedEventArgs e)
        {
            addUserMagangement();
        }

        private void Button_add_Script_Click(object sender, RoutedEventArgs e)
        {
            Grid_for_Mode_Selection.Visibility = Visibility.Collapsed;
            workingWithScript = new WorkingWithScript();
            myGrid.Children.Add(workingWithScript);

            workingWithScript.Margin = new Thickness(0, 0, 0, 0);
            workingWithScript.VerticalAlignment = VerticalAlignment.Center;
            workingWithScript.HorizontalAlignment = HorizontalAlignment.Center;
            workingWithScript.Visibility = Visibility.Visible;
           
            workingWithScript.exitEvent += WorkingWithScript_exitEvent; 
        }

        private void Button_add_Memory_Click(object sender, RoutedEventArgs e)
        {
            Grid_for_Mode_Selection.Visibility = Visibility.Collapsed;
            memoryScript2 = new MemoryScript2();
            myGrid.Children.Add(memoryScript2);
            memoryScript2.Margin = new Thickness(0, 0, 0, 0);
            memoryScript2.VerticalAlignment = VerticalAlignment.Center;      
            memoryScript2.HorizontalAlignment = HorizontalAlignment.Center;
            WelcomePage.currentWord = 0;
            memoryScript2.exitEvent += MemoryScript2_exitEvent;

        }

        private void Button_Practice_Sentence_Click(object sender, RoutedEventArgs e)
        {
            Grid_for_Mode_Selection.Visibility = Visibility.Collapsed;
            practiceSentences = new PracticeSentences();

            if (checkBoxSkeletonPractice.IsChecked == true)
            {
                practiceSentences.showSkeleton = true;
            }

            myGrid.Children.Add(practiceSentences);
            practiceSentences.Margin = new Thickness(0, 0, 0, 0);
            practiceSentences.VerticalAlignment = VerticalAlignment.Center;
            practiceSentences.HorizontalAlignment = HorizontalAlignment.Center;
            practiceSentences.exitEvent += PracticeSentences_exitEvent;
            WelcomePage.currentWord= 0;
        }

        private void Button_Review_Practice_Click(object sender, RoutedEventArgs e)
        {
            Grid_for_Mode_Selection.Visibility = Visibility.Collapsed;
            reviewPractice = new ReviewPractice();
            myGrid.Children.Add(reviewPractice);
            reviewPractice.Margin = new Thickness(0, 0, 0, 0);
            reviewPractice.VerticalAlignment = VerticalAlignment.Center;
            reviewPractice.HorizontalAlignment = HorizontalAlignment.Center;
            reviewPractice.exitEvent += ReviewPractice_exitEvent;

        }

      


        #endregion

        #region exit events

        private void UserManagement_exitEvent(object sender, string x)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                userManagement.Visibility = Visibility.Collapsed;
                myGrid.Children.Remove(userManagement);
                Grid_for_Mode_Selection.Visibility = Visibility.Visible;
                initSpeech();
            }));
        }

        private void WorkingWithScript_exitEvent(object sender, string x)
        {
            restartSpeech();
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                workingWithScript.Visibility = Visibility.Collapsed;
                myGrid.Children.Remove(workingWithScript);
                Grid_for_Mode_Selection.Visibility = Visibility.Visible;
            }));
        }

        private void PracticeMode_exitEvent(object sender, string x)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                practiceMode.Visibility = Visibility.Collapsed;
                myGrid.Children.Remove(practiceMode);
                Grid_for_Mode_Selection.Visibility = Visibility.Visible;
            }));

        }

        private void MemoryScript_exitEvent(object sender, string x)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                memoryScript.Visibility = Visibility.Collapsed;
                myGrid.Children.Remove(memoryScript);
                Grid_for_Mode_Selection.Visibility = Visibility.Visible;
            }));
        }

        private void MemoryScript2_exitEvent(object sender, string x)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                memoryScript2.Visibility = Visibility.Collapsed;
                myGrid.Children.Remove(memoryScript2);
                Grid_for_Mode_Selection.Visibility = Visibility.Visible;
            }));
        }

        private void PracticeSentences_exitEvent(object sender, string x)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                practiceSentences.Visibility = Visibility.Collapsed;
                myGrid.Children.Remove(practiceSentences);
                Grid_for_Mode_Selection.Visibility = Visibility.Visible;
            }));
        }

        private void ReviewPractice_exitEvent(object sender, string x)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                reviewPractice.Visibility = Visibility.Collapsed;
                myGrid.Children.Remove(reviewPractice);
                Grid_for_Mode_Selection.Visibility = Visibility.Visible;
            }));
        }


        #endregion

        
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        #region button animations

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            volumeButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_volume1O.png"));
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            volumeButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_volume1.png"));
        }

        private void UserManagementButton_MouseEnter(object sender, MouseEventArgs e)
        {
            UserManagementButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_userO.png"));
        }

        private void UserManagementButton_MouseLeave(object sender, MouseEventArgs e)
        {
            UserManagementButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_user.png"));
        }

        private void Button_MouseEnter_1(object sender, MouseEventArgs e)
        {
            buttonExitImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_exit1O.png"));
        }

        private void Button_MouseLeave_1(object sender, MouseEventArgs e)
        {
            buttonExitImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_exit1.png"));
        }

        #endregion
    }
}
