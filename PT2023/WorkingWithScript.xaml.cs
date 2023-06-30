﻿using PT2023.utilObjects;
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

        int MaxCharacters = 40;
        string path;
        List<string> slides;
        int currentSlide = 0;
        SlideHandler slideHandler;

        public WorkingWithScript()
        {
            InitializeComponent();
       
            getSlides();
            setSlideAndText();
            if (SlideHandler.SlideConfigs.Count == 0)
            {
                writeText();
            }




        }


        void writeText()
        {
            try
            {
                path = System.IO.Path.Combine(UserManagement.usersPathScripts + "\\Script.txt");
                MainWindow.scriptPath = path;
                if (!File.Exists(path))
                {
                    FileStream fs = File.Create(path);
                }
               
                    //Run run;
                    string[] lines = File.ReadAllLines(path);
                foreach (string s in lines)
                {
                    scriptText.Text = scriptText.Text + s + System.Environment.NewLine;

                

                }
            }
            catch
            {

            }
        }

        void refreshText()
        {
            scriptText.Text = "";
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            
            doChunkstuff();
            updateSlideInfo();
            if (SlideHandler.SlideConfigs.Count > 0)
            {
                slideHandler.generateScript();
            }
                
            exitEvent(this, "");
        }

        public string createChunks(string line)
        {
            string text = "";
            string tempLine = line;
            int position = -1;
            bool cut= false;

            //chunk if there is a tripple dot



            while (tempLine.Length > MaxCharacters || tempLine.Contains(".") || tempLine.Contains(",")||  tempLine.Contains("!") || 
                tempLine.Contains("?") || tempLine.Contains(";") || tempLine.Contains(":") )
            {
                cut = false;
                position = tempLine.IndexOf("...");
                if (tempLine.IndexOf("...") != -1 && position<MaxCharacters)
                {
                    text = text + tempLine.Substring(0, position +3) + System.Environment.NewLine;
                    tempLine = tempLine.Substring(position+3);
                    cut= true;
                }
                position = tempLine.IndexOf(".");
                if (tempLine.IndexOf(".") != -1 && position < MaxCharacters)
                {
                    text = text + tempLine.Substring(0, position +1 ) + System.Environment.NewLine;
                    tempLine = tempLine.Substring(position + 1);
                    cut = true;
                   
                }
                position = tempLine.IndexOf(",");
                if (tempLine.IndexOf(",") != -1 && position < MaxCharacters)
                {
                    text = text + tempLine.Substring(0, position +1 ) + System.Environment.NewLine;
                    tempLine = tempLine.Substring(position + 1);
                    cut = true;
                }
                position = tempLine.IndexOf("!");
                if (tempLine.IndexOf("!") != -1 && position < MaxCharacters)
                {
                    text = text + tempLine.Substring(0, position +1) + System.Environment.NewLine;
                    tempLine = tempLine.Substring(position + 1);
                    cut = true;
                }
                position = tempLine.IndexOf("?");
                if (tempLine.IndexOf("?") != -1 && position < MaxCharacters)
                {
                    text = text + tempLine.Substring(0, position +1 ) + System.Environment.NewLine;
                    tempLine = tempLine.Substring(position + 1);
                    cut = true;
                }
                position = tempLine.IndexOf(":");
                if (tempLine.IndexOf(":") != -1 && position < MaxCharacters)
                {
                    text = text + tempLine.Substring(0, position +1 ) + System.Environment.NewLine;
                    tempLine = tempLine.Substring(position + 1);
                    cut = true;
                }
                position = tempLine.IndexOf(";");
                if (tempLine.IndexOf(";") != -1 && position < MaxCharacters)
                {
                    text = text + tempLine.Substring(0, position +1) + System.Environment.NewLine;
                    tempLine = tempLine.Substring(position + 1);
                    cut = true;
                }
                if( cut == false)
                {
                    position = findeIndexesOfSpace(tempLine);

                    string tempString = tempLine.Substring(0, position + 1);
                    if(tempString.IndexOf(" ")==0)
                    {
                        tempString = tempString.Substring(1);
                    }
                    if(tempString.LastIndexOf(" ")==tempString.Length-1 )
                    {
                        tempString = tempString.Substring(0, tempString.Length - 1);
                    }
                    text = text + tempString + System.Environment.NewLine;
                    tempLine = tempLine.Substring(position + 1);
                }
                


            }


            if (tempLine.IndexOf(" ") == 0)
            {
                tempLine = tempLine.Substring(1);
            }
            if (tempLine.LastIndexOf(" ") == tempLine.Length - 1 && tempLine.Length > 0)
            {
                tempLine = tempLine.Substring(0, tempLine.Length - 1);
            }
            if(tempLine.Length > 0)
            {
                text = text + tempLine + System.Environment.NewLine;
            }
            

            

            return text;
        }

        int findeIndexesOfSpace(string line)
        {
            int indexesSpace = -1;
           

            string tempLine = line;

            while (tempLine.LastIndexOf(" ") > MaxCharacters)
            {
                indexesSpace = tempLine.LastIndexOf(" ");
                tempLine = tempLine.Substring(0, indexesSpace);
            }



            

            return tempLine.LastIndexOf(" ");
        }

        private void chunkButton_Click(object sender, RoutedEventArgs e)
        {
            doChunkstuff();
            refreshText();
            if (SlideHandler.SlideConfigs.Count > 0)
            {
                setSlideAndText();
            }
             else
            {
                writeText();
            }


        }

        void doChunkstuff()
        {
            string line = "";
            string formattedText = "";
            StringReader reader = new StringReader(scriptText.Text);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length > MaxCharacters || line.Contains(".") || line.Contains(",") 
                    || line.Contains("!") || line.Contains("?") || line.Contains(";") 
                    || line.Contains(":") )
                {
                    formattedText = formattedText + createChunks(line);
                }
                else
                {
                    
                    if (line.IndexOf(" ") == 0)
                    {
                        line = line.Substring(1);
                    }
                    formattedText = formattedText + line + System.Environment.NewLine;
                }
            };

            if (SlideHandler.SlideConfigs.Count > 0)
            {
                SlideHandler.SlideConfigs[currentSlide].scriptText = formattedText;
            }
            else
            {
                File.WriteAllText(MainWindow.scriptPath, formattedText);
            }

           // File.WriteAllText(MainWindow.scriptPath, formattedText);
        }

        #region button animations

        private void Button_Close_MouseEnter(object sender, MouseEventArgs e)
        {
            Button_CloseImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_save_closeO.png"));
        }

        private void Button_Close_MouseLeave(object sender, MouseEventArgs e)
        {
            Button_CloseImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_save_close.png"));
        }

        private void chunkButton_MouseEnter(object sender, MouseEventArgs e)
        {
            chunkButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_chunkO.png"));
        }

        private void chunkButton_MouseLeave(object sender, MouseEventArgs e)
        {
            chunkButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_chunk.png"));
        }

        private void buttonPrevious_MouseEnter(object sender, MouseEventArgs e)
        {
            prevButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Back_O.png"));
        }

        private void buttonPrevious_MouseLeave(object sender, MouseEventArgs e)
        {
            prevButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Back.png"));
        }

        private void buttonNext_MouseEnter(object sender, MouseEventArgs e)
        {
            nextButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_NextO.png"));
        }

        private void buttonNext_MouseLeave(object sender, MouseEventArgs e)
        {
            nextButtonImg.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\btn_Next.png"));
        }


        #endregion

        #region slides Stuff
        void setSlideAndText()
        {
            if(SlideHandler.SlideConfigs.Count>0)
            {
                slideImg.Source = new BitmapImage(new Uri(SlideHandler.SlideConfigs[currentSlide].fileName));
                scriptText.Text = SlideHandler.SlideConfigs[currentSlide].scriptText;
                buttonNext.Visibility=Visibility.Visible;
            }
            else
            {
                buttonNext.Visibility=Visibility.Collapsed;
                
            }
            buttonPrevious.Visibility = Visibility.Collapsed;


        }
        void getSlides()
        {
            slideHandler = new SlideHandler();
            slideHandler.init();

        }


        void updateSlideInfo()
        {
            
            slideHandler.updateScriptText(currentSlide);
        }

        #endregion
        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if(currentSlide<SlideHandler.SlideConfigs.Count-1)
            {
                
                doChunkstuff();
                updateSlideInfo();
                currentSlide++;
                refreshText();
                setSlideAndText();
                buttonPrevious.Visibility = Visibility.Visible;
                if (currentSlide  == SlideHandler.SlideConfigs.Count-1)
                {
                    buttonNext.Visibility = Visibility.Collapsed;
                }

            }
           
        }

        private void buttonPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentSlide > 0)
            {
                doChunkstuff();
                updateSlideInfo();
                currentSlide--;
                refreshText();
                setSlideAndText();
                buttonNext.Visibility = Visibility.Visible;
                buttonPrevious.Visibility = Visibility.Visible;
            }
            else
            {
                buttonPrevious.Visibility = Visibility.Collapsed;
            }

        }



    }
}
