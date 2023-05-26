using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023.utilObjects
{
    public class SlideHandler
    {
        public static List<SlideConfig> SlideConfigs = new List<SlideConfig>();
       
        public SlideHandler()
        {
            
        }

        public void init()
        {


            SlideConfigs = new List<SlideConfig>();
            getSlideNames();
            getScriptText();
            getStartIndexes();




        }

        public static int getCurrentSlide(int word)
        {
            int currentSlide = 0;
            for(int i = 0; i < SlideConfigs.Count; i++)
            {
                if (word >= SlideConfigs[i].startIndex)
                {
                    currentSlide = i;
                }
            }

            
            return currentSlide;
        }

        void getSlideNames()
        {
            string path = System.IO.Path.Combine(UserManagement.presentationPath + "\\Slides.txt");
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();

            }
            else
            {
                SlideSelection.SlidesPath = File.ReadAllText(path);
            }
            if(SlideSelection.SlidesPath != "" || SlideSelection.SlidesPath != null)
            {
                bool exists = System.IO.Directory.Exists(SlideSelection.SlidesPath);

                if ( exists)
                {
                    var files = from file in Directory.EnumerateFiles(SlideSelection.SlidesPath) select file;

                    foreach (var file in files)
                    {
                        if (file.IndexOf("Slide") != -1 && file.IndexOf(".PNG") != -1)
                        {
                            SlideConfig slideConfig = new SlideConfig();
                            slideConfig.fileName = file;
                            SlideConfigs.Add(slideConfig);
                        }
                    }
                }
               
            }
            
        }

        void getScriptText()
        {
            int counter = 1;
            foreach(SlideConfig slideConfig in SlideConfigs)
            {
                string path = System.IO.Path.Combine(UserManagement.usersPathScripts + "\\Slide"+ counter+ ".txt");
                if (!File.Exists(path))
                {
                    FileStream fs = File.Create(path);
                    fs.Close();
                    slideConfig.scriptText = "";

                }
                else
                {
                    slideConfig.scriptText = File.ReadAllText(path);
                }
                counter++;
            }
        }
        void getStartIndexes()
        {
            int counter = 0;
            if (SlideConfigs.Count > 0)
            {
                SlideConfigs[0].startIndex = 0;
                for (int i = 1; i < SlideConfigs.Count; i++)
                {
                    counter = counter + CountLines(SlideConfigs[i - 1].scriptText);
                    SlideConfigs[i].startIndex = counter;
                }
            }
            
            //foreach (SlideConfig slideConfig in SlideConfigs)
            //{
            //    counter = counter + CountLines(slideConfig.scriptText);
            //    slideConfig.startIndex = counter;
            //}
        }
        public void setStartIndexAndText(int currentSlide)
        {

        }

        public void updateScriptText(int slide)
        {
            int slideTemp = slide + 1;
            string path = System.IO.Path.Combine(UserManagement.usersPathScripts + "\\Slide" + slideTemp + ".txt");

            File.WriteAllText(path, SlideConfigs[slide].scriptText);
            getStartIndexes();
        }

        public void generateScript()
        {
            try
            {
                string path = "";
                path = System.IO.Path.Combine(UserManagement.usersPathScripts + "\\Script.txt");
                MainWindow.scriptPath = path;
                if (!File.Exists(path))
                {
                    FileStream fs = File.Create(path);
                }

                string formattedText="";

                foreach (SlideConfig slideConfig in SlideConfigs)
                {
                    formattedText = formattedText + slideConfig.scriptText;
                }
                File.WriteAllText(MainWindow.scriptPath, formattedText);
            }
            catch
            {

            }
        }

         int CountLines(string text)
        {
            int count = 0;
            if (!string.IsNullOrEmpty(text))
            {
                count = text.Length - text.Replace("\n", string.Empty).Length;

                // if the last char of the string is not a newline, make sure to count that line too
                if (text[text.Length - 1] != '\n')
                {
                    ++count;
                }
            }

            return count;
        }

    }
}
