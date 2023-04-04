using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023
{
    public class PeriodicMovements
    {
        int t_minMovementThreshold = 10;
      //  double t_averageTimeDiferenceThreshold = 200;
      //  int t_averageDistanceDiferenceThreshold = 5;
        double t_timeForAction = 4000;
        int t_swingsToTakeAction = 2;

        public string fired = "";


        List<PositionTimePair> maxX;
        List<PositionTimePair> minX;

        List<DisplacementTimePair> DispX;



        enum Direction { increase, decrease, none };
        Direction directionX;

        //float currentMinX;
        //float currentMaxX;
        //float currentMinY;
        //float currentMaxY;
        //float currentMinZ;
        //float currentMaxZ;
        //int maxDisplacement;
        //int minDislpacement;
        //long maxMovementTime;
        //bool isMoving = false;
        public int hipNew;
        public int hipOld;
        double startTime;
        double time;
        //int currentCycleX = 0;
        //int currentCycleY = 0;
        //int currentCycleZ = 0;

        public bool result = false;

        public PeriodicMovements()
        {
            directionX = Direction.none;

            maxX = new List<PositionTimePair>();

            minX = new List<PositionTimePair>();
            DispX = new List<DisplacementTimePair>();


            startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        }
        public bool checPeriodicMovements(int HipX, int HipXL)
        {

            t_minMovementThreshold = Math.Abs(HipX - HipXL)/6;
            time = DateTime.Now.TimeOfDay.TotalMilliseconds - startTime;
        
            if (time >= t_timeForAction)
            {
                result = checkAnalisys();
                resetValues();
                startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            }
            else
            {
                hipNew = HipX;
                if(Math.Abs(hipNew-hipOld)>t_minMovementThreshold)
                {
                    
                    storeValues();
                    hipOld = hipNew;
                }
            }

            return result;
        }
        bool checkAnalisys()
        {
            bool result = false;
            int countX = DispX.Count;
            if (countX > 1)
            {
               

                float averageDistance = 0;
                for (int i = 0; i < countX; i++)
                {
                    averageDistance = averageDistance + DispX[i].displacement;
                }
                averageDistance = averageDistance / countX;

                if (averageDistance > t_minMovementThreshold && countX >= t_swingsToTakeAction)
                {
                    result = true;
                    fired = "X";
                }

            }
           

            return result;
        }
        void resetValues()
        {

            maxX = new List<PositionTimePair>();

            DispX.Clear();
            startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            directionX = Direction.none;

        }
        void storeValues()
        {
            checDirections();
        }
        public void checDirections()
        {
            checkDirectionX();
 


        }
        public void checkDirectionX()
        {
            if (hipNew >= hipOld)
            {
                if (directionX.Equals(Direction.decrease))
                {
                    minX.Add(new PositionTimePair(hipOld));
                    if (minX.Count == maxX.Count)
                    {
                        float max = Math.Abs(this.maxX[this.maxX.Count - 1].position);
                        float min = Math.Abs(this.minX[this.minX.Count - 1].position);
                        float displacement = Math.Abs(max - min);
                        if (displacement > t_minMovementThreshold)
                        {
                            DispX.Add(new DisplacementTimePair(displacement));
                        }

                    }
                }
                directionX = Direction.increase;
            }
            else
            {
                if (directionX.Equals(Direction.increase))
                {
                    maxX.Add(new PositionTimePair(hipOld));
                    if (minX.Count == maxX.Count)
                    {
                        float max = Math.Abs(this.maxX[this.maxX.Count - 1].position);
                        float min = Math.Abs(this.minX[this.minX.Count - 1].position);
                        float displacement = Math.Abs(max - min);
                        if (displacement > t_minMovementThreshold)
                        {
                            DispX.Add(new DisplacementTimePair(displacement));
                        }

                    }
                }
                directionX = Direction.decrease;
            }
        }
       
     }
    

    public class PositionTimePair
    {
        public float position;
        public DateTime time;
        public PositionTimePair(float position)
        {
            this.position = position;
            this.time = DateTime.Now;
        }

    }
    public class DisplacementTimePair
    {
        public float displacement;
        public DateTime time;
        public DisplacementTimePair(float displacement)
        {
            this.displacement = displacement;
            this.time = DateTime.Now;
        }
    }


    
}
