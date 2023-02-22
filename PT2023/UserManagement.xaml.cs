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
        public static string usersPathScripts;
        public static string usersPathVideos;
        public static string usersPathLogs;
        public List<string> users;

        public delegate void ExitEvent(object sender, string x);
        public event ExitEvent exitEvent;

        public UserManagement()
        {
            InitializeComponent();

            users = new List<string>();
            GetDirectories();


            usersListBox.ItemsSource = users;
        }

        private void GetDirectories()
        {
            string executingDirectory = Directory.GetCurrentDirectory();
            // usersPath = executingDirectory + "\\users"; 
            usersPath = System.IO.Path.Combine(executingDirectory, "users");

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

        private void usersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            userNameTextBox.Text = (string)usersListBox.SelectedValue;
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            usersPath = System.IO.Path.Combine(usersPath, userNameTextBox.Text);
            usersPathScripts = System.IO.Path.Combine(usersPath, "Scripts");
            usersPathVideos = System.IO.Path.Combine(usersPath, "Videos");
            usersPathLogs = System.IO.Path.Combine(usersPath, "Logs");
          
            MainWindow.scriptPath = System.IO.Path.Combine(usersPathScripts + "\\Script.txt");

            bool exists = System.IO.Directory.Exists(usersPath);
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(usersPath);
                System.IO.Directory.CreateDirectory(usersPathScripts);
                System.IO.Directory.CreateDirectory(usersPathVideos);
                System.IO.Directory.CreateDirectory(usersPathLogs);
            }
                

            exitEvent(this, "");
        }
    }
}
