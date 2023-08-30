using Accord.Video.FFMPEG;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.DirectSound;
using System.Drawing;
using System.Threading.Tasks;

namespace PT2023
{
    public class RecordingClass
    {

        public static VideoFileWriter vf;
        public static AudioCaptureDevice AudioSource;
        public static int SourceTop;
        public static int SourceLeft;
        public static int SourceWidth;
        public static int SourceHeight;


        System.DateTime sartRecordingTime;
        System.DateTime lastRecordingVideoTime;
        System.DateTime lastRecordingSoundTime;




        private MemoryStream stream;


        private WaveEncoder encoder;

        Process process;

        private float[] current;

        private int frames;
        private int samples;
        private TimeSpan duration;

        string filename;
        string filenameAudio;
        string filenameCombined;
        int i;
        bool audioReady=false;

        private Thread myCaptureThread;
        public bool isRecording = false;

        Bitmap bmpScreenShot;

        public RecordingClass(string id)
        {

            foreach (var process in Process.GetProcessesByName("ffmpeg.exe"))
            {
                process.Kill();
            }

            // filenameAudio = MainWindow.recordingPath + "\\" + MainWindow.recordingID + ".wav";
            filenameAudio = System.IO.Path.Combine(UserManagement.usersPathVideos, id + ".wav");
            //filenameCombined = MainWindow.recordingPath + "\\" + MainWindow.recordingID + "c.mp4";
            filenameCombined = System.IO.Path.Combine(UserManagement.usersPathVideos, id + "c.mp4");
            //filename = MainWindow.recordingPath + "\\" + MainWindow.recordingID + ".mp4";
            filename = System.IO.Path.Combine(UserManagement.usersPathVideos, id + ".mp4");

            initalizeAudioStuff();

            vf = new VideoFileWriter();
        }

        #region audio
        private void initalizeAudioStuff()
        {

            try
            {
                audioReady = false;
                AudioSource = new AudioCaptureDevice();
                AudioDeviceInfo info = null;
                var adc = new AudioDeviceCollection(AudioDeviceCategory.Capture);
                foreach (var ad in adc)
                {
                    string desc = ad.Description;
                    if (desc.IndexOf("Audio") > -1)
                    {
                        info = ad;
                    }
                }
                if (info == null)
                {
                    AudioSource = new AudioCaptureDevice();
                }
                else
                {
                    AudioSource = new AudioCaptureDevice(info);
                }

                //AudioCaptureDevice source = new AudioCaptureDevice();
                AudioSource.DesiredFrameSize = 4096;
                AudioSource.SampleRate = 44100;
                //int sampleRate = 44100;
                //int sampleRate = 22050;

                AudioSource.NewFrame += AudioSource_NewFrame;
                // AudioSource.Format = SampleFormat.Format64BitIeeeFloat;
                AudioSource.AudioSourceError += AudioSource_AudioSourceError;
                // AudioSource.Start();
                //int x = 1;
            }
            catch
            {

            }

            // Create buffer for wavechart control
            current = new float[AudioSource.DesiredFrameSize];

            // Create stream to store file
            stream = new MemoryStream();
            encoder = new WaveEncoder(stream);

            frames = 0;
            samples = 0;





        }

        private void AudioSource_AudioSourceError(object sender, AudioSourceErrorEventArgs e)
        {
            int x = 1;
            x++;
        }

        private void AudioSource_NewFrame(object sender, Accord.Audio.NewFrameEventArgs e)
        {

            if (isRecording)
            {
                System.TimeSpan diff1 = DateTime.Now.Subtract(sartRecordingTime);
                if (diff1.Seconds >= 0.0)
                {
                    //writer.WriteAudioFrame(e.Signal.RawData);
                    e.Signal.CopyTo(current);

                    encoder.Encode(e.Signal);


                    duration += e.Signal.Duration;

                    samples += e.Signal.Samples;
                    frames += e.Signal.Length;
                }


            }
        }

        private void doAudioStop()
        {
            // Stops both cases
            if (AudioSource != null)
            {
                // If we were recording
                AudioSource.SignalToStop();
                AudioSource.WaitForStop();
            }


            var fileStream = File.Create(filenameAudio);
            stream.WriteTo(fileStream);
            fileStream.Close();
            fileStream.Dispose();
            // Also zero out the buffers and screen
            Array.Clear(current, 0, current.Length);
            // AudioSource.Dispose();
            audioReady = true;

        }

        #endregion

        public void startRecording()
        {
            isRecording = true;



            int screenWidth = (int)System.Windows.SystemParameters.PrimaryScreenWidth * 2;
            int screenHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight * 2;
            bmpScreenShot = new Bitmap(screenWidth, screenHeight);


            //vf.Open(filename, screenWidth, screenHeight, 25, VideoCodec.Default, 400000); //new comment




            vf.Open(filename, SourceWidth*2, SourceHeight*2, 25, VideoCodec.Default, 400000); //new line

            myCaptureThread = new Thread(new ThreadStart(captureFunction));
            myCaptureThread.Start();

            // Start
            AudioSource.Start();

        }

        #region stop

        public void stopRecording()
        {
            isRecording = false;
            //vf.Close();
            while(vf.IsOpen==true)
            {

            }
            doAudioStop();


        }

        public void combineFiles()
        {
            //Thread thread = new Thread(combineFilesThread);
            //thread.Start();

            Task taskA = Task.Run(() => combineFilesThread());



        }

        void combineFilesThread1()
        {
            //string args = "/c ffmpeg -i \"video.mp4\" -i \"mic.wav\" -shortest outPutFile.mp4";
            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.CreateNoWindow = false;
            //startInfo.FileName = "cmd.exe";
            //startInfo.WorkingDirectory = @"" + outputPath;
            //startInfo.Arguments = args;
            //using Process exeProcess = Process.Start(startInfo)

            //exeProcess.WaitForExit();
        }
        void combineFilesThread()
        {
            try
            {
                
                while(audioReady== false && vf.IsOpen == false)
                {
                   Thread.Sleep(100);
                }
                // Process.Start("ffmpeg", "-i " + filename + " -i " + filenameAudio + " -c:v copy -c:a aac -strict experimental " + filenameCombined + "");

                string FFmpegFilename;
                //string executingDirectory = Directory.GetCurrentDirectory();
                //string[] text = File.ReadAllLines(executingDirectory + "\\Config\\FFMPEGLocation.txt");
                FFmpegFilename = Directory.GetCurrentDirectory() + "\\FFmpeg\\bin\\ffmpeg.exe";

                process = new Process();
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.FileName = FFmpegFilename;


                process.StartInfo.Arguments = "-i " + filename + " -i " + filenameAudio + " -c:v copy -c:a aac -strict experimental " + filenameCombined + " -shortest";
                //  process.StartInfo.Arguments = "-i " + filename + " -i " + filenameAudio + " -map 0:v -map 1:a -c:v copy -c:a aac -strict experimental" + filenameCombined;
                //process.StartInfo.Arguments = "-i " + filename + " -i " + filenameAudio + " -c:v copy -c:a aac -strict experimental" + filenameCombined;
                // process.StartInfo.Arguments = "-i " + filename + " -i " + filenameAudio + " - c:v copy - map 0:v - map 1:a  " + filenameCombined  + " -shortest";


                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();



                // string cmd = FFmpegFilename + " -i " + filename + " -i " + filenameAudio + " -c:v copy -c:a aac -strict experimental " + filenameCombined + " -shortest";
                // Process.Start(cmd);

                //string processOutput = null;
                //while ((processOutput = process.StandardOutput.ReadLine()) != null)
                //{
                //    // do something with processOutput
                //    Debug.WriteLine(processOutput);
                //}


              



                process.WaitForExit();





                int a = 1;
                a++;
            

            }
            catch
            {
                int x = 0;
                x++;
            }
            

            

        }

        #endregion

        #region videoStuff
        private void captureFunction()
        {
            while (isRecording == true)
            {
                try
                {
                    int screenWidth = (int)System.Windows.SystemParameters.PrimaryScreenWidth * 2;
                    int screenHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight * 2;


                    Bitmap bmpScreenShot = new Bitmap(SourceWidth * 2, SourceHeight * 2); //new lines
                   
                    Graphics gfx = Graphics.FromImage((System.Drawing.Image)bmpScreenShot);

                    gfx.CopyFromScreen(SourceLeft*2, SourceTop*2, 0, 0, new System.Drawing.Size(SourceWidth*2, SourceHeight*2)); //new lines

                    //gfx.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));  //new comment
                    System.TimeSpan diff1 = DateTime.Now.Subtract(sartRecordingTime);
                    if(vf.IsOpen==true)
                    {
                        vf.WriteVideoFrame(bmpScreenShot, diff1);
                    }
                    
                }
                catch
                {

                }

                Thread.Sleep(30);
            }
            vf.Close();
           // vf.Dispose();

        }

        #endregion
    }
}

