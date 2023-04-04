using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023
{
    public class CalcArmsMovement2D
    {
        public enum Gesture { nogesture, small, medium, big };//TODO
        public  static Gesture currentGesture;
        public Gesture previousGesture;
        public Gesture prePreviousGesture;

        public double MinAngleLeftArmShoulderLineA = 0;
        public double MinAngleLeftArmShoulderLineB = 0;


        public double MinAngleRightArmShoulderLineA = 0;
        public double MinAngleRightArmShoulderLineB = 0;



        public double MaxAngleLeftArmShoulderLineA = 0;
        public double MaxAngleLeftArmShoulderLineB = 0;


        public double MaxAngleRightArmShoulderLineA = 0;
        public double MaxAngleRightArmShoulderLineB = 0;



        public double MinAngleLeftForearmLeftArmA = 0;
        public double MinAngleLeftForearmLeftArmB = 0;


        public double MinAngleRightForearmRightArmA = 0;
        public double MinAngleRightForearmRightArmB = 0;
 

        public double MaxAngleLeftForearmLeftArmA = 0;
        public double MaxAngleLeftForearmLeftArmB = 0;


        public double MaxAngleRightForearmRightArmA = 0;
        public double MaxAngleRightForearmRightArmB = 0;


        bool GrowingAngleLeftArmShoulderLineA = true;
       // bool GrowingAngleLeftArmShoulderLineB = true;


        bool GrowingAngleRightArmShoulderLineA = true;
       // bool GrowingAngleRightArmShoulderLineB = true;


        bool GrowingAngleLeftForearmLeftArmA = true;
       // bool GrowingAngleLeftForearmLeftArmB = true;


        bool GrowingAngleRightForearmRightArmA = true;
       // bool GrowingAngleRightForearmRightArmB = true;


        public int SwingAngleLeftArmShoulderLineA = 0;
        public int SwingAngleLeftArmShoulderLineB = 0;


        public int SwingAngleRightArmShoulderLineA = 0;
        public int SwingAngleRightArmShoulderLineB = 0;


        public int SwingAngleLeftForearmLeftArmA = 0;
        public int SwingAngleLeftForearmLeftArmB = 0;
  

        public int SwingAngleRightForearmRightArmA = 0;
        public int SwingAngleRightForearmRightArmB = 0;
    


        public double leftArmAngleChange = 0;
        public double rightArmAngleChange = 0;
        public double gestureSize = 0;

        double preAngLeftUpArm;
        double preAngRightUpArm;
        double preAngLeftLowArm;
        double preAngRightLowArm;

        public CalcArmsMovement2D()
        {

        }

        /// <summary>
        /// Call when after a gesture was found and you want to reset to start analysing when a new gesture happens
        /// </summary>
        public void resetMaxAndMin()
        {
            setPreviousAngles();
            currentGesture = Gesture.nogesture;

            MinAngleLeftArmShoulderLineA = PoseAnalysis.angleftUpArm;
           // MinAngleLeftArmShoulderLineB = parent.bfpa.prevAngleLeftArmShoulderLineB;
       

            MinAngleRightArmShoulderLineA = PoseAnalysis.angRightUpArm;
           // MinAngleRightArmShoulderLineB = parent.bfpa.angleRightArmShoulderLineB;
   

            MaxAngleLeftArmShoulderLineA = PoseAnalysis.angleftUpArm; 
           // MaxAngleLeftArmShoulderLineB = parent.bfpa.angleRightArmShoulderLineB;
    

            MaxAngleRightArmShoulderLineA = PoseAnalysis.angRightUpArm;
          //  MaxAngleRightArmShoulderLineB = parent.bfpa.prevAngleLeftArmShoulderLineB;
       


            MinAngleLeftForearmLeftArmA = PoseAnalysis.angLeftLowArm;
           // MinAngleLeftForearmLeftArmB = parent.bfpa.angleLeftForearmLeftArmB;


            MinAngleRightForearmRightArmA = PoseAnalysis.angRightLowArm;
           // MinAngleRightForearmRightArmB = parent.bfpa.angleRightForearmRightArmB;
   

            MaxAngleLeftForearmLeftArmA = PoseAnalysis.angLeftLowArm;
          //  MaxAngleLeftForearmLeftArmB = parent.bfpa.angleLeftForearmLeftArmB;


            MaxAngleRightForearmRightArmA = PoseAnalysis.angRightLowArm;
          //  MaxAngleRightForearmRightArmB = parent.bfpa.angleRightForearmRightArmB;



            GrowingAngleLeftArmShoulderLineA = true;
          //  GrowingAngleLeftArmShoulderLineB = true;


            GrowingAngleRightArmShoulderLineA = true;
           // GrowingAngleRightArmShoulderLineB = true;


            GrowingAngleLeftForearmLeftArmA = true;
           // GrowingAngleLeftForearmLeftArmB = true;


            GrowingAngleRightForearmRightArmA = true;
          //  GrowingAngleRightForearmRightArmB = true;
           // GrowingAngleRightForearmRightArmC = true;

            SwingAngleLeftArmShoulderLineA = 0;
            SwingAngleLeftArmShoulderLineB = 0;
           // SwingAngleLeftArmShoulderLineC = 0;

            SwingAngleRightArmShoulderLineA = 0;
            SwingAngleRightArmShoulderLineB = 0;
           // SwingAngleRightArmShoulderLineC = 0;

            SwingAngleLeftForearmLeftArmA = 0;
            SwingAngleLeftForearmLeftArmB = 0;
          //  SwingAngleLeftForearmLeftArmC = 0;

            SwingAngleRightForearmRightArmA = 0;
            SwingAngleRightForearmRightArmB = 0;
          //  SwingAngleRightForearmRightArmC = 0;
        }

        void setPreviousAngles()
        {
            preAngLeftUpArm = PoseAnalysis.angleftUpArm;

            preAngRightUpArm = PoseAnalysis.angRightUpArm;

            preAngLeftLowArm = PoseAnalysis.angLeftLowArm;

            preAngRightLowArm = PoseAnalysis.angRightLowArm;

        }

        public void calcArmMovements()
        {

            getGrowingValues();
            setMaxMinValues();
            calcCurrentGestureSize();
            setPreviousAngles();
 
            prePreviousGesture = previousGesture;
            previousGesture = currentGesture;

        }

        void getGrowingValues()
        {
            bool growingVariable;

            /////////// left shoulder
            growingVariable = calcIsGrowing(PoseAnalysis.angleftUpArm,
                preAngLeftUpArm);
            if (GrowingAngleLeftArmShoulderLineA != growingVariable)
            {
                GrowingAngleLeftArmShoulderLineA = growingVariable;
                if (MaxAngleLeftArmShoulderLineA - MinAngleLeftArmShoulderLineA > 10)
                {
                    SwingAngleLeftArmShoulderLineA++;
                }
                MaxAngleLeftArmShoulderLineA = PoseAnalysis.angleftUpArm;
                MinAngleLeftArmShoulderLineA = PoseAnalysis.angleftUpArm;
            }
           

            ///////////right shoulder
            growingVariable = calcIsGrowing(PoseAnalysis.angRightUpArm,
                preAngRightUpArm);
            if (GrowingAngleRightArmShoulderLineA != growingVariable)
            {
                GrowingAngleRightArmShoulderLineA = growingVariable;
                if (MaxAngleRightArmShoulderLineA - MinAngleRightArmShoulderLineA > 10)
                {
                    SwingAngleRightArmShoulderLineA++;
                }
                MaxAngleRightArmShoulderLineA = PoseAnalysis.angRightUpArm;
                MinAngleRightArmShoulderLineA = PoseAnalysis.angRightUpArm;
            }
            

            ///////////left forearm
            growingVariable = calcIsGrowing(PoseAnalysis.angLeftLowArm,
               preAngLeftLowArm);
            if (GrowingAngleLeftForearmLeftArmA != growingVariable)
            {
                GrowingAngleLeftForearmLeftArmA = growingVariable;
                if (MaxAngleLeftForearmLeftArmA - MinAngleLeftForearmLeftArmA > 10)
                {
                    SwingAngleLeftForearmLeftArmA++;
                }
                MaxAngleLeftForearmLeftArmA = PoseAnalysis.angLeftLowArm;
                MinAngleLeftForearmLeftArmA = PoseAnalysis.angLeftLowArm;
            }
          

            /////////// right forearm
            growingVariable = calcIsGrowing(PoseAnalysis.angRightLowArm,
              preAngRightLowArm);
            if (GrowingAngleRightForearmRightArmA != growingVariable)
            {
                GrowingAngleRightForearmRightArmA = growingVariable;
                if (MaxAngleRightForearmRightArmA - MinAngleRightForearmRightArmA > 10)
                {
                    SwingAngleRightForearmRightArmA++;
                }
                MaxAngleRightForearmRightArmA = PoseAnalysis.angRightLowArm;
                MinAngleRightForearmRightArmA = PoseAnalysis.angRightLowArm;
            }
           

        }

        void setMaxMinValues()
        {
            //////// Left shoulder
            if ( PoseAnalysis.angleftUpArm < MinAngleLeftArmShoulderLineA)
            {

                MinAngleLeftArmShoulderLineA = PoseAnalysis.angleftUpArm;
            }
            if (PoseAnalysis.angleftUpArm > MaxAngleLeftArmShoulderLineA)
            {
                MaxAngleLeftArmShoulderLineA = PoseAnalysis.angleftUpArm;
            }

  

            ////// Right Shoulder
            if (PoseAnalysis.angRightUpArm < MinAngleRightArmShoulderLineA)
            {
                MinAngleRightArmShoulderLineA = PoseAnalysis.angRightUpArm;
            }
            if (PoseAnalysis.angRightUpArm > MaxAngleRightArmShoulderLineA)
            {
                MaxAngleRightArmShoulderLineA = PoseAnalysis.angRightUpArm;
            }

           


            /////// LeftForearm
            if (PoseAnalysis.angLeftLowArm < MinAngleLeftForearmLeftArmA)
            {
                MinAngleLeftForearmLeftArmA = PoseAnalysis.angLeftLowArm;
            }
            if (PoseAnalysis.angLeftLowArm > MaxAngleLeftForearmLeftArmA)
            {
                MaxAngleLeftForearmLeftArmA = PoseAnalysis.angLeftLowArm;
            }



            ///////Right Forearm
            if (PoseAnalysis.angRightLowArm < MinAngleRightForearmRightArmA)
            {
                MinAngleRightForearmRightArmA = PoseAnalysis.angRightLowArm;
            }
            if (PoseAnalysis.angRightLowArm > MaxAngleRightForearmRightArmA)
            {
                MaxAngleRightForearmRightArmA = PoseAnalysis.angRightLowArm;
            }

          

        }



        void calcCurrentGestureSize()
        {



            ///leftArm
            leftArmAngleChange = Math.Abs(MaxAngleLeftArmShoulderLineA - MinAngleLeftArmShoulderLineA) +
                Math.Abs(MaxAngleLeftForearmLeftArmA - MinAngleLeftForearmLeftArmA);

            ///// Right Arm
            rightArmAngleChange = Math.Abs(MaxAngleRightArmShoulderLineA - MinAngleRightArmShoulderLineA) +
                Math.Abs(MaxAngleRightForearmRightArmA - MinAngleRightForearmRightArmA);

            if (rightArmAngleChange > leftArmAngleChange)
            {
                gestureSize = rightArmAngleChange;
            }
            else
            {
                gestureSize = leftArmAngleChange;
            }
            // gestureSize = rightArmAngleChange;


            if (gestureSize < 10)
            {
                currentGesture = Gesture.nogesture;
            }
            else if (gestureSize < 20)
            {
                currentGesture = Gesture.small;
            }
            else if (gestureSize < 30)
            {
                currentGesture = Gesture.medium;
            }
            else
            {
                currentGesture = Gesture.big;
            }

        }


        bool calcIsGrowing(double current, double previous)
        {
            bool isGrowing = true;
            if (current < previous)
            {
                isGrowing = false;
            }
            return isGrowing;
        }


    }
}
