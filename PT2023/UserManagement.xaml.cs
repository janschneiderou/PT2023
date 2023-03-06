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
    /// Interaction logic for UserManagement.xaml
    /// </summary>
    public partial class UserManagement : UserControl
    {
        public static string usersPath;
        public static string presentationPath; 
        public static string usersPathScripts;
        public static string usersPathVideos;
        public static string usersPathLogs;
        public List<string> users;
        public List<string> presentations;
        string tempPath;

        public delegate void ExitEvent(object sender, string x);
        public event ExitEvent exitEvent;

        public UserManagement()
        {
            InitializeComponent();

            userGrid.Visibility = Visibility.Visible;
            presentationGrid.Visibility = Visibility.Collapsed;


            users = new List<string>();
            presentations = new List<string>();

            GetDirectories();


            usersListBox.ItemsSource = users;
        }

        private void GetDirectories()
        {
            string executingDirectory = Directory.GetCurrentDirectory();
            // usersPath = executingDirectory + "\\users"; 
            usersPath = System.IO.Path.Combine(executingDirectory, "Users");

            bool exists = System.IO.Directory.Exists(usersPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(usersPath);
            try
            {
                List<string> temp;
              
                temp = Directory.GetDirectories(usersPath).ToList();
                foreach (string s in temp)
                {
                    int x = s.LastIndexOf("\\");
                    users.Add(s.Substring(x + 1));
                }

            }
            catch (UnauthorizedAccessException)
            {

            }
        }

        private void getPresentationsDirectories()
        {
            
            tempPath = System.IO.Path.Combine(usersPath, "Presentations");

            bool exists = System.IO.Directory.Exists(tempPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(tempPath);
            try
            {
                List<string> temp;

                temp = Directory.GetDirectories(tempPath).ToList();
                foreach (string s in temp)
                {
                    int x = s.LastIndexOf("\\");
                    presentations.Add(s.Substring(x + 1));
                }

            }
            catch (UnauthorizedAccessException)
            {

            }
        }

        private void usersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            userNameTextBox.Text = (string)usersListBox.SelectedValue;
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            usersPath = System.IO.Path.Combine(usersPath, userNameTextBox.Text);
            
          
          

            bool exists = System.IO.Directory.Exists(usersPath);
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(usersPath);
                
            }

            presentationGrid.Visibility = Visibility.Visible;
            userGrid.Visibility = Visibility.Collapsed;
            getPresentationsDirectories();
            presentationsListBox.ItemsSource = presentations;
        }

        private void presentationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            presentationNameTextBox.Text = (string)presentationsListBox.SelectedValue;
        }

        private void presentationButton_Click(object sender, RoutedEventArgs e)
        {
            presentationPath = System.IO.Path.Combine(tempPath, presentationNameTextBox.Text);
            usersPathScripts = System.IO.Path.Combine(presentationPath, "Scripts");
            usersPathVideos = System.IO.Path.Combine(presentationPath, "Videos");
            usersPathLogs = System.IO.Path.Combine(presentationPath, "Logs");

            MainWindow.scriptPath = System.IO.Path.Combine(usersPathScripts + "\\Script.txt");

            bool exists = System.IO.Directory.Exists(presentationPath);
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(presentationPath);
                System.IO.Directory.CreateDirectory(usersPathScripts);
                System.IO.Directory.CreateDirectory(usersPathVideos);
                System.IO.Directory.CreateDirectory(usersPathLogs);
            }


            userGrid.Visibility = Visibility.Visible;
            presentationGrid.Visibility = Visibility.Collapsed;
            exitEvent(this, "");
        }
    }
}
