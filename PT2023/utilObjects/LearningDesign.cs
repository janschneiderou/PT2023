using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023.utilObjects
{
    public class LearningDesign
    {
        public List<LearningTask> Tasks;

        public enum TaskType
        {
            SLIDESELECTION,
            WRITESCRIPT,
            PRACTICEWITHSCRIPT,
            PRACTICEWITHOUTSCRIPT,
            REVIEWPRESENTATION,
            MEMORY
        };
        public LearningDesign()
        {
            loadTasks();
        }
        public void loadTasks()
        {
            Tasks = new List<LearningTask>();
            string path = System.IO.Path.Combine(UserManagement.presentationPath + "\\LearningTasks.json");
            if (!File.Exists(path))
            {
                createLearningDesign();
            }
        }

        //Default LearningDesign
        void createLearningDesign()
        {
            
            LearningTask task = new LearningTask();
            task.taskType = TaskType.SLIDESELECTION;
            task.description = "Before we start, \nmy recomendation is to \nadd the slides \nof the presentation \nthat you want to practice.";

            Tasks.Add(task);

            task = new LearningTask();
            task.taskType= TaskType.WRITESCRIPT;
            task.description = "I consider that writing a \nsrcript for your \npresentation is  \n a good practice. \nLet's do that";
            Tasks.Add(task);

            task = new LearningTask();
            task.taskType = TaskType.MEMORY;
            task.description = "Great! \nnow let's try to memorise \nthe script.";
            Tasks.Add(task);

            task = new LearningTask();
            task.taskType = TaskType.PRACTICEWITHSCRIPT;
            task.description = "By now, \nyou should be familar\nwith the script. \nLet's practice \nthe presentation.";
            Tasks.Add(task);

            task = new LearningTask();
            task.taskType = TaskType.REVIEWPRESENTATION;
            task.description = "You did it! \nNow let's see how \nit went.";
            Tasks.Add(task);

            task = new LearningTask();
            task.taskType = TaskType.PRACTICEWITHSCRIPT;
            task.description = "You already know \nwhat to improve \nLet's master it";
            Tasks.Add(task);

            task = new LearningTask();
            task.taskType = TaskType.PRACTICEWITHSCRIPT;
            task.description = "You know the saying... \nPractice makes perfect. \nlet's do it";
            Tasks.Add(task);

            task = new LearningTask();
            task.taskType = TaskType.PRACTICEWITHOUTSCRIPT;
            task.description = "Great! \none last time \nnow do it without \nany help \nun-check with script";
            Tasks.Add(task);

            task = new LearningTask();
            task.taskType = TaskType.REVIEWPRESENTATION;
            task.description = "Awesome work! \nLet's review it \nand see your progress.";
            Tasks.Add(task);
        }

    }
}
