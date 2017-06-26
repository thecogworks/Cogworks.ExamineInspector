using Cogworks.ExamineInspector.Model;
using Lucene.Net.Util;

namespace Cogworks.ExamineInspector.Helpers
{
    internal sealed class TermInfoQueue : PriorityQueue
    {
        public TermInfoQueue(int size)
        {
            Initialize(size);
        }

        public override bool LessThan(object a, object b)
        {
            var termInfoA = a as TermInfo;
            var termInfoB = b as TermInfo;

            if (termInfoA == null || termInfoB == null) { return false; }

            return termInfoA.DocFreq < termInfoB.DocFreq;
        }
    }
}