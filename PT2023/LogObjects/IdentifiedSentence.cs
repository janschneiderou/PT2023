using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023.LogObjects
{
    public class IdentifiedSentence
    {
        public bool wasIdentified;
        public string sentence;
        public TimeSpan end;
        public TimeSpan start;
        public IdentifiedSentence(string sentence )
        {
            this.sentence = sentence;
            wasIdentified = false;

        }
    }
}
