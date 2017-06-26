using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Cogworks.ExamineInspector.Helpers;
using Cogworks.ExamineInspector.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Cogworks.ExamineInspector.Web.Controllers.Api
{
    [PluginController("examineInspector")]
    public class ExamineInspectorApiController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public HttpResponseMessage GetAllExamineIndexes()
        {
            try
            {
                using (var examineInspectorService = new ExamineInspectorService())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, examineInspectorService.GetAllExamineIndexes());
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [HttpGet]
        public HttpResponseMessage GetAllAnalysers()
        {
            try
            {
                var analyserKeys = ExamineInspectorHelper.GetAnalyzerKeys();
                return Request.CreateResponse(HttpStatusCode.OK, analyserKeys);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [HttpGet]
        public HttpResponseMessage Analyse(string analyser, string textToAnalyse)
        {
            try
            {
                using (var examineInspectorService = new ExamineInspectorService())
                {
                    return Request.CreateResponse(examineInspectorService.Analyse(analyser, textToAnalyse));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [HttpGet]
        public HttpResponseMessage GetIndexSummary(string indexPath)
        {
            try
            {
                using (var examineInspectorService = new ExamineInspectorService(indexPath))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, examineInspectorService.GetIndexSummary());
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [HttpGet]
        public HttpResponseMessage GetTopTermsInIndex(string indexPath, string fields, int noOfTerms)
        {
            try
            {
                using (var examineInspectorService = new ExamineInspectorService(indexPath))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, examineInspectorService.GetHighFrequencyTerms(noOfTerms, fields.Split(',')));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [HttpGet]
        public HttpResponseMessage GetDocument(string indexPath, int docId)
        {
            try
            {
                using (var examineInspectorService = new ExamineInspectorService(indexPath))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, examineInspectorService.GetDocument(docId));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [HttpGet]
        public HttpResponseMessage Search(string indexPath, string selectedAnalyzer, string query, string defaultField)
        {
            try
            {
                using (var examineInspectorService = new ExamineInspectorService(indexPath))
                {
                    var generatedQuery = string.Empty;
                    return
                        Request.CreateResponse(examineInspectorService.Search(selectedAnalyzer, query, defaultField,
                            out generatedQuery));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
    }
}