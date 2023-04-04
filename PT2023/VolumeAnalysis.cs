using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023
{

    
    public class VolumeAnalysis
    {
        #region thresholds
        public static int t_isSpeakingThreshold = 2;
        public static int t_softSpeakingThreshold = 5;
        public static int t_loudSpeakingThreshold = 35;

        public static double t_pauseThreshold = 1000;

        public static double t_speekingTooLongTime = 7000;
        public static double t_pausingTooLongTime = 5000;

        public static double t_guessTime = 500;
        public static int t_sentencesWithoutPauses = 3;

        #endregion

      

        #region helpers
        public static bool isSpeaking = false;
        private  bool softSpeaking = false;
        private  bool loudSpeaking = false;
        private bool isProbablySpeaking = false;
        private bool isProbablySpeakingLoud = false;
        private bool isProbablySpeakingSoft = false;

        public static int currentAudioLevel;

        private double speakingTime;
        private double pauseTime;
        private double speakTimeStart;
        private double pauseTimeStart;
        private double loudTimeStart;
        private double softTimeStart;

        private double currentTime;

        private int sentencesCounter = 0;

        #endregion

        #region mistakes

        public static bool m_speakingLongMistake=false;
        public static bool m_speakingLoudMistake = false;
        public static bool m_speakingSoftMistake = false;
        public static bool m_pausingLongMistake = false;

        #endregion

        #region flags
        public static bool f_speakingLong = true;
        public static bool f_speakingLoud = true;
        public static bool f_speakingSoft = true;
        public static bool f_pausingLong = true;
        public static bool f_pausingGood=false;
        public static bool f_pausingGoodLogged=false;

        #endregion



        public VolumeAnalysis()
        {

        }

        public void analyse()
        {
            isSpeakingAnalysis();
        }

        private void isSpeakingAnalysis()
        {
            currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            if (isSpeaking == false)
            {
                if (currentAudioLevel > t_isSpeakingThreshold && isProbablySpeaking == false)
                {
                    speakTimeStart = currentTime;
                    isProbablySpeaking = true;
                }
                else if (currentAudioLevel > t_isSpeakingThreshold && isProbablySpeaking == true && (currentTime - speakTimeStart) > t_guessTime)
                {
                    isSpeaking = true;
                    sentencesCounter++;
                }
                if (currentTime - pauseTimeStart > t_pauseThreshold)
                {
                    m_speakingLongMistake = false;
                    sentencesCounter = 0;
                    if(!f_pausingGoodLogged)
                    {
                        f_pausingGood = true;
                    }
                }
                if(currentTime-pauseTimeStart> t_pausingTooLongTime)
                {
                    m_pausingLongMistake = true;
                    f_pausingGood = false;
                }

            }
            else
            {
                f_pausingGoodLogged = false;
                f_pausingGood = false;

                if (f_speakingLoud)
                {
                    analyseLoudSpeaking();
                }
                if(f_speakingSoft)
                {
                    analyseSoftSpeaking();
                }
                m_pausingLongMistake = false;

                if (currentAudioLevel < t_isSpeakingThreshold && isProbablySpeaking == true)
                {
                    pauseTimeStart = DateTime.Now.TimeOfDay.TotalMilliseconds;
                    isProbablySpeaking = false;
                }
                else if (currentAudioLevel < t_isSpeakingThreshold && isProbablySpeaking == false && (currentTime - pauseTimeStart) > t_guessTime)
                {
                    isSpeaking = false;

                }
                if ((currentTime - speakTimeStart > t_speekingTooLongTime  || sentencesCounter> t_sentencesWithoutPauses) && f_speakingLong==true)
                {
                    m_speakingLongMistake = true;
                }
            }
        }

        private void analyseLoudSpeaking()
        {
            if(currentAudioLevel > t_loudSpeakingThreshold && isProbablySpeakingLoud==false)
            {
                loudTimeStart = currentTime;
                isProbablySpeakingLoud = true;
            }
            else if (currentAudioLevel> t_loudSpeakingThreshold && (currentTime - loudTimeStart) > t_guessTime)
            {
                m_speakingLoudMistake=true;
            }
            else if (currentAudioLevel < t_loudSpeakingThreshold)
            {
                isProbablySpeakingLoud = false;
                m_speakingLoudMistake = false;
            }
        }

        private void analyseSoftSpeaking()
        {
            if (currentAudioLevel < t_softSpeakingThreshold && isProbablySpeakingSoft == false)
            {
                softTimeStart = currentTime;
                isProbablySpeakingSoft = true;
            }
            else if (currentAudioLevel < t_softSpeakingThreshold && (currentTime - softTimeStart) > t_guessTime)
            {
                m_speakingSoftMistake = true;
            }
            else if (currentAudioLevel > t_softSpeakingThreshold)
            {
                isProbablySpeakingSoft = false;
                m_speakingSoftMistake = false;
            }
        }

    }
}
