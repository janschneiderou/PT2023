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
    /// Interaction logic for SlideSelection.xaml
    /// </summary>
    public partial class SlideSelection : UserControl
    {
        public static string SlidesPath;

        public delegate void ExitEvent(object sender, string x);
        public event ExitEvent exitEvent;
        public SlideSelection()
        {
            InitializeComponent();

            string path = System.IO.Path.Combine(UserManagement.presentationPath + "\\Slides.txt");
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();

            }
            else
            {
                textboxPath.Text = File.ReadAllText(path);
            }
        }

        private void Button_findSlides_Click(object sender, RoutedEventArgs e)
        {
            // Create a "Save As" dialog for selecting a directory (HACK)
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = textboxPath.Text; // Use current value for initial dir
            dialog.Title = "Select the Directory of your slides"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                // Our final value is in path
                textboxPath.Text = path;
            }
        }


        #region buttonsAnimations
        private void Button_findSlides_MouseEnter(object sender, MouseEventArgs e)
        {
            findButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_findO.png"));
        }

        private void Button_findSlides_MouseLeave(object sender, MouseEventArgs e)
        {
            findButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_find.png"));
        }
        private void Go_back_MouseEnter(object sender, MouseEventArgs e)
        {
            go_backImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_backO.png"));
        }

        private void Go_back_MouseLeave(object sender, MouseEventArgs e)
        {
            go_backImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_return.png"));
        }


        #endregion



        private void Go_back_Click(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(UserManagement.presentationPath + "\\Slides.txt");
            File.WriteAllText(path, textboxPath.Text);
            SlidesPath= textboxPath.Text;
            exitEvent(this, "");
        }
    }
}
