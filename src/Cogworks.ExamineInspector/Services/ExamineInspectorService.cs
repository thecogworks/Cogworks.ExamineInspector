using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cogworks.ExamineInspector.Constants;
using Cogworks.ExamineInspector.Helpers;
using Cogworks.ExamineInspector.Model;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Umbraco.Core.Logging;
using LuceneVersion = Lucene.Net.Util.Version;

namespace Cogworks.ExamineInspector.Services
{
    public interface IExamineInspectorService : IDisposable
    {
        LuceneIndexSummary GetIndexSummary();

        IEnumerable<TermInfo> GetHighFrequencyTerms(int numTerms, string[] fields);

        Document GetDocument(int docId);

        IEnumerable<SearchResult> Search(string selectedAnalyzer, string query, string defaultField,
            out string parsedQuery);

        IEnumerable<ExamineIndex> GetAllExamineIndexes();

        IEnumerable<string> Analyse(string analyser, string textToAnalyse);
    }

    public class ExamineInspectorService : IExamineInspectorService
    {
        private readonly FSDirectory _directory;

        private readonly string _indexPath;

        private readonly IndexReader _reader;

        private readonly ISearcher _searcher;

        private readonly Analyzer _stdAnalyzer;

        private Analyzer _analyzer;

        private static readonly LuceneVersion LuceneVersion = LuceneVersion.LUCENE_29;

        private bool _disposed;

        /// <summary>
        /// used for unit testing
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="path"></param>
        public ExamineInspectorService(IndexReader reader, string path)
        {
            _indexPath = path;
            _reader = reader;
        }

        public ExamineInspectorService(string indexPath)
        {
            _indexPath = indexPath;
            _directory = FSDirectory.Open(new DirectoryInfo(_indexPath));
            _reader = IndexReader.Open(_directory, true);
            _stdAnalyzer = new StandardAnalyzer(LuceneVersion);
            _searcher = new LuceneSearcher(_directory, _stdAnalyzer);
        }

        public ExamineInspectorService()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Manual release of managed resources.
                    if (_directory != null)
                    {
                        _directory.Close();
                        _directory.Dispose();
                    }

                    if (_reader != null)
                    {
                        _reader.Close();
                        _reader.Dispose();
                    }
                }
                // Release unmanaged resources.
                _disposed = true;
            }
        }

        ~ExamineInspectorService() { Dispose(false); }

        public LuceneIndexSummary GetIndexSummary()
        {
            if (string.IsNullOrEmpty(_indexPath))
            {
                LogHelper.Info(GetType(), "_indexPath is empty");
                throw new Exception("Invalid index");
            }

            var summary = new LuceneIndexSummary
            {
                NoOfDocuments = _reader.MaxDoc(),
                NoOfTerms = ExamineInspectorHelper.GetTermCount(_reader),
                NumberOfFields = _reader.GetFieldNames(IndexReader.FieldOption.ALL).Count,
                Fields = _reader.GetFieldNames(IndexReader.FieldOption.ALL),
                IndexFiles = GetIndexFiles()
            };

            return summary;
        }

        public IEnumerable<ExamineIndex> GetAllExamineIndexes()
        {
            var indexes = new List<ExamineIndex>();
            var examineIndexesCollection = ExamineManager.Instance.IndexProviderCollection;

            foreach (LuceneIndexer examineItem in examineIndexesCollection)
            {
                var i = new ExamineIndex { Name = examineItem.Name, IndexPath = examineItem.LuceneIndexFolder.FullName };
                indexes.Add(i);
            }

            return indexes;
        }

        public IEnumerable<string> Analyse(string analyser, string textToAnalyse)
        {
            var analysers = ExamineInspectorHelper.GetAnalyzers();
            var analyserObject = (Analyzer)Activator.CreateInstance((Type)analysers[analyser]);

            var tokenList = new List<string>();
            var tokenStream = analyserObject.TokenStream("Analyze", new StringReader(textToAnalyse));
            var token = tokenStream.Next();

            while (token != null)
            {
                tokenList.Add(token.TermText());
                token = tokenStream.Next();
            }

            return tokenList;
        }

        public IEnumerable<TermInfo> GetHighFrequencyTerms(int numTerms, string[] fields)
        {
            if (_reader == null || fields == null)
            {
                return new TermInfo[0];
            }

            var termInfoQueue = new TermInfoQueue(numTerms);

            var terms = _reader.Terms();

            while (terms.Next())
            {
                var term = terms.Term();
                if (termInfoQueue.Size() >= numTerms) { break; }

                if (fields.Length > 0)
                {
                    // The lamda expresion is testing to see if the field that belongs to the term from lucene
                    // is in the list of fields we passed in
                    var skipField = fields.All(field => !term.Field().Equals(field));
                    if (skipField) { continue; }

                    if (terms.DocFreq() > 0)
                    {
                        termInfoQueue.Add(new TermInfo(term, terms.DocFreq()));
                    }
                }
            }

            var res = BuildTermInfoList(termInfoQueue);
            return res;
        }

        private static IEnumerable<TermInfo> BuildTermInfoList(PriorityQueue termInfoQueue)
        {
            var res = new List<TermInfo>();
            var termInfoQueueSize = termInfoQueue.Size();

            for (var i = 0; i < termInfoQueueSize; i++)
            {
                res.Add((TermInfo)termInfoQueue.Pop());
            }

            return res;
        }

        public Document GetDocument(int docId)
        {
            var doc = _reader.Document(docId);
            return doc;
        }

        public IEnumerable<SearchResult> Search(string selectedAnalyzer, string query, string defaultField, out string parsedQuery)
        {
            var queryParser = CreateQueryParser(selectedAnalyzer, defaultField);
            var q = queryParser.Parse(query);
            parsedQuery = q.ToString();

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.RawQuery(parsedQuery);
            var results = _searcher.Search(filter);
            var fieldKeys = new List<string>();

            for (var i = 0; i < results.Count(); i++)
            {
                var searchResult = results.ElementAt(i);
                var docId = searchResult.DocId;

                searchResult.Fields.Add("docId", docId.ToString());
                searchResult.Fields.Add("score", searchResult.Score.ToString());
                fieldKeys.AddRange(searchResult.Fields.Select(field => field.Key).Except(fieldKeys));
            }

            foreach (var searchResult in results)
            {
                foreach (var fieldKey in fieldKeys)
                {
                    var hasKey = searchResult.Fields.Keys.Contains(fieldKey);
                    if (!hasKey)
                    {
                        searchResult.Fields.Add(fieldKey, "NULL");
                    }
                }
            }

            return results.Skip(0);
        }

        private QueryParser CreateQueryParser(string analyzerName, string defField)
        {
            if (string.IsNullOrWhiteSpace(analyzerName)) { analyzerName = ExamineInspectorConstants.StandardAnalyzerNameSpace; }

            _analyzer = AnalyzerFromName(analyzerName) ?? _stdAnalyzer;

            var queryParser = new QueryParser(LuceneVersion, defField, _analyzer);

            return queryParser;
        }

        private static Analyzer AnalyzerFromName(string analyzerName)
        {
            var analyzerType = (Type)ExamineInspectorHelper.GetAnalyzers()[analyzerName];

            if (analyzerType == null)
            {
                var analyzer = Assembly.GetAssembly(typeof(Analyzer));
                analyzerType = analyzer.GetType(analyzerName);
            }

            return (Analyzer)Activator.CreateInstance(analyzerType);
        }

        private IEnumerable<IndexFile> GetIndexFiles()
        {
            var indexFiles = new List<IndexFile>();

            if (_directory != null)
            {
                var files = _directory.ListAll();

                foreach (var file in files)
                {
                    var indexfile = new IndexFile { FileName = file };
                    var fileLength = _directory.FileLength(file);

                    indexfile.FileSize = fileLength;
                    indexFiles.Add(indexfile);
                }
            }

            return indexFiles;
        }
    }
}