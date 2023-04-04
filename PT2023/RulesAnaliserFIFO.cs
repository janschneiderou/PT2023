using PT2023.LogObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023
{
    public class RulesAnaliserFIFO
    {
        public delegate void FeedBackEvent(object sender, PresentationAction x);
        public event FeedBackEvent feedBackEvent;

        public delegate void CorrectionEvent(object sender, PresentationAction x);
        public event CorrectionEvent correctionEvent;


        #region thresholds
        public double t_timeBetweenFeedbacks = 3500;
        #endregion

        ArrayList mistakes;
        ArrayList l_mistakes;
        ArrayList l_newMistakes;
        ArrayList l_mistakesL;

        PresentationAction[] sentMistakes;
        PresentationAction[] crossedArms;
        PresentationAction[] handsNearFace;

        PresentationAction[] highVolumes;
        PresentationAction[] lowVolumes;
        PresentationAction[] longPauses;
        PresentationAction[] longTalks;
        PresentationAction[] periodicMovements;

        PresentationAction[] legsCrossed;
        PresentationAction[] handsNotMoving;

        PresentationAction previousAction;

        int crossedArmsMistakes = 0;
        int handsNearFaceMistakes = 0;
   
     
        int highVolumesMistakes = 0;
        int lowVolumesMistakes = 0;
        int longPausesMistakes = 0;
        int longTalksMistakes = 0;
        int periodicMovementsMistakes = 0;

        int legsCrossedMistakes = 0;
        int handsNotMovingMistakes = 0;

        public double lastFeedbackTime = 0;
       
        public bool noInterrupt = true;

        int interruptNumber = 9999999;//4; this can be used in case we want to add some interruptions

        public RulesAnaliserFIFO()
        {
            
            mistakes = new ArrayList();
            l_mistakesL = new ArrayList();
            l_newMistakes = new ArrayList();

            crossedArms = new PresentationAction[interruptNumber];
            handsNearFace = new PresentationAction[interruptNumber];
           
         
            highVolumes = new PresentationAction[interruptNumber];
            lowVolumes = new PresentationAction[interruptNumber];
            longPauses = new PresentationAction[interruptNumber];
            longTalks = new PresentationAction[interruptNumber];
            periodicMovements = new PresentationAction[interruptNumber];
           
            legsCrossed = new PresentationAction[interruptNumber];
            handsNotMoving = new PresentationAction[interruptNumber];
           
            previousAction = new PresentationAction();
            previousAction.myMistake = PresentationAction.MistakeType.NOMISTAKE;
        }

        #region mistakesLLists

        private void updateMistakeListL()
        {
            if (l_mistakesL.Count > 0)
            {
                deleteNoLongerMistakesL();
            }

            addNewMistakesL();
        }

        private void addNewMistakesL()
        {
            l_newMistakes.Clear();
            //Look PoseMistakes
            addPoseMistakes();
            //Look VolumeMistakes
            addVolumeMistakes();
           

            foreach (PresentationAction ba in l_newMistakes)
            { 
                DateTime currentTime = DateTime.Now;
                bool isAlreadyThere = false;
                foreach (PresentationAction a in l_mistakesL)
                {
                    if (ba.myMistake == a.myMistake)
                    {
                        isAlreadyThere = true;
                        break;
                    }
                }
                if (isAlreadyThere == false)
                {
                    if (ba.interrupt)
                    {
                        l_mistakesL.Insert(0, ba);
                    }
                    else
                    {
                       l_mistakesL.Add(ba);
                        PracticeLogAction pa = new PracticeLogAction();
                        pa.id = ba.timeStarted;
                        pa.start = PracticeMode.practiceSession.start-DateTime.Now;
                        pa.logAction = ba.myMistake.ToString();
                        PracticeMode.practiceSession.actions.Add(pa);


                       // PracticeMode.loggingString = PracticeMode.loggingString + "<Start>" + ba.myMistake + " timestart =" + currentTime.ToString() + "</start>\n";
                    }
               

                }
            }
        }

        private void deleteNoLongerMistakesL()
        {
            int[] nolongerErrors = new int[l_mistakesL.Count];
            int i = 0;
            DateTime currentTime = DateTime.Now;

            foreach (PresentationAction a in l_mistakesL)
            {

                foreach (PresentationAction ba in l_newMistakes)
                {
                    if (a.myMistake == ba.myMistake)
                    {
                        nolongerErrors[i] = 1;
                        break;
                    }
                }
                i++;
            }
            for (int j = nolongerErrors.Length; j > 0; j--)
            {
                if (nolongerErrors[j - 1] == 0)
                {
                    // logTimeMistakes(j - 1);
                    foreach(PracticeLogAction pa in PracticeMode.practiceSession.actions )
                    {
                        if(pa.id == ((PresentationAction)l_mistakesL[j-1]).timeStarted)
                        {
                            pa.end=PracticeMode.practiceSession.start - DateTime.Now;
                            break;
                        }
                    }
                   // PracticeMode.loggingString = PracticeMode.loggingString + "<stop>" + ((PresentationAction)l_mistakesL[j-1]).myMistake + " timestop =" + currentTime.ToString() + "</stop> \n";
                    l_mistakesL.RemoveAt(j - 1);
                }
            }
        }

        private void addVolumeMistakes()
        {
            if(VolumeAnalysis.m_speakingLoudMistake)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.HIGH_VOLUME);
                l_newMistakes.Insert(0, pa);
            }
            if(VolumeAnalysis.m_speakingSoftMistake)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.LOW_VOLUME);
                l_newMistakes.Insert(0, pa);
            }
            if(VolumeAnalysis.m_speakingLongMistake)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.LONG_TALK);
                l_newMistakes.Insert(0, pa);
            }
            if(VolumeAnalysis.m_pausingLongMistake)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.LONG_PAUSE);
                l_newMistakes.Insert(0, pa);
            }
        }

        private void addPoseMistakes()
        {
            if (PoseAnalysis.m_CrossedLegs)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.LEGSCROSSED);
                l_newMistakes.Insert(0, pa);
            }
            if (PoseAnalysis.m_noGestures)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.HANDS_NOT_MOVING);
                l_newMistakes.Insert(0, pa);
            }
            if (PoseAnalysis.m_CrossedArms)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.ARMSCROSSED);
                l_newMistakes.Insert(0, pa);
            }
            if (PoseAnalysis.m_dancing)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.DANCING);
                l_newMistakes.Insert(0, pa);
            }
            if (PoseAnalysis.m_handsFace)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.HANDS_NEAR_FACE);
                l_newMistakes.Insert(0, pa);
            }
            if (PoseAnalysis.m_noHands)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.LEFTHANDNOTVISIBLE);
                l_newMistakes.Insert(0, pa);
            }
        }

        #endregion


        #region analysisCycle
        

        public void AnalyseRules()
        {
            logGoodStuff();

            if (checkTimeToGiveFeedback() == true)
            {
                bool didIGiveFeedback = false;
                updateMistakeListL();

                //give correction when no mistakes and previous mistake is not NOMISTAKE
                if (l_mistakesL.Count == 0 && previousAction.myMistake != PresentationAction.MistakeType.NOMISTAKE)
                {
                    doGoodEventStuff();

                   // parent.sendFeedback(PresentationAction.MistakeType.NOMISTAKE);
                    //put previous mistake to no Mistake
                    //start timer
                }
                if(l_mistakesL.Count>0)
                {
                    if (((PresentationAction)l_mistakesL[0]).myMistake != previousAction.myMistake &&
                                 previousAction.myMistake != PresentationAction.MistakeType.NOMISTAKE)
                    {
                        doGoodEventStuff();
                       // parent.sendFeedback(PresentationAction.MistakeType.NOMISTAKE);
                        // mistakes.Clear();
                        didIGiveFeedback = true;
                    }
                    if (didIGiveFeedback == false)
                    {
                        doFeedbackEventStuff();
                       // parent.sendFeedback(((PresentationAction)mistakes[0]).myMistake);

                    }
                }

            }

        }

        #region sendFeedbacks

        private void doGoodEventStuff()
        {

            correctionEvent(this, previousAction);

            previousAction.myMistake = PresentationAction.MistakeType.NOMISTAKE;
            l_newMistakes.Clear();

           
        }

        private void doFeedbackEventStuff()
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            ((PresentationAction)l_mistakesL[0]).timeFinished = currentTime;


            if (previousAction.myMistake != ((PresentationAction)l_mistakesL[0]).myMistake )
            {
                ((PresentationAction)l_mistakesL[0]).setMistakeDefaults();
               
                feedBackEvent(this, (PresentationAction)l_mistakesL[0]);

                previousAction = (PresentationAction)l_mistakesL[0];
  

            }
           

        }
        #endregion

        public bool checkTimeToGiveFeedback()
        {
            bool giveFeedback = false;

            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            if (currentTime - lastFeedbackTime > t_timeBetweenFeedbacks)
            {
                giveFeedback = true;
            }

            return giveFeedback;
        }

        #endregion

        public void logGoodStuff()
        {
            if (PoseAnalysis.f_gestureStarted)
            {
                PracticeLogAction pa = new PracticeLogAction();
                pa.id = DateTime.Now.Millisecond;
                pa.mistake = false;
                pa.start = DateTime.Now - PracticeMode.practiceSession.start;
                pa.end = DateTime.Now.AddSeconds(1) - PracticeMode.practiceSession.start;
                pa.logAction = "BIG_GESTURE";
                PracticeMode.practiceSession.actions.Add(pa);
            }
            if(VolumeAnalysis.f_pausingGood)
            {
                PracticeLogAction pa = new PracticeLogAction();
                pa.id = DateTime.Now.Millisecond;
                pa.mistake = false;
                pa.start = DateTime.Now.AddSeconds(-2) - PracticeMode.practiceSession.start;
                pa.end = DateTime.Now.AddSeconds(1) - PracticeMode.practiceSession.start;
                pa.logAction = "GOOD_PAUSE";
                PracticeMode.practiceSession.actions.Add(pa);
                VolumeAnalysis.f_pausingGood = false;
                VolumeAnalysis.f_pausingGoodLogged = true;
            }
        }


    }
}
