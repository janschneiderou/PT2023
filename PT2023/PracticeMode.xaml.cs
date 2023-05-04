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
using System.Drawing;
using System.Speech;
using System.Speech.Synthesis;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using PT2023.LogObjects;
using System.IO;
using Newtonsoft.Json;


namespace PT2023
{
    /// <summary>
    /// Interaction logic for PracticeMode.xaml
    /// </summary>
    public partial class PracticeMode : UserControl
    {
        public bool withScript = false;
        public bool showSkeleton = false;

        #region things for script

        bool showScript = true;

        public static bool readyToPresent=false;

        FeedbacksSentences feedbackSentences;

        bool readyForManualFeedback = false;
        bool manualVissible = true;


        #endregion

        bool autoVissible = true;

        public delegate void ExitEvent(object sender, string x);
        public event ExitEvent exitEvent;

        RecordingClass recordingClass;
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

        #endregion

        PoseAnalysis poseAnalysis;
        VolumeAnalysis volumeAnalysis;
        RulesAnaliserFIFO rulesAnaliserFIFO;

        #region logging
        public static string loggingString="";
        public static LogObjects.PracticeSession practiceSession;
        #endregion

        #region text2Speech
        SpeechSynthesizer speechSynthesizerObj;
        #endregion



        public PracticeMode()
        {
            


            InitializeComponent();

            speechSynthesizerObj = new SpeechSynthesizer();
            loggingString = "";
            readyToPresent = false;
            myCountDown.startAnimation();

            if (withScript == false)
            {
                CB_Show_Script.IsEnabled = false;
                CB_Show_Script.IsChecked = false;
                CB_Show_Script_Checked(null, null);
            }
           


            f_isAnalysing = true;

            poseAnalysis = new PoseAnalysis();
            volumeAnalysis = new VolumeAnalysis();
            rulesAnaliserFIFO = new RulesAnaliserFIFO();

            initTFstuff();
            initWebcam();
           
            
            rulesAnaliserFIFO.feedBackEvent += RulesAnaliserFIFO_feedBackEvent;
            rulesAnaliserFIFO.correctionEvent += RulesAnaliserFIFO_correctionEvent;


        }
        #region event catchers from the Analysis
        private void RulesAnaliserFIFO_correctionEvent(object sender, PresentationAction x)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                FeedbackLabel.Visibility=Visibility.Collapsed;
            }));
        }

        private void RulesAnaliserFIFO_feedBackEvent(object sender, PresentationAction x)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                if(autoVissible==true)
                {
                    FeedbackLabel.Visibility = Visibility.Visible;
                    FeedbackLabel.Content = x.message;
                }
                
            }));
        }

        #endregion


        #region FeedbacksForScript

        void loadFeedbacks()
        {
            string path = System.IO.Path.Combine(UserManagement.usersPathLogs + "\\Feedbacks.json");
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
                feedbackSentences = new FeedbacksSentences();

                foreach (IdentifiedSentence sentence in practiceSession.sentences)
                {
                    FeedbackForSentences f4s = new FeedbackForSentences("", "", "", sentence.sentence);
                    feedbackSentences.feedbacks.Add(f4s);
                }

                string myString = Newtonsoft.Json.JsonConvert.SerializeObject(feedbackSentences);
                File.WriteAllText(path, myString);

            }
            else
            {
                string jsonOne = File.ReadAllText(path);
                feedbackSentences = JsonConvert.DeserializeObject<FeedbacksSentences>(jsonOne);
                if (feedbackSentences == null)
                {
                    foreach (IdentifiedSentence sentence in practiceSession.sentences)
                    {
                        FeedbackForSentences f4s = new FeedbackForSentences("", "", "", sentence.sentence);
                        feedbackSentences.feedbacks.Add(f4s);
                    }
                }


            }
            readyForManualFeedback = true;
        }

        void displayManualFeedback()
        {
            if (withScript == true && readyForManualFeedback == true)
            {
               // ManualFeedback.Visibility = Visibility.Visible;
                foreach (FeedbackForSentences f4s in feedbackSentences.feedbacks)
                {
                    if (f4s.sentence == ScriptLabel.Content.ToString())
                    {
                        ManualFeedback.Content = f4s.feedbackKeywords;
                    }
                }

            }
        }


        #endregion


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

                        if(f_isAnalysing== true)
                        {
                            poseAnalysis.analysePose(m_posenet.m_keypoints);
                            volumeAnalysis.analyse();
                            if(readyToPresent==true)
                            {
                                rulesAnaliserFIFO.AnalyseRules();
                            }
                            
                            if(withScript==true && showScript== true)
                            {
                                doScriptStuff();
                            }
                        }
                        
                    }

                    // Display keypoints and frame in imageview
                    if(showSkeleton==true)
                    {
                        ShowKeypoints();
                        ShowJoints();
                    }

                    // just for debug
                    try
                    {
                        
                            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                                if (poseAnalysis != null)
                                {
                                    DebugLabel.Content = "Andlge LeftUP=" + CalcArmsMovement2D.currentGesture + "\n";
                                }
                            }));
                        
                       
                    }
                   catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    
                  //
                    //Calcfeedback();
                   
                    ShowFrame();

                }
                m_frame.Dispose();
                inprocess = false;

            }
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
                    || SpeechToText.words[WelcomePage.currentWord].Text == " "+text
                    || SpeechToText.words[WelcomePage.currentWord].Text == text +" "
                    || SpeechToText.words[WelcomePage.currentWord].Text == " " + text + " ")
                {
                    WelcomePage.currentWord++;
                }
            }
            logRecognisedWord(text);
        }
        void doScriptStuff()
        {
            
           
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate {
                if (SpeechToText.words.Count > WelcomePage.currentWord)
                {
                    ScriptLabel.Content = SpeechToText.words[WelcomePage.currentWord].Text;

                    displayManualFeedback();

                
                    
                }
                else
                {
                    ScriptLabel.Content = "The End";
                }
                
            }));
        }

        public void setWithScript()
        {
            withScript = true;
            CB_Show_Script.IsEnabled = true;
            CB_Show_Script.IsChecked = true;
            CB_Show_Script_Checked(null, null);
        }

        #endregion

        #region logs
        public void logRecognisedWord(string text)
        {
            if (practiceSession != null)
            {
                foreach (IdentifiedSentence identifiedSentence in practiceSession.sentences)
                {
                    if (identifiedSentence.sentence == text
                        || identifiedSentence.sentence == " " + text
                        || identifiedSentence.sentence == text + " "
                        || identifiedSentence.sentence == " " + text + " ")
                    {
                        identifiedSentence.wasIdentified = true;
                        identifiedSentence.start = VolumeAnalysis.sentenceStarted;
                        identifiedSentence.end = DateTime.Now.AddSeconds(0) - PracticeMode.practiceSession.start;
                        
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


                //Emgu.CV.CvInvoke.Flip(m_frame, m_frame, FlipType.Horizontal);
                //var bitmap = Emgu.CV.BitmapExtension.ToBitmap(m_frame);
                //Dispatcher.BeginInvoke(new ThreadStart(delegate { drawImage(bitmap); }));
                //Emgu.CV.CvInvoke.WaitKey(1); //wait a few clock cycles
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

        #region checkboxes

        private void CB_Manual_Unchecked(object sender, RoutedEventArgs e)
        {
            manualVissible = false;
            ManualFeedback.Visibility = Visibility.Collapsed;
        }

        private void CB_Manual_Checked(object sender, RoutedEventArgs e)
        {
            manualVissible=true;
            ManualFeedback.Visibility = Visibility.Visible;
        }

        private void CB_Auto_Unchecked(object sender, RoutedEventArgs e)
        {
            autoVissible=false;
            FeedbackLabel.Visibility = Visibility.Collapsed;
        }

        private void CB_Auto_Checked(object sender, RoutedEventArgs e)
        {
            autoVissible = true;
        }

        private void CB_Speak_Checked(object sender, RoutedEventArgs e)
        {
            if(CB_Speak.IsChecked == false)
            {
                VolumeAnalysis.m_pausingLongMistake = false;
                VolumeAnalysis.f_pausingLong = false;
            }
            else
            {
                VolumeAnalysis.f_pausingLong = true;
            }
            

        }

        private void CB_Pauses_Checked(object sender, RoutedEventArgs e)
        {
            if(CB_Pauses.IsChecked==false)
            {
                VolumeAnalysis.m_speakingLongMistake = false;
                VolumeAnalysis.f_speakingLong = false;
            }
            else
            {
                VolumeAnalysis.f_speakingLong = true;
            }
        }

        private void CB_Soft_Checked(object sender, RoutedEventArgs e)
        {
            if (CB_Soft.IsChecked == false)
            {
                VolumeAnalysis.m_speakingSoftMistake = false;
                VolumeAnalysis.f_speakingSoft = false;
            }
            else
            {
                VolumeAnalysis.f_speakingSoft = true;
            }
        }

        private void CB_Loud_Checked(object sender, RoutedEventArgs e)
        {
            if (CB_Loud.IsChecked == false)
            {
                VolumeAnalysis.m_speakingLoudMistake = false;
                VolumeAnalysis.f_speakingLoud = false;
            }
            else
            {
                VolumeAnalysis.f_speakingLoud = true;
            }
        }

        private void CB_Hands_Face_Checked(object sender, RoutedEventArgs e)
        {
            if (CB_Hands_Face.IsChecked == false)
            {
              
                PoseAnalysis.f_handsFace = false;
            }
            else
            {
                PoseAnalysis.f_handsFace = true;
            }
        }

        private void CB_Hands_Checked(object sender, RoutedEventArgs e)
        {
            if (CB_Hands.IsChecked == false)
            {
             
                PoseAnalysis.f_noHands = false;
            }
            else
            {
                PoseAnalysis.f_handsFace = true;
            }
        }

        private void CB_Dancing_Checked(object sender, RoutedEventArgs e)
        {
            if (CB_Dancing.IsChecked == false)
            {
                
                PoseAnalysis.f_dancing = false;
            }
            else
            {
                PoseAnalysis.f_dancing = true;
            }
        }

        private void CB_Gestures_Checked(object sender, RoutedEventArgs e)
        {
            if (CB_Gestures.IsChecked == false)
            {
               
                PoseAnalysis.f_noGestures = false;
            }
            else
            {
                PoseAnalysis.f_noGestures = true;
            }
        }

        private void CB_LegsCrossed_Checked(object sender, RoutedEventArgs e)
        {
            if (CB_LegsCrossed.IsChecked == false)
            {
 
                PoseAnalysis.f_CrossedLegs = false;
            }
            else
            {
                PoseAnalysis.f_CrossedLegs = true;
            }
        }

        private void CB_ArmsCrossed_Checked(object sender, RoutedEventArgs e)
        {
            if (CB_ArmsCrossed.IsChecked == false)
            {
               
                PoseAnalysis.f_CrossedArms = false;
            }
            else
            {
                PoseAnalysis.f_CrossedArms = true;
            }
        }


        private void CB_Show_Script_Checked(object sender, RoutedEventArgs e)
        {
            if(CB_Show_Script.IsChecked == false)
            {
                showScript= false;
                scrptCanvas.Visibility = Visibility.Collapsed;
                buttonBack.Visibility = Visibility.Collapsed;
                buttonNext.Visibility = Visibility.Collapsed;
                ScriptLabel.Visibility = Visibility.Collapsed;
                buttonSpeak.Visibility = Visibility.Collapsed;
            }
            else 
            {
                showScript = true;
                scrptCanvas.Visibility= Visibility.Visible;
                buttonBack.Visibility = Visibility.Visible;
                buttonNext.Visibility = Visibility.Visible;
                ScriptLabel.Visibility = Visibility.Visible;
                buttonSpeak.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Pause Start and Exit
        private void Button_Pause_Play_Click(object sender, RoutedEventArgs e)
        {
            if(f_isAnalysing==true)
            {
                
                f_isAnalysing = false;
                Grid_Pause.Visibility = Visibility.Visible;
                DateTime currentTime = DateTime.Now;
                pauseLoggingStuff();
                //loggingString = loggingString + "<PauseTimeStart>" + currentTime.ToString() + "</PauseTimeStart>";
            }
            else
            {
               
                f_isAnalysing = true;
                Grid_Pause.Visibility = Visibility.Collapsed;
                DateTime currentTime = DateTime.Now;
               // loggingString = loggingString + "<PauseTimeStop>" + currentTime.ToString() + "</PauseTimeStop>";
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

            f_isAnalysing =true;
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
            rulesAnaliserFIFO = null;
            WelcomePage.currentWord = 0;
            finishLoggingStuff();
            exitEvent(this, "");
        }

        private void myCountDown_countdownFinished(object sender)
        {
            myCountDown.Visibility=Visibility.Collapsed;
            DateTime currentTime = DateTime.Now;
            loggingString = loggingString + "<StartTIme>" + currentTime.ToString() + "</startTime>";
            readyToPresent = true;
            startLoggingStuff();
            

        }

        private void startLoggingStuff()
        {
            practiceSession = new PracticeSession();
            practiceSession.scriptVisible = withScript;

            // Start VIDEO stuff
            recordingClass = new RecordingClass(practiceSession.videoId);
            recordingClass.startRecording();
            //End video Stuff

            foreach (Word word in SpeechToText.words)
            {
                IdentifiedSentence identifiedSentence = new IdentifiedSentence(word.Text);
                practiceSession.sentences.Add(identifiedSentence);
            }
            loadFeedbacks();
        }

        private void pauseLoggingStuff()
        {
            foreach(PracticeLogAction pa in practiceSession.actions)
            {
                TimeSpan tp = new TimeSpan();
                if(pa.end==tp)
                {
                    pa.end = DateTime.Now - practiceSession.start;
                }
            }
        }

        void saveToJSON()
        {
            string myString = Newtonsoft.Json.JsonConvert.SerializeObject(practiceSession);
            Console.WriteLine(myString);
        }
        private void finishLoggingStuff()
        {
            // saveToJSON();
            practiceSession.end = DateTime.Now;
            string path = System.IO.Path.Combine(UserManagement.usersPathLogs + "\\PracticeSession.json");
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
                PracticeSessions sessions = new PracticeSessions();

                sessions.sessions.Add(practiceSession);
                string myString = Newtonsoft.Json.JsonConvert.SerializeObject(sessions);
                File.WriteAllText(path, myString);

            }
            else
            {
                string jsonOne = File.ReadAllText(path);
                PracticeSessions sessions = JsonConvert.DeserializeObject<PracticeSessions>(jsonOne);
                if (sessions == null)
                {
                    sessions = new PracticeSessions();
                }
                sessions.sessions.Add(practiceSession);
                string myString = Newtonsoft.Json.JsonConvert.SerializeObject(sessions);
                File.WriteAllText(path, myString);
            }
            recordingClass.stopRecording();
            recordingClass.combineFiles();
        }

        #endregion

       
    }
}
