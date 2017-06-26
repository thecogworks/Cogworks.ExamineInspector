using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;

namespace Cogworks.ExamineInspector.Helpers
{
    public static class ExamineInspectorHelper
    {
        public static Type[] DefaultAnalyzers =
            {
                typeof (StandardAnalyzer),
                typeof (SimpleAnalyzer),
                typeof (StopAnalyzer),
                typeof (WhitespaceAnalyzer),
                typeof (KeywordAnalyzer)
            };

        public static List<string> GetAnalyzerKeys()
        {
            var keys = GetAnalyzers().Keys;
            var analyserKeys = new List<string>();

            foreach (var key in keys)
            {
                analyserKeys.Add(key.ToString());
            }

            return analyserKeys;
        }

        public static SortedList GetAnalyzers()
        {
            var type = typeof(Analyzer);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var analyzers = new SortedList();

            var types = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(type));

            var aTypes = types as Type[] ?? types.ToArray();

            if (!aTypes.Any())
            {
                foreach (var defaultAnalyzers in DefaultAnalyzers)
                {
                    analyzers[defaultAnalyzers.FullName] = defaultAnalyzers;
                }
            }
            else
            {
                foreach (var aType in aTypes)
                {
                    analyzers[aType.FullName] = aType;
                }
            }

            return analyzers;
        }

        public static int GetTermCount(IndexReader reader)
        {
            var totalCount = 0;
            var termEnum4TermCount = reader.Terms();

            while (termEnum4TermCount.Next())
            {
                totalCount++;
            }

            termEnum4TermCount.Close();

            return totalCount;
        }
    }
}