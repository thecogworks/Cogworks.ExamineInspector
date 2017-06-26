(function () {
    'use strict';

    angular
        .module('umbraco')
        .filter('compress', compress);

    function compress() {
        return compressFilter;

        function compressFilter(text) {
            return text.replace(/[\. ,:-]+/g, '');
        };
    }
})();