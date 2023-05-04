using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT2023.LogObjects
{
    public class FeedbackForSentences
    {
        public string Explanation;
        public string feedbackKeywords;
        public string iconSource;
        public string sentence;

        public FeedbackForSentences(string explanation, string feedbackKeywords, string iconSource, string sentence)
        {
            Explanation = explanation;
            this.feedbackKeywords = feedbackKeywords;
            this.iconSource = iconSource;
            this.sentence = sentence;
        }   
    }
}
