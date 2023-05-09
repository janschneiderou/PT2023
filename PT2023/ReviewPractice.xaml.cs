using Newtonsoft.Json;
using PT2023.LogObjects;
using System;
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
    /// Interaction logic for ReviewPractice.xaml
    /// </summary>
    public partial class ReviewPractice : UserControl
    {
        bool canReview = true;
        int currentSession = 0;
        PracticeSessions sessions;
        FeedbacksSentences feedbackSentences;
        bool videoLoaded=false;
        bool isPlaying=false;
        bool keepPlaying;

        public delegate void ExitEvent(object sender, string x);
        public event ExitEvent exitEvent;

        public ReviewPractice()
        {
            InitializeComponent();
            getSessions();

            VideoCreation vc = new VideoCreation();

        }


        void loadFeedback()
        {
            string path = System.IO.Path.Combine(UserManagement.usersPathLogs + "\\Feedbacks.json");
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
                feedbackSentences = new FeedbacksSentences();

                foreach (IdentifiedSentence sentence in sessions.sessions[currentSession].sentences)
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
                    foreach (IdentifiedSentence sentence in sessions.sessions[currentSession].sentences)
                    {
                        FeedbackForSentences f4s = new FeedbackForSentences("", "", "", sentence.sentence);
                        feedbackSentences.feedbacks.Add(f4s);
                    }
                }
            
                
            }
        }

        private void getSessions()
        {
            string path = System.IO.Path.Combine(UserManagement.usersPathLogs + "\\PracticeSession.json");
            if (!File.Exists(path))
            {
                warningLabel.Visibility = Visibility.Visible;
                canReview = false;

            }
            else
            {
                string jsonOne = File.ReadAllText(path);
                sessions = JsonConvert.DeserializeObject<PracticeSessions>(jsonOne);
                foreach (PracticeSession session in sessions.sessions)
                {
                    sessionDisplayed.Items.Add(session.start);
                  
                }

                sessionDisplayed.SelectedIndex = sessions.sessions.Count() - 1;
                currentSession = sessions.sessions.Count() - 1;
                selectVideo();
                loadFeedback();
                getSentences();
                
            }
        }
        
        void getSentences()
        {
            selectedSentences.Items.Clear();

            foreach (IdentifiedSentence sentence in sessions.sessions[currentSession].sentences)
            {
                selectedSentences.Items.Add(sentence.sentence);
            }
        }

        private void sessionDisplayed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentSession = sessionDisplayed.SelectedIndex;
           
            selectVideo();
            getSentences();
        }

        

        void selectVideo()
        {
            try
            {
                string videoPath = UserManagement.usersPathVideos + "\\" + sessions.sessions[currentSession].videoId + "c.mp4";
                myVideo.Source = new System.Uri(videoPath);
                videoLoaded = true;
                myVideo.Stop();


            }

            catch
            {
                videoLoaded = false;
            }
        }
        

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            exitEvent(this, "");
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if(videoLoaded)
            {
                if(isPlaying)
                {
                    myVideo.Pause();

                    

                    btnPlay.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Play.png"));
                    isPlaying = false;
                }
                else
                {
                    
                    myVideo.Play();
                    btnPlay.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_pause_play.png"));
                    isPlaying = true;
                }
               
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            myVideo.Stop();
            btnPlay.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Play.png"));
            isPlaying = false;
        }

        private void selectedSentences_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            foreach (FeedbackForSentences f4s in feedbackSentences.feedbacks)
            {
                if (selectedSentences.SelectedIndex != -1)
                {
                    if (f4s.sentence == sessions.sessions[currentSession].sentences[selectedSentences.SelectedIndex].sentence)
                    {
                        TextFeedbackExplanation.Text = f4s.Explanation;
                        TextFeedbackKeyword.Text = f4s.feedbackKeywords;
                    }
                }
            }
        }

        private void PlaySentence_Click(object sender, RoutedEventArgs e)
        {
            if(chbSentences.IsChecked==true)
            {
                myVideo.Position = sessions.sessions[currentSession].sentences[selectedSentences.SelectedIndex].start;
                myVideo.Play();

                Thread thread = new Thread(startSentencePlay);

                

                thread.Start();

                
            }
        }

        void startSentencePlay()
        {
            keepPlaying = true;
            while (keepPlaying)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(200));

                Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate
                {

                    if (myVideo.Position >= sessions.sessions[currentSession].sentences[selectedSentences.SelectedIndex].end)
                    {
                        keepPlaying = false;
                    }

                }));

                
                
            }
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate
            {

                myVideo.Pause();

            }));
        }

        private void btnAddFeedback_Click(object sender, RoutedEventArgs e)
        {
            foreach (FeedbackForSentences f4s in feedbackSentences.feedbacks )
            {
                if(f4s.sentence == sessions.sessions[currentSession].sentences[selectedSentences.SelectedIndex].sentence)
                {
                    f4s.Explanation = TextFeedbackExplanation.Text;
                    f4s.feedbackKeywords = TextFeedbackKeyword.Text;
                }
            }
            string path = System.IO.Path.Combine(UserManagement.usersPathLogs + "\\Feedbacks.json");
            string myString = Newtonsoft.Json.JsonConvert.SerializeObject(feedbackSentences);
            File.WriteAllText(path, myString);
        }

        #region button animations
        private void Play_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isPlaying)
            {
                btnPlay.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_pause_playO.png"));
            }
            else
            {
                btnPlay.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_PlayO.png"));
            }
            
        }

        private void Play_MouseLeave(object sender, MouseEventArgs e)
        {
            if(isPlaying)
            {
                btnPlay.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_pause_play.png"));
            }
            else
            {
                btnPlay.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Play.png"));
            }
            
        }

        private void Stop_MouseEnter(object sender, MouseEventArgs e)
        {
            btnStop.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_StopO.png"));
        }

        private void Stop_MouseLeave(object sender, MouseEventArgs e)
        {
            btnStop.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Stop.png"));
        }

        private void Return_MouseEnter(object sender, MouseEventArgs e)
        {

            ReturnImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_backO.png"));
        }

        private void Return_MouseLeave(object sender, MouseEventArgs e)
        {
            ReturnImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_return.png"));
        }

        private void PlaySentence_MouseEnter(object sender, MouseEventArgs e)
        {
            PlaySentenceImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_PlayO.png"));
        }
        private void PlaySentence_MouseLeave(object sender, MouseEventArgs e)
        {
            PlaySentenceImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Play.png"));
        }

        private void btnAddFeedback_MouseLeave(object sender, MouseEventArgs e)
        {
            btnAddFeedbackImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_add.png"));
        }

        private void btnAddFeedback_MouseEnter(object sender, MouseEventArgs e)
        {
            btnAddFeedbackImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_addO.png"));
        }

        #endregion




    }
}
