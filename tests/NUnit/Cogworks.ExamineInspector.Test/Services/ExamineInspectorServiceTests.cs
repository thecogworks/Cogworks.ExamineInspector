using System;
using System.Collections.Generic;
using System.Linq;
using Cogworks.ExamineInspector.Model;
using Cogworks.ExamineInspector.Services;
using Cogworks.ExamineInspector.Test.TestHelper;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NUnit.Framework;

namespace Cogworks.ExamineInspector.Test.Services
{
    [TestFixture]
    public class ExamineInspectorServiceTests
    {
        private string _indexPath;
        private IndexReader _reader;
        private Directory _ramdirectory;

        private IExamineInspectorService _ExamineInspectorServiceEmptyConstructor;
        private IExamineInspectorService _ExamineInspectorServiceUnitTestConstructor;
        private IExamineInspectorService _ExamineInspectorService;

        [OneTimeSetUp]
        public void TestSetup()
        {
            //_indexPath = @"C:\Projects\Cogworks\ExamineInspector\Source\ExamineInspector.Web\App_Data\TEMP\ExamineIndexes\External\Index";
            _indexPath = @"trololo";
            _ramdirectory = LuceneTestHelper.BuildRamDirectory();
            _reader = IndexReader.Open(_ramdirectory, true);

            _ExamineInspectorServiceEmptyConstructor = new ExamineInspectorService();
            _ExamineInspectorServiceUnitTestConstructor = new ExamineInspectorService(_reader, _indexPath);
            //_ExamineInspectorService = new ExamineInspectorService(_indexPath);
        }

        [OneTimeTearDown]
        public void TestTearDown()
        {
            _ramdirectory.Dispose();
            _reader.Dispose();
        }

        #region GetIndexSummary

        [Test]
        public void Given_Empty_IndexPath_Expect_Exception_To_Be_Thrown_When_GetIndexSummary_Gets_Call()
        {
            Assert.Throws<Exception>(() => _ExamineInspectorServiceEmptyConstructor.GetIndexSummary());
        }

        [Test]
        public void Given_Valid_LuceneDirectory_IndexSummaryObject_Is_Returned_When_GetSummaryIsCalled()
        {
            var res = _ExamineInspectorServiceUnitTestConstructor.GetIndexSummary();

            Assert.AreEqual(typeof(LuceneIndexSummary), res.GetType());
        }

        #endregion GetIndexSummary

        #region GetAllExamineIndexes

        public void Ensure_GetAllExamineIndexes_Return_Right_Type()
        {
            var res = _ExamineInspectorServiceEmptyConstructor.GetAllExamineIndexes();
            Assert.AreEqual(typeof(IEnumerable<ExamineIndex>), res.GetType());
        }

        #endregion GetAllExamineIndexes

        #region GetHighFrequencyTerms

        [Test]
        public void Given_Valid_LuceneDirectory_And_Field_Expect_TermList_When_GetHighFrequencyTerms_Called()
        {
            var terms = _ExamineInspectorServiceUnitTestConstructor.GetHighFrequencyTerms(5, new[] { "content" });

            Assert.NotZero(terms.Count());
        }

        [Test]
        public void Given_Valid_LuceneDirectory_And_Field_Except_TermIpsum_When_GetHighFrequencyTerms_Called()
        {
            var terms = _ExamineInspectorServiceUnitTestConstructor.GetHighFrequencyTerms(5, new[] { "content" });
            var term = terms.FirstOrDefault(x => x.Term.Text().Contains("ipsum"));

            Assert.NotNull(term);
        }

        #endregion GetHighFrequencyTerms
    }
}