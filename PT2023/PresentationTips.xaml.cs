using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for PresentationTips.xaml
    /// </summary>
    public partial class PresentationTips : UserControl, INotifyPropertyChanged
    {
        private string tipsText;
        private string[] tips;
        private int currentIndex;

        public string TipsText
        {
            get { return tipsText; }
            set { tipsText = value; NotifyPropertyChanged(); }
        }
        public delegate void ExitEvent(object sender, string x);
        public event ExitEvent exitEvent;
        public PresentationTips()
        {
            InitializeComponent();

            // Set the DataContext to the instance of the PresentationTips class
            DataContext = this;

            //Initial starting text
            TipsText = "Get your presentation tips here!";

            // Initialize tips array
            tips = new string[]
            {
                "Tip 1: Begin your presentation with a compelling story that captures the attention of your audience. Stories have the power to engage people emotionally and make your message more memorable.",
                "Tip 2: Use visual aids strategically to support your key points. Visuals such as charts, graphs, and images can help clarify complex information and make it easier for your audience to understand.",
                "Tip 3: Practice active listening during your presentation. Pay attention to your audience's reactions and adjust your delivery accordingly. Respond to questions and comments with openness and respect.",
                "Tip 4: Incorporate relevant examples and case studies to illustrate your ideas. Real-life examples make your content more relatable and show how your concepts can be applied in practical situations.",
                "Tip 5: Use a conversational tone and language in your presentation. Avoid jargon and technical terms that might confuse your audience. Speak in a way that everyone can understand and relate to.",
                "Tip 6: Make your presentation interactive by including activities or exercises that involve audience participation. This helps to keep your audience engaged and allows them to connect with the content on a deeper level.",
                "Tip 7: Use humor appropriately to lighten the mood and create a positive atmosphere. Well-placed humor can help you connect with your audience, make your presentation more enjoyable, and aid in information retention.",
                "Tip 8: Be mindful of your body language and non-verbal cues. Stand tall, maintain good posture, and use hand gestures purposefully to emphasize key points. Your body language should convey confidence and enthusiasm.",
                "Tip 9: Pace yourself during your presentation. Speak clearly and at a moderate speed to ensure that your audience can follow along. Take pauses to allow important points to sink in and to give yourself a moment to breathe.",
                "Tip 10: Close your presentation with a strong and memorable ending. Summarize your main points, reiterate your key message, and leave your audience with a call to action or a thought-provoking question.",
                // Add more tips as needed
            };

            currentIndex = -1; // Set currentIndex to -1 to indicate the initial state
            UpdateTipsText(); // Call UpdateTipsText() to display the initial text
        }

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void Return_Click(object sender, RoutedEventArgs e)
        {
            exitEvent(this, "");
        }

        private void Return_MouseEnter(object sender, MouseEventArgs e)
        {

            ReturnImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_returnO.png"));
        }

        private void Return_MouseLeave(object sender, MouseEventArgs e)
        {
            ReturnImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_return.png"));
        }

        private void Back_MouseEnter(object sender, MouseEventArgs e)
        {
            BackImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Back_O.png"));
        }

        private void Back_MouseLeave(object sender, MouseEventArgs e)
        {
            BackImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Back.png"));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Handle the back button click
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = tips.Length - 1;

            UpdateTipsText();
        }

        private void Next_MouseEnter(object sender, MouseEventArgs e)
        {
            NextImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_NextO.png"));
        }

        private void Next_MouseLeave(object sender, MouseEventArgs e)
        {
            NextImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Next.png"));
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            // Handle the next button click
            currentIndex++;
            if (currentIndex >= tips.Length)
                currentIndex = 0;

            UpdateTipsText();
        }

        private void UpdateTipsText()
        {
            if (currentIndex == -1)
            {
                TipsText = "Get your presentation tips here!";
            }
            else
            {
                TipsText = tips[currentIndex];
            }
        }
    }
}
