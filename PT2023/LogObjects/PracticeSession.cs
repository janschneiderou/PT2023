using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023.LogObjects
{
    public class PracticeSession
    {
        public DateTime start;
        public List<IdentifiedSentence> sentences;
        public List<PracticeLogAction> actions;
        public bool scriptVisible;

        public PracticeSession()
        {
            start = DateTime.Now;
            sentences = new List<IdentifiedSentence>();
            actions = new List<PracticeLogAction>();
        }

        
    }
}
