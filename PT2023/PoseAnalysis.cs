using PT2023.LogObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023
{
    public class PoseAnalysis
    {
        Keypoint[] m_keypoints;

        #region Thresholds
        public static int t_handSize = 10;
        public static double t_posture=500;
        public static double t_gesture=3000;
        public static int t_sentencesWithoutGestures = 3;
        public static int t_dances=4;
        #endregion

        #region mistakes
        public static bool m_CrossedArms = false;
        public static bool m_CrossedLegs = false;
        public static bool m_dancing = false;
        public static bool m_noGestures = false;
        public static bool m_handsFace = false;
        public static bool m_noHands = false;

        #endregion

        #region flags

        public static bool f_CrossedArms = true;
        public static bool f_CrossedLegs = true;
        public static bool f_dancing = true;
        public static bool f_noGestures = true;
        public static bool f_handsFace = true;
        public static bool f_noHands = true;
        public static bool f_gestureStarted = false;
        #endregion


        #region angles
        public static double angLeftLowArm;
        public static double angRightLowArm;
        public static double angleftUpArm;
        public static double angRightUpArm;
        #endregion

        #region helpers
        double currentTime;
        double startCrossedArms;
        double startCrossedLegs;
        double startDancing;
        double startGestures;
        double startHandsFace;
        double startNoHands;
        int startSpeakingCounter;
        bool startedSpeaking = false;

        bool possibleCrossedArms=false;
        bool possibleCrossedLegs = false;
        bool possibleDanding = false;
        bool possibleGestures = false;
        bool possibleHandsFace = false;
        bool possibleNoHands = false;

        bool gestureReseted = true;

        double timeBetweenGestures = 0;
        double timeLastGesture = 0;

        public CalcArmsMovement2D calcArmsMovement2;
        public PeriodicMovements periodicMovements;

        #endregion

        public PoseAnalysis()
        {
            calcArmsMovement2 = new CalcArmsMovement2D();
            periodicMovements = new PeriodicMovements();
        }

        public void analysePose(Keypoint[] keypoints)
        {
            m_keypoints = keypoints;
            currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            calcArmAngles();
            clearMistakes();
           
            if(f_CrossedArms)
            {
                analyseCrossedArms();
            }
            if(f_CrossedLegs)
            {
                analyseCrossedLegs();
            }
            if(f_dancing)
            {
                analyseDancing();
            }
            if(f_handsFace)
            {
                analyseHandsCloseToFace();
            }
            if(f_noHands)
            {
                analyseNoHands();
            }
            if(f_noGestures)
            {
                analyseGestureHappen();
            }
               
           
        }

        public void clearMistakes()
        {
            m_CrossedArms = false;
            m_CrossedLegs = false;
            m_dancing = false;
            m_noGestures = false;
            m_handsFace = false;
            m_noHands = false;
        }

        public void analyseCrossedArms()
        {

            if (m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.X < m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.X )
            {
                if (m_keypoints[(int)BodyParts.LEFT_WRIST].position.X > m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.X &&
                    m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X < m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.X &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y < m_keypoints[(int)BodyParts.LEFT_HIP].position.Y &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y > m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y < m_keypoints[(int)BodyParts.RIGHT_HIP].position.Y &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y > m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y
                && possibleCrossedArms == false)
                {
                    startCrossedArms = currentTime;
                    possibleCrossedArms = true;
                }
                else if (m_keypoints[(int)BodyParts.LEFT_WRIST].position.X > m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.X &&
                    m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X < m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.X &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y < m_keypoints[(int)BodyParts.LEFT_HIP].position.Y &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y > m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y < m_keypoints[(int)BodyParts.RIGHT_HIP].position.Y &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y > m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y
                    && possibleCrossedArms == true && currentTime - startCrossedArms > t_posture)
                {
                    m_CrossedArms = true;
                }
                else if (m_keypoints[(int)BodyParts.LEFT_WRIST].position.X < m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.X 
                    || m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X > m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.X)
                {
                    m_CrossedArms = false;
                    possibleCrossedArms = false;

                }
            }
            else
            {
                if (m_keypoints[(int)BodyParts.LEFT_WRIST].position.X < m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.X &&
                     m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X > m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.X &&
                 m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y < m_keypoints[(int)BodyParts.LEFT_HIP].position.Y &&
                 m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y > m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y &&
                 m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y < m_keypoints[(int)BodyParts.RIGHT_HIP].position.Y &&
                 m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y > m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y
                 && possibleCrossedArms == false)
                {
                    startCrossedArms = currentTime;
                    possibleCrossedArms = true;
                }
                else if (m_keypoints[(int)BodyParts.LEFT_WRIST].position.X < m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.X &&
                    m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X > m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.X &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y < m_keypoints[(int)BodyParts.LEFT_HIP].position.Y &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y > m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y < m_keypoints[(int)BodyParts.RIGHT_HIP].position.Y &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y > m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y
                    && possibleCrossedArms == true && currentTime - startCrossedArms > t_posture)
                {
                    m_CrossedArms = true;
                }
                else if (m_keypoints[(int)BodyParts.LEFT_WRIST].position.X > m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.X
                    || m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X < m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.X)
                {
                    m_CrossedArms = false;
                    possibleCrossedArms = false;

                }
            }

            
        }
        public void analyseCrossedLegs()
        {
            if (m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.X < m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.X)
            {
                if (m_keypoints[(int)BodyParts.LEFT_ANKLE].position.X > m_keypoints[(int)BodyParts.RIGHT_ANKLE].position.X &&
                possibleCrossedLegs == false && m_keypoints[(int)BodyParts.LEFT_ANKLE].position_raw.IsEmpty == false)
                {
                    startCrossedLegs = currentTime;
                    possibleCrossedLegs = true;
                }
                else if (m_keypoints[(int)BodyParts.LEFT_ANKLE].position.X > m_keypoints[(int)BodyParts.RIGHT_ANKLE].position.X &&
                    possibleCrossedLegs == true && currentTime - startCrossedLegs > t_posture && m_keypoints[(int)BodyParts.LEFT_ANKLE].position_raw.IsEmpty == false)
                {
                    m_CrossedLegs = true;
                }
                else if (m_keypoints[(int)BodyParts.LEFT_ANKLE].position.X < m_keypoints[(int)BodyParts.RIGHT_ANKLE].position.X && m_keypoints[(int)BodyParts.LEFT_ANKLE].position_raw.IsEmpty == false)
                {
                    m_CrossedLegs = false;
                    possibleCrossedLegs = false;
                }
            }
            else
            {
                if (m_keypoints[(int)BodyParts.LEFT_ANKLE].position.X < m_keypoints[(int)BodyParts.RIGHT_ANKLE].position.X &&
                possibleCrossedLegs == false && m_keypoints[(int)BodyParts.LEFT_ANKLE].position_raw.IsEmpty == false)
                {
                    startCrossedLegs = currentTime;
                    possibleCrossedLegs = true;
                }
                else if (m_keypoints[(int)BodyParts.LEFT_ANKLE].position.X < m_keypoints[(int)BodyParts.RIGHT_ANKLE].position.X &&
                    possibleCrossedLegs == true && currentTime - startCrossedLegs > t_posture && m_keypoints[(int)BodyParts.LEFT_ANKLE].position_raw.IsEmpty == false)
                {
                    m_CrossedLegs = true;
                }
                else if (m_keypoints[(int)BodyParts.LEFT_ANKLE].position.X > m_keypoints[(int)BodyParts.RIGHT_ANKLE].position.X && m_keypoints[(int)BodyParts.LEFT_ANKLE].position_raw.IsEmpty == false)
                {
                    m_CrossedLegs = false;
                    possibleCrossedLegs = false;
                }
            }

            
        }
        public void analyseNoHands()
        {
            if ((m_keypoints[(int)BodyParts.LEFT_WRIST].position.X == 0 || m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X == 0) &&
               possibleNoHands == false)
            {
                startNoHands = currentTime;
                possibleNoHands = true;
            }
            else if((m_keypoints[(int)BodyParts.LEFT_WRIST].position.X == 0 || m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X == 0) &&
               possibleNoHands == true && currentTime-startNoHands>t_posture)
            {
                m_noHands = true;
            }
            else if (m_keypoints[(int)BodyParts.LEFT_WRIST].position.X!=0 && m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X != 0)
            {
                m_noHands = false;
                possibleNoHands = false;
            }
        }
        public void analyseHandsCloseToFace()
        {
            if ((m_keypoints[(int)BodyParts.LEFT_WRIST].position.X  > m_keypoints[(int)BodyParts.LEFT_EAR].position.X &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.X < m_keypoints[(int)BodyParts.RIGHT_EAR].position.X &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y < m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y &&
                 m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y < m_keypoints[(int)BodyParts.LEFT_EAR].position.Y - 
                 (m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y- m_keypoints[(int)BodyParts.LEFT_EAR].position.Y)
                || m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X < m_keypoints[(int)BodyParts.RIGHT_EAR].position.X &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X > m_keypoints[(int)BodyParts.LEFT_EAR].position.X &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y < m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y > m_keypoints[(int)BodyParts.RIGHT_EAR].position.Y -
                (m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y - m_keypoints[(int)BodyParts.RIGHT_EAR].position.Y)) &&
               possibleHandsFace == false)
            {
                startHandsFace = currentTime;
                possibleHandsFace = true;
            }
            else if ((m_keypoints[(int)BodyParts.LEFT_WRIST].position.X > m_keypoints[(int)BodyParts.LEFT_EAR].position.X &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.X < m_keypoints[(int)BodyParts.RIGHT_EAR].position.X &&
                m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y < m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y &&
                 m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y < m_keypoints[(int)BodyParts.LEFT_EAR].position.Y -
                 (m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y - m_keypoints[(int)BodyParts.LEFT_EAR].position.Y)
                || m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X < m_keypoints[(int)BodyParts.RIGHT_EAR].position.X &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X > m_keypoints[(int)BodyParts.LEFT_EAR].position.X &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y < m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y > m_keypoints[(int)BodyParts.RIGHT_EAR].position.Y -
                (m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y - m_keypoints[(int)BodyParts.RIGHT_EAR].position.Y)) &&
               possibleHandsFace == true && currentTime-startHandsFace>t_posture)
            {
                m_handsFace = true;
            }
            else if (m_keypoints[(int)BodyParts.LEFT_WRIST].position.X < m_keypoints[(int)BodyParts.LEFT_EAR].position.X &&
                m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X < m_keypoints[(int)BodyParts.RIGHT_EAR].position.X &&
                 m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y > m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y &&
                 m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y > m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y )
            {
                m_handsFace = false;
                possibleHandsFace = false;
            }

        }
        public void analyseGestureHappen()
        {
            calcArmsMovement2.calcArmMovements();
            if (VolumeAnalysis.isSpeaking && startedSpeaking == false)
            {
                startedSpeaking = true;
                startGestures = currentTime;
                startSpeakingCounter++;
                gestureReseted = true;

            }
            else if (VolumeAnalysis.isSpeaking)
            {
                if(CalcArmsMovement2D.currentGesture == CalcArmsMovement2D.Gesture.nogesture)
                {
                    
                    if((startSpeakingCounter> t_sentencesWithoutGestures && calcArmsMovement2.prePreviousGesture == CalcArmsMovement2D.Gesture.nogesture) 
                        || currentTime - startGestures > t_gesture)
                    {
                        m_noGestures = true;
                    }
                    f_gestureStarted = false;


                }
                else
                {
                    if(CalcArmsMovement2D.currentGesture == CalcArmsMovement2D.Gesture.big)
                    {
                        if (gestureReseted)
                        {
                            f_gestureStarted = true;
                            gestureReseted = false;
                        }
                        else
                        {
                            f_gestureStarted = false;
                        }
                    }
                    else
                    {
                        f_gestureStarted = false;
                    }
                    
                    m_noGestures = false;
                    startSpeakingCounter = 0;
                    calcArmsMovement2.resetMaxAndMin();
                }
              
                


            }
            else
            {
                startedSpeaking = false;
                f_gestureStarted = false;

            }

        }

        public void analyseDancing()
        {
            if (periodicMovements.checPeriodicMovements(m_keypoints[(int)BodyParts.LEFT_HIP].position.X, m_keypoints[(int)BodyParts.RIGHT_HIP].position.X))
            {
                startDancing = currentTime;
                m_dancing = true;
            }
            else
            {
                if(currentTime - startDancing > t_dances)
                {
                    m_dancing = false;
                }
            }
        }

        public void calcArmAngles()
        {
            double upArmLineX = m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.X - m_keypoints[(int)BodyParts.LEFT_ELBOW].position.X;
            double upArmLineY = m_keypoints[(int)BodyParts.LEFT_SHOULDER].position.Y - m_keypoints[(int)BodyParts.LEFT_ELBOW].position.Y;

            angleftUpArm = Math.Atan(upArmLineX / upArmLineY) * 180 / Math.PI;

            upArmLineX = m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.X - m_keypoints[(int)BodyParts.RIGHT_ELBOW].position.X;
            upArmLineY = m_keypoints[(int)BodyParts.RIGHT_SHOULDER].position.Y - m_keypoints[(int)BodyParts.RIGHT_ELBOW].position.Y;

            angRightUpArm = Math.Atan(upArmLineX / upArmLineY) * 180 / Math.PI;

            double lowArmLineX = m_keypoints[(int)BodyParts.LEFT_WRIST].position.X - m_keypoints[(int)BodyParts.LEFT_ELBOW].position.X;
            double lowArmLineY = m_keypoints[(int)BodyParts.LEFT_WRIST].position.Y - m_keypoints[(int)BodyParts.LEFT_ELBOW].position.Y;

            angLeftLowArm = Math.Atan(lowArmLineX / lowArmLineY) * 180 / Math.PI;

            lowArmLineX = m_keypoints[(int)BodyParts.RIGHT_WRIST].position.X - m_keypoints[(int)BodyParts.RIGHT_ELBOW].position.X;
            lowArmLineY = m_keypoints[(int)BodyParts.RIGHT_WRIST].position.Y - m_keypoints[(int)BodyParts.RIGHT_ELBOW].position.Y;

            angRightLowArm = Math.Atan(lowArmLineX / lowArmLineY) * 180 / Math.PI;

            //int x = 0;

        }
       

    }
}
