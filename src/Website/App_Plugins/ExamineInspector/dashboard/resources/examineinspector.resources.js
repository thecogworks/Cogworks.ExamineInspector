(function () {
    'use strict';

    angular
        .module('umbraco.resources')
        .factory('ExamineInspector.Dashboard.Resources', ExamineInspectorDashboardResources);

    ExamineInspectorDashboardResources.$inject = ['$http'];

    function ExamineInspectorDashboardResources($http) {
        var API_ROOT = '/umbraco/backoffice/examineInspector/examineInspectorApi/';

        var service = {
            getExamineIndexes: getExamineIndexes,
            getSummaryForIndex: getSummaryForIndex,
            getTopTermsInIndex: getTopTermsInIndex,
            getDocumentFromIndex: getDocumentFromIndex,
            getAllAnalysers: getAllAnalysers,
            search: search,
            analyse: analyse
        };

        return service;

        function getExamineIndexes() {
            return $http.get(API_ROOT + 'GetAllExamineIndexes');
        }

        function getSummaryForIndex(indexPath) {
            return $http.get(API_ROOT + 'GetIndexSummary/?indexPath=' + indexPath);
        }

        function getTopTermsInIndex(indexPath, noOfTerms, fields) {
            return $http.get(API_ROOT + 'GetTopTermsInIndex?noOfTerms=' + noOfTerms + '&indexPath=' + indexPath + '&fields=' + fields);
        }

        function getDocumentFromIndex(indexPath, docId) {
            return $http.get(API_ROOT + 'GetDocument?indexPath=' + indexPath + '&docId=' + docId);
        }

        function getAllAnalysers() {
            return $http.get(API_ROOT + 'GetAllAnalysers');
        }

        function search(indexPath, selectedAnalyzer, query, defaultField) {
            return $http.get(API_ROOT + 'Search?indexPath=' + indexPath + '&selectedAnalyzer=' + selectedAnalyzer + '&query=' + query + '&defaultField=' + defaultField);
        }

        function analyse(analyser, textToAnalyse) {
            return $http.get(API_ROOT + 'Analyse?analyser=' + analyser + '&textToAnalyse=' + textToAnalyse);
        }
    }
})();