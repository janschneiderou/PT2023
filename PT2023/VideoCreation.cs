using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PT2023
{
    public class VideoCreation
    {
        string filename;
        string filenameAudio;
        string filenameCombined;
        Process process;

        public VideoCreation()
        {
            foreach (var process in Process.GetProcessesByName("ffmpeg.exe"))
            {
                process.Kill();
            }
            deleteDuplicates();
            string[] files = Directory.GetFiles(UserManagement.usersPathVideos);
            List<string> ids = new List<string>();
            List<string> wavs = new List<string>();
            List<string> mp4s = new List<string>();

        

            //get Ids. 
            foreach (string file in files)
            {
                string id = "";
                if(file.IndexOf(".wav")!=-1)
                {
                    id = file.Substring(0, file.IndexOf("."));

                    // filenameAudio = MainWindow.recordingPath + "\\" + MainWindow.recordingID + ".wav";
                    filenameAudio = System.IO.Path.Combine(UserManagement.usersPathVideos, id + ".wav");
                    //filenameCombined = MainWindow.recordingPath + "\\" + MainWindow.recordingID + "c.mp4";
                    filenameCombined = System.IO.Path.Combine(UserManagement.usersPathVideos, id + "c.mp4");
                    //filename = MainWindow.recordingPath + "\\" + MainWindow.recordingID + ".mp4";
                    filename = System.IO.Path.Combine(UserManagement.usersPathVideos, id + ".mp4");


                }
                if (filenameAudio != null)
                {
                    break;
                }
            }
            if(filenameAudio != null)
            {
                combineFiles();
            }
            
            
        }

        void deleteDuplicates()
        {
            List<string> ids = new List<string>();
            string[] files = Directory.GetFiles(UserManagement.usersPathVideos);
            foreach (string file in files)
            {
                if(file.IndexOf("c.mp4")!=-1)
                {
                    //ids.Add(file.Substring(0, file.IndexOf("c.")));
                    string id= file.Substring(0, file.IndexOf("c."));
                    if (File.Exists(id + ".wav"))
                    {
                        try
                        {
                            File.Delete(id + ".wav");
                        }
                        catch
                        {

                        }
                        
                    }
                    if (File.Exists(id + ".mp4"))
                    {
                        try
                        {
                            File.Delete(id + ".mp4");
                        }
                        catch
                        {

                        }
                    }
                   
                }
            }
           
            
        }

        public void combineFiles()
        {
            Thread thread = new Thread(combineFilesThread);
            thread.Start();

        }

        void combineFilesThread()
        {
            try
            {
                // Process.Start("ffmpeg", "-i " + filename + " -i " + filenameAudio + " -c:v copy -c:a aac -strict experimental " + filenameCombined + "");

                string FFmpegFilename;
                //string executingDirectory = Directory.GetCurrentDirectory();
                //string[] text = File.ReadAllLines(executingDirectory + "\\Config\\FFMPEGLocation.txt");
                FFmpegFilename = Directory.GetCurrentDirectory() + "\\FFmpeg\\bin\\ffmpeg.exe";

                process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.FileName = FFmpegFilename;
                // process.StartInfo.FileName = @"C:\FFmpeg\bin\ffmpeg.exe";

                process.StartInfo.Arguments = "-i " + filename + " -i " + filenameAudio + " -c:v copy -c:a aac -strict experimental " + filenameCombined + " -shortest";


                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();

                // vf.Dispose();
                // AudioSource.Dispose();

            }
            catch
            {
                int x = 0;
                x++;
            }
        }
    }
}
