using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Windows;
using System.Diagnostics;
using System.Globalization;

namespace PT2023
{
    public class SpeechToText
    {
        /// <summary>
        /// the engine
        /// </summary>
        SpeechRecognitionEngine speechRecognitionEngine = null;

        /// <summary>
        /// list of predefined commands
        /// </summary>
        public  static List<Word> words = new List<Word>();


        private string currentLanguage = "en-GB";

        public delegate void SpeechRecognized(object sender, string text);
        public event SpeechRecognized speechRecognizedEvent;

        public delegate void VolumeReceived(object sender, int audioLevel);
        public event VolumeReceived volumeReceivedEvent;


        #region Constructor
        public SpeechToText()
        {
            try
            {

              //  currentLanguage = CultureInfo.InstalledUICulture.Name.ToString();
                // create the engine
                speechRecognitionEngine = createSpeechEngine(currentLanguage);

                // hook to events
                speechRecognitionEngine.AudioLevelUpdated += new EventHandler<AudioLevelUpdatedEventArgs>(engine_AudioLevelUpdated);
                speechRecognitionEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(engine_SpeechRecognized);

                // load dictionary
                loadGrammar();

                // use the system's default microphone
                speechRecognitionEngine.SetInputToDefaultAudioDevice();

                // start listening
                speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Voice recognition failed");
            }
        }




        #endregion


        #region internal functions and methods

        /// <summary>
        /// Creates the speech engine.
        /// </summary>
        /// <param name="preferredCulture">The preferred culture.</param>
        /// <returns></returns>
        private SpeechRecognitionEngine createSpeechEngine(string preferredCulture)
        {
            foreach (RecognizerInfo config in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (config.Culture.ToString() == preferredCulture)
                {
                    speechRecognitionEngine = new SpeechRecognitionEngine(config);
                    break;
                }
            }

            // if the desired culture is not found, then load default
            if (speechRecognitionEngine == null)
            {
                MessageBox.Show("The desired culture is not installed on this machine, the speech-engine will continue using "
                    + SpeechRecognitionEngine.InstalledRecognizers()[0].Culture.ToString() + " as the default culture.",
                    "Culture " + preferredCulture + " not found!");
                speechRecognitionEngine = new SpeechRecognitionEngine(SpeechRecognitionEngine.InstalledRecognizers()[0]);
            }

            return speechRecognitionEngine;
        }


        /// <summary>
        /// Loads the grammar and commands.
        /// </summary>
        private void loadGrammar()
        {
            try
            {
                words = new List<Word>();
                Choices texts = new Choices();
                string[] lines = File.ReadAllLines(MainWindow.scriptPath);
                foreach (string line in lines)
                {
                    // skip commentblocks and empty lines..
                    if (line.StartsWith("--") || line == String.Empty) continue;

                    // split the line
                    var parts = line.Split(new char[] { '|' });

                    // add commandItem to the list for later lookup or execution
                    words.Add(new Word() { Text = line });

                    // add the text to the known choices of speechengine
                    texts.Add(line);
                }
                Grammar wordsList = new Grammar(new GrammarBuilder(texts));
                speechRecognitionEngine.LoadGrammar(wordsList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region speechEngine events

        /// <summary>
        /// Handles the SpeechRecognized event of the engine control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Recognition.SpeechRecognizedEventArgs"/> instance containing the event data.</param>
        void engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            try
            {
                if(speechRecognitionEngine != null)
                {
                    speechRecognizedEvent(this, e.Result.Text);
                }
                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        /// <summary>
        /// Handles the AudioLevelUpdated event of the engine control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Recognition.AudioLevelUpdatedEventArgs"/> instance containing the event data.</param>
        void engine_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            try
            {
                if(volumeReceivedEvent != null)
                {
                    volumeReceivedEvent(this, e.AudioLevel);
                }
                
            }//prgLevel.Value = e.AudioLevel;
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


        }

        #endregion

    }
}
