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

using System.Speech;
using System.Speech.Synthesis;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

namespace PT2023
{
    /// <summary>
    /// Interaction logic for PracticePresentation.xaml
    /// </summary>
    public partial class PracticeSentences : UserControl
    {



        public delegate void ExitEvent(object sender, string x);
        public event ExitEvent exitEvent;

     
        bool readyToPresent = false;
        public bool showSkeleton = false;

        private VideoCapture m_Webcam;

        /// <summary>
        /// Our current m_frame to process.
        /// </summary>
        private Mat m_frame;

        #region poseEstimationInitTF


        /// <summary>
        /// Our pose estimator
        /// </summary>
        private PoseNetEstimator m_posenet;

        /// <summary>
        /// A basic flag checking if process frame is ongoing or not.
        /// </summary>
        static bool inprocessframe = false;

        /// <summary>
        /// A basic flag checking if process is ongoing or not
        /// </summary>
        static bool inprocess = false;

        int myImageHeight;
        int myImageWidth;

        bool f_isAnalysing;
        bool f_isLookingForMistakes = false;

        #endregion

        PoseAnalysis poseAnalysis;
        VolumeAnalysis volumeAnalysis;


        #region mistakes
        bool m_PhraseRecognize = true;
        bool m_posture = false;
        bool m_volume = false;
        bool m_gesture = false;

        #endregion


        #region logging
        public static string loggingString = "";
        #endregion

        #region text2Speech
        SpeechSynthesizer speechSynthesizerObj;
        #endregion

        #region Feedback strings

        string str_gesture;
        string str_volume;
        string str_dancing;
        string str_crossed_arms;
        string str_noHands;
        string str_faceHands;
        string str_crossed_legs;

        #endregion


        public PracticeSentences()
        {
            InitializeComponent();

            speechSynthesizerObj = new SpeechSynthesizer();
            loggingString = "";
            readyToPresent = false;
            myCountDown.startAnimation();

            f_isAnalysing = true;

            poseAnalysis = new PoseAnalysis();
            volumeAnalysis = new VolumeAnalysis();

            initTFstuff();
            initWebcam();

        }


        #region initTF stuff

        private void initTFstuff()
        {

            myImageWidth = (int)myImage.Width;
            myImageHeight = (int)myImage.Height;
            string appBaseDir = System.AppDomain.CurrentDomain.BaseDirectory;

            m_posenet = new PoseNetEstimator(frozenModelPath: appBaseDir + "models\\posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite",
                                             numberOfThreads: 4);

        }

        #endregion


        #region initWebcamStuff
        private void initWebcam()
        {

            m_frame = new Mat();

            // 2- Our webcam will represent the first camera found on our device
            m_Webcam = new VideoCapture(MainWindow.selectedCamera);

            // 3- When the webcam capture (grab) an image, callback on ProcessFrame method
            //m_Webcam.ImageGrabbed += M_Webcam_ImageGrabbed; // event based
            m_Webcam.ImageGrabbed += M_Webcam_ImageGrabbed; ; // event based
            m_Webcam.Start();

        }

        private Mat GetMatFromSDImage(System.Drawing.Image image)
        {
            int stride = 0;
            Bitmap bmp = new Bitmap(image);

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

            System.Drawing.Imaging.PixelFormat pf = bmp.PixelFormat;
            if (pf == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                stride = bmp.Width * 4;
            }
            else
            {
                stride = bmp.Width * 3;
            }

            Image<Bgra, byte> cvImage = new Image<Bgra, byte>(bmp.Width, bmp.Height, stride, (IntPtr)bmpData.Scan0);

            bmp.UnlockBits(bmpData);

            return cvImage.Mat;
        }

        #endregion

       

        #region thread that does everything
        /// <summary>
        /// //Thread where image is received and all important calls are made//
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void M_Webcam_ImageGrabbed(object sender, EventArgs e)
        {


            if (!inprocess)
            {
                // Say we start to process
                inprocess = true;

                // Reinit m_frame
                m_frame = new Mat();

                // Retrieve
                m_Webcam.Retrieve(m_frame);



                // If frame is not empty, try to process it
                if (!m_frame.IsEmpty)
                {
                    //if not already processing previous frame, process it
                    if (!inprocessframe)
                    {
                        ProcessFrame(m_frame.Clone());

                        if (f_isAnalysing == true)
                        {
                            poseAnalysis.analysePose(m_posenet.m_keypoints);
                            volumeAnalysis.analyse();
                            if(VolumeAnalysis.isSpeaking==true)
                            {
                                if(f_isLookingForMistakes==false)
                                {
                                    Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                                        hideStuff();
                                    }));
                                    
                                    resetMistakes();
                                    f_isLookingForMistakes = true;
                                }
                                else
                                {
                                    loadMistakes();
                                }
                                
                            }
                            else if(VolumeAnalysis.m_pausingLongMistake==true)
                            {
                                if(f_isLookingForMistakes == true)
                                {
                                    Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                                        setMistakesValues();
                                        showStuff();
                                    }));
                                    
                                    f_isLookingForMistakes = false;
                                }
                                
                            }
                            doScriptStuff();

                          
                        }

                    }

                    // Display keypoints and frame in imageview
                    if (showSkeleton == true)
                    {
                        ShowKeypoints();
                        ShowJoints();
                    }

                    // just for debug
                    

                    //
                    //Calcfeedback();

                    ShowFrame();

                }
                m_frame.Dispose();
                inprocess = false;

            }
        }

        #endregion


        #region  draw Skeleton and stuff

        private void ShowKeypoints()
        {
            if (!m_frame.IsEmpty)
            {
                float count = 1;
                foreach (Keypoint kpt in m_posenet.m_keypoints) // if not empty array of points
                {
                    if (kpt != null)
                    {
                        if ((kpt.position.X != -1) & (kpt.position.Y != -1)) // if points are valids
                        {
                            Emgu.CV.CvInvoke.Circle(m_frame, kpt.position,
                                                    3, new MCvScalar(200, 255, (int)((float)255 / count), 255), 2);
                        }
                        count++;
                    }
                }
            }
        }


        private void ShowJoints()
        {
            if (!m_frame.IsEmpty)
            {
                foreach (int[] joint in m_posenet.m_keypointsJoints) // if not empty array of points
                {
                    if (m_posenet.m_keypoints[joint[0]] != null & m_posenet.m_keypoints[joint[1]] != null)
                    {
                        if ((m_posenet.m_keypoints[joint[0]].position != new System.Drawing.Point(-1, -1)) &
                            (m_posenet.m_keypoints[joint[0]].position != new System.Drawing.Point(0, 0)) &
                            (m_posenet.m_keypoints[joint[1]].position != new System.Drawing.Point(-1, -1)) &
                            (m_posenet.m_keypoints[joint[1]].position != new System.Drawing.Point(0, 0))) // if points are valids
                        {
                            Emgu.CV.CvInvoke.Line(m_frame,
                                                  m_posenet.m_keypoints[joint[0]].position,
                                                  m_posenet.m_keypoints[joint[1]].position,
                                                  new MCvScalar(200, 255, 255, 255), 2);
                        }
                    }
                }
            }
        }

        #endregion

        #region process Pose detection
        /// <summary>
        /// A method to get keypoints from a frame.
        /// </summary>
        /// <param name="frame">A copy of <see cref="m_frame"/>. It could be resized beforehand.</param>
        public void ProcessFrame(Emgu.CV.Mat frame)
        {
            if (!inprocessframe)
            {
                inprocessframe = true;
                DateTime start = DateTime.Now;

                m_posenet.Inference(frame);

                DateTime stop = DateTime.Now;
                long elapsedTicks = stop.Ticks - start.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                Console.WriteLine(1000 / (double)elapsedSpan.Milliseconds);

                inprocessframe = false;
            }
        }


        #endregion


        #region display frame and skeleton
        private void ShowFrame()
        {
            if (!m_frame.IsEmpty)
            {

                CvInvoke.Resize(m_frame, m_frame, new System.Drawing.Size(myImageWidth, myImageHeight));
                Emgu.CV.CvInvoke.Flip(m_frame, m_frame, FlipType.Horizontal);
                var bitmap = Emgu.CV.BitmapExtension.ToBitmap(m_frame);
                // var bitmap = Emgu.CV.BitmapExtension.ToBitmap(m_background);
                Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate { drawImage(bitmap); }));
                Emgu.CV.CvInvoke.WaitKey(1); //wait a few clock cycles


            }
        }
        private void drawImage(System.Drawing.Bitmap bitmap)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            myImage.Source = bi;
        }

        #endregion

        #region do script stuff

        private void buttonSpeak_Click(object sender, RoutedEventArgs e)
        {
            speechSynthesizerObj.Dispose();
            if ((string)ScriptLabel.Content != "")
            {

                speechSynthesizerObj = new SpeechSynthesizer();

                speechSynthesizerObj.SpeakAsync((string)ScriptLabel.Content);

            }
        }


        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            if (WelcomePage.currentWord > 0)
            {
                WelcomePage.currentWord--;
            }
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (WelcomePage.currentWord < SpeechToText.words.Count)
            {
                WelcomePage.currentWord++;

            }
        }

        public void recognizedWord(string text)
        {
            if (WelcomePage.currentWord < SpeechToText.words.Count)
            {

                if (SpeechToText.words[WelcomePage.currentWord].Text == text
                    || SpeechToText.words[WelcomePage.currentWord].Text == " " + text
                    || SpeechToText.words[WelcomePage.currentWord].Text == text + " "
                    || SpeechToText.words[WelcomePage.currentWord].Text == " " + text + " ")
                {
                    m_PhraseRecognize = true;
                    Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                        setMistakesValues();
                        showStuff();
                    }));
                    f_isLookingForMistakes = false;
                }
            }
        }
        void doScriptStuff()
        {


            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                if (SpeechToText.words.Count > WelcomePage.currentWord)
                {
                    ScriptLabel.Content = SpeechToText.words[WelcomePage.currentWord].Text;
                }
                else
                {
                    ScriptLabel.Content = "The End";
                }

            }));
        }

        #endregion


        #region Pause Start and Exit
        private void Button_Pause_Play_Click(object sender, RoutedEventArgs e)
        {
            if (f_isAnalysing == true)
            {

                f_isAnalysing = false;
                Grid_Pause.Visibility = Visibility.Visible;
                DateTime currentTime = DateTime.Now;
                loggingString = loggingString + "<PauseTimeStart>" + currentTime.ToString() + "</PauseTimeStart>";
            }
            else
            {

                f_isAnalysing = true;
                Grid_Pause.Visibility = Visibility.Collapsed;
                DateTime currentTime = DateTime.Now;
                loggingString = loggingString + "<PauseTimeStop>" + currentTime.ToString() + "</PauseTimeStop>";
            }
        }

        private void Button_yes_Exit_Click(object sender, RoutedEventArgs e)
        {
            doExitStuff();
        }

        private void Button_keep_practicing_Click(object sender, RoutedEventArgs e)
        {
            Grid_Pause.Visibility = Visibility.Collapsed;
            DateTime currentTime = DateTime.Now;
            loggingString = loggingString + "<PauseTimeStop>" + currentTime.ToString() + "</PauseTimeStop>";

            f_isAnalysing = true;
        }

        public void doExitStuff()
        {
            DateTime currentTime = DateTime.Now;
            loggingString = loggingString + "<StopTIme>" + currentTime.ToString() + "</StopTime>";
            m_Webcam.Stop();
            m_Webcam.ImageGrabbed -= M_Webcam_ImageGrabbed;
            m_posenet = null;
            poseAnalysis = null;
            volumeAnalysis = null;
           
            WelcomePage.currentWord = 0;
            exitEvent(this, "");
        }

        private void myCountDown_countdownFinished(object sender)
        {
            myCountDown.Visibility = Visibility.Collapsed;
            DateTime currentTime = DateTime.Now;
            loggingString = loggingString + "<StartTIme>" + currentTime.ToString() + "</startTime>";
            readyToPresent = true;
        }

        #endregion


        #region feedback and interaction

        private void hideStuff()
        {
            imageGesture.Visibility=Visibility.Collapsed;
            imagePhrase.Visibility=Visibility.Collapsed;
            imagePosture.Visibility = Visibility.Collapsed;
            imageVolume.Visibility=Visibility.Collapsed;

            closePostureLabel.Visibility=Visibility.Collapsed;
            crosslegsLabel.Visibility=Visibility.Collapsed;
            dancingLabel.Visibility = Visibility.Collapsed;
            volumeLabel.Visibility=Visibility.Collapsed;
            gestureLabel.Visibility=Visibility.Collapsed;
            noHandsLabel.Visibility=Visibility.Collapsed; 
            touchFaceLabel.Visibility=Visibility.Collapsed;
        }

        private void showStuff()
        {
            imageGesture.Visibility = Visibility.Visible;
            imagePhrase.Visibility = Visibility.Visible;
            imagePosture.Visibility = Visibility.Visible;
            imageVolume.Visibility = Visibility.Visible;

            closePostureLabel.Visibility = Visibility.Visible;
            crosslegsLabel.Visibility = Visibility.Visible;
            dancingLabel.Visibility = Visibility.Visible;
            volumeLabel.Visibility = Visibility.Visible;
            gestureLabel.Visibility = Visibility.Visible;
            noHandsLabel.Visibility = Visibility.Visible;
            touchFaceLabel.Visibility = Visibility.Visible;
        }

        private void loadMistakes()
        {
            if(str_volume!="")
            {
                if(VolumeAnalysis.m_speakingLoudMistake== true)
                {
                    str_volume = "too loud";
                    m_volume = true;
                }
                else if(VolumeAnalysis.m_speakingSoftMistake== true)
                {
                    str_volume = "too soft";
                    m_volume = true;
                }
            }
            if(CalcArmsMovement2D.currentGesture!= CalcArmsMovement2D.Gesture.nogesture)
            {
                switch(CalcArmsMovement2D.currentGesture)
                {
                    case CalcArmsMovement2D.Gesture.small:
                        if(str_gesture!="medium" || str_gesture!="big")
                        {
                            str_gesture = "small";
                            m_gesture = false;
                        }
                        
                        break;
                    case CalcArmsMovement2D.Gesture.medium:
                        if(str_gesture!="big")
                        {
                            str_gesture = "medium";
                            m_gesture = false;
                        }
                        
                        break;
                    case CalcArmsMovement2D.Gesture.big:
                        str_gesture = "big";
                        m_gesture = false;
                        break;

                }
            }
           
            if(PoseAnalysis.m_CrossedLegs==true)
            {
                str_crossed_legs = "crossed legs";
                m_posture = true;
            }
            if (PoseAnalysis.m_handsFace== true)
            {
                str_faceHands = "hands touching face";
                m_posture = true;
            }
            if(PoseAnalysis.m_CrossedArms==true)
            {
                str_crossed_arms = "crossed arms";
                m_posture = true;
            }
            if(PoseAnalysis.m_noHands==true)
            {
                str_noHands = "no visible hands";
                m_posture = true;
            }
            if(PoseAnalysis.m_dancing==true)
            {
                str_dancing = "dancing";
                m_posture = true;
            }
        }

        private void resetMistakes()
        {
            str_gesture = "none";
            str_volume = "";
            str_dancing = "";
            str_crossed_arms = "";
            str_noHands = "";
            str_faceHands = "";
            str_crossed_legs = "";

            m_PhraseRecognize = false;
            m_posture = false;
            m_volume = false;
            m_gesture = true;
        }

        private void setMistakesValues()
        {
            setImages();
            
            setLabels();

        }
        private void setImages()
        {
            if (m_PhraseRecognize == true)
            {
                imagePhrase.Source = (new ImageSourceConverter()).ConvertFromString(Environment.CurrentDirectory + "\\Images\\correct.png") as ImageSource;
            }
            else
            {
                imagePhrase.Source = (new ImageSourceConverter()).ConvertFromString(Environment.CurrentDirectory + "\\Images\\wrong.png") as ImageSource;
            }

            if (m_gesture == true )
            {
                imageGesture.Source = (new ImageSourceConverter()).ConvertFromString(Environment.CurrentDirectory + "\\Images\\wrong.png") as ImageSource;

            }
            else
            {
                imageGesture.Source = (new ImageSourceConverter()).ConvertFromString(Environment.CurrentDirectory + "\\Images\\correct.png") as ImageSource;

            }

            if (m_volume == true)
            {
                imageVolume.Source = (new ImageSourceConverter()).ConvertFromString(Environment.CurrentDirectory + "\\Images\\wrong.png") as ImageSource;

            }
            else
            {
                imageVolume.Source = (new ImageSourceConverter()).ConvertFromString(Environment.CurrentDirectory + "\\Images\\correct.png") as ImageSource;

            }

            if (m_posture == true)
            {
                imagePosture.Source = (new ImageSourceConverter()).ConvertFromString(Environment.CurrentDirectory + "\\Images\\wrong.png") as ImageSource;

            }
            else
            {
                imagePosture.Source = (new ImageSourceConverter()).ConvertFromString(Environment.CurrentDirectory + "\\Images\\correct.png") as ImageSource;

            }
        }
        private void setLabels()
        {
            crosslegsLabel.Content = str_crossed_legs;
            closePostureLabel.Content = str_crossed_arms;
            dancingLabel.Content = str_dancing;
            noHandsLabel.Content = str_noHands;
            touchFaceLabel.Content = str_faceHands;
            volumeLabel.Content = str_volume;
            gestureLabel.Content = str_gesture;

        }


        private void Button_start_Click(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion


        #region animation for buttons
        private void Button_start_MouseEnter(object sender, MouseEventArgs e)
        {
            Button_startImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_returnO.png"));
        }

        private void Button_start_MouseLeave(object sender, MouseEventArgs e)
        {
            Button_startImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_return.png"));
        }

        #endregion

        private void buttonBack_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonBackImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Back_O.png"));
        }

        private void buttonBack_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonBackImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Back.png"));
        }

        private void buttonNext_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonNextImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_NextO.png"));
        }

        private void buttonNext_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonNextImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Next.png"));
        }

        private void buttonSpeak_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonSpeakImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_speakO.png"));
        }

        private void buttonSpeak_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonSpeakImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_speak.png"));
        }

        private void Button_yes_Exit_MouseEnter(object sender, MouseEventArgs e)
        {
            Button_yes_ExitImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_yesO.png"));
        }

        private void Button_yes_Exit_MouseLeave(object sender, MouseEventArgs e)
        {
            Button_yes_ExitImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_yes.png"));
        }

        private void Button_keep_practicing_MouseEnter(object sender, MouseEventArgs e)
        {
            Button_keep_practicingImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_noO.png"));
        }

        private void Button_keep_practicing_MouseLeave(object sender, MouseEventArgs e)
        {
            Button_keep_practicingImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_no.png"));
        }

        private void Button_Pause_Play_MouseEnter(object sender, MouseEventArgs e)
        {
            Button_Pause_PlayImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_backO.png"));
        }

        private void Button_Pause_Play_MouseLeave(object sender, MouseEventArgs e)
        {
            Button_Pause_PlayImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_return.png"));
        }
    }
}
