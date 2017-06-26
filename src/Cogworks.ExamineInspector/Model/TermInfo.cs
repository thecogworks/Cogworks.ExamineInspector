using Lucene.Net.Index;

namespace Cogworks.ExamineInspector.Model
{
    public class TermInfo
    {
        public TermInfo(Term term, int docFrequency)
        {
            Term = term;
            DocFreq = docFrequency;
        }

        public int DocFreq { set; get; }

        public Term Term { set; get; }
    }
}