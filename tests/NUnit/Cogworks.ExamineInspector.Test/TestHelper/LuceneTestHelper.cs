using System;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Cogworks.ExamineInspector.Test.TestHelper
{
    public static class LuceneTestHelper
    {
        public static Directory BuildRamDirectory()
        {
            RAMDirectory directory = new RAMDirectory();

            Analyzer analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);

            String test = "la marianna la va in campagna......";
            String test2 = "Lorem Ipsum è un testo segnaposto .....";
            using (IndexWriter ixw = new IndexWriter(directory, analyzer))
            {
                Document document = new Document();

                document.Add(new Field("Id", test.GetHashCode().ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                document.Add(new Field("content", test, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
                ixw.AddDocument(document);

                document = new Document();
                document.Add(new Field("Id", test2.GetHashCode().ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                document.Add(new Field("content", test2, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
                ixw.AddDocument(document);
                ixw.Commit();
            }

            return directory;
        }
    }
}