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
        public string videoId;
        public List<IdentifiedSentence> sentences;
        public List<PracticeLogAction> actions;
        public bool scriptVisible;
        public DateTime end;

        public PracticeSession()
        {
            start = DateTime.Now;
            videoId = start.Minute+""+start.Second +"" + start.Millisecond + "";
            sentences = new List<IdentifiedSentence>();
            actions = new List<PracticeLogAction>();
        }

        
    }
}
