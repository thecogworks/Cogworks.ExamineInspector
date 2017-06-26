(function () {
    angular
        .module('umbraco')
        .controller('ExamineInspector.Dashboard.DashboardController', ExamineInspectorDashboardController);

    ExamineInspectorDashboardController.$inject = [
        '$scope',
        'formHelper',
        'notificationsService',
        'ExamineInspector.Dashboard.Resources'
    ];

    function ExamineInspectorDashboardController($scope, formHelper, notificationsService, examineInspectorDashboardResources) {
        var vm = this;
        vm.files = {};
        vm.files.selectedIndexTotalFileSize = {};

        /////////////////////////////Dashboard/////////////////////////////////
        vm.dashboard = {};
        vm.dashboard.indexes = {};
        vm.dashboard.selectedIndex = {};
        vm.dashboard.indexChanged = indexChanged;
        vm.dashboard.indexPicked = false;
        vm.dashboard.ready = false;
        vm.dashboard.setError = function (error) {
            console.log(error);
        };

        loadIndexes();

        function loadIndexes() {
            examineInspectorDashboardResources.getExamineIndexes()
                .then(function (response) {
                    vm.dashboard.indexes = response.data;
                    vm.dashboard.ready = true;
                },
                    function (error) {
                        vm.dashboard.setError(error);
                    }
                );
        }

        function indexChanged() {
            if (vm.dashboard.selectedIndex != null) {
                resetOverviewValues();
                getIndexSummary(vm.dashboard.selectedIndex.indexPath);
                vm.dashboard.indexPicked = true;
            } else {
                vm.dashboard.indexPicked = false;
            }
        };

        function resetOverviewValues() {
            vm.overview.haveFieldsToShow = false;
            vm.overview.selectAllCheckbox.checked = false;
            vm.overview.selectedFields = [];
            vm.overview.selectedIndexSummary = {
                data: {},
                fields: []
            };
        }

        ///////////////////////////////////Overview/////////////////////////////////
        vm.overview = {};
        vm.overview.selectedIndexSummary = {
            data: {},
            fields: []
        };
        vm.overview.getFieldsForTerms = getFieldsForTerms;
        vm.overview.termsToShow = 10;
        vm.overview.topTerms = {};
        vm.overview.selectedFields = [];
        vm.overview.setSelectedFields = setSelectedFields;
        vm.overview.selectAll = selectAll;
        vm.overview.haveSelectedFields = false;
        vm.overview.haveFieldsToShow = false;
        vm.overview.selectAllCheckbox = {};

        function getFieldsForTerms() {
            var terms = vm.overview.selectedFields.map(field => field.label).toString();
            vm.overview.isRequired = true;
            vm.dashboard.ready = false;
            examineInspectorDashboardResources
                .getTopTermsInIndex(vm.dashboard.selectedIndex.indexPath, vm.overview.termsToShow, terms)
                .then(function (response) {
                    vm.overview.topTerms = response.data;
                    vm.overview.haveFieldsToShow = vm.overview.topTerms.length > 0;
                    vm.dashboard.ready = true;
                },
                    function (error) {
                        vm.dashboard.setError(error);
                    });
        };

        function selectAll($event) {
            if ($event != null) {
                vm.overview.selectAllCheckbox = $event.target;
                if (!angular.isArray(vm.overview.selectedIndexSummary.fields)) {
                    return;
                }

                for (var i = 0; i < vm.overview.selectedIndexSummary.fields.length; i++) {
                    var field = vm.overview.selectedIndexSummary.fields[i];
                    field.selected = vm.overview.selectAllCheckbox.checked;
                    setSelectedFields(field);
                }
            }
        }

        function setSelectedFields(field) {
            var index = vm.overview.selectedFields.indexOf(field);
            var fieldFound = index !== -1;

            if (field.selected) {
                if (!fieldFound) {
                    vm.overview.selectedFields.push(field);
                }
            } else {
                vm.overview.selectedFields.splice(index, 1);
            }

            vm.overview.haveSelectedFields = vm.overview.selectedFields.length > 0;
            vm.overview.topTerms = {};
            vm.overview.haveFieldsToShow = false;
        }

        function getIndexSummary(indexPath) {
            vm.dashboard.ready = false;
            examineInspectorDashboardResources.getSummaryForIndex(indexPath)
                .then(function (response) {
                    response.data.fields.forEach(function (item, index) {
                        vm.overview.selectedIndexSummary.fields.push({
                            id: index,
                            label: item,
                            selected: false
                        });
                    });
                    vm.overview.selectedIndexSummary.data = response.data;
                    vm.dashboard.ready = true;
                    getSelectedIndexTotalFileSize(vm.overview.selectedIndexSummary.data.indexFiles);
                },
                    function (error) {
                        vm.dashboard.setError(error);
                    });
        }

        ///////////////////////////////////Search/////////////////////////////////
        vm.search = {};
        vm.search.selectedAnalyser = '';
        vm.search.analysers = {};
        vm.search.defaultField = {};
        vm.search.searchQuery = '';
        vm.search.searchResults = {};
        vm.search.hasSearchResults = false;
        vm.search.showNoSearchResultsMesssage = false;
        vm.search.hasDocument = false;
        vm.search.renderDocumentInfo = {};
        vm.search.document = {
            data: {},
            keysToRemove: [
                'binaryLength', 'binaryOffset', 'omitNorms', 'omitTermFreqAndPositions', 'storeOffsetWithTermVector',
                'storePositionWithTermVector', 'storeTermVector', 'lazy', 'isCompressed', 'isBinary', 'fieldsData', 'tokenStream'
            ]
        };

        vm.search.fieldsToRemove = [];

        vm.search.fieldsToDisplay = {
            fields: []
        };

        vm.search.checkboxExpanded = false;
        vm.search.docId = '';

        loadAnalysers();

        function loadAnalysers() {
            examineInspectorDashboardResources.getAllAnalysers()
                .then(function (response) {
                    vm.search.analysers = response.data;
                    vm.analyse.analysers = response.data;
                },
                    function (error) {
                        vm.dashboard.setError(error);
                    });
        }

        vm.search.analyserChanged = function () {
            if (vm.search.selectedAnalyser != null) {
                vm.search.hasSearchResults = false;
                vm.search.searchResults = {};
            }
        };

        vm.search.executeSearch = function () {
            vm.dashboard.ready = false;
            examineInspectorDashboardResources.search(
                    vm.dashboard.selectedIndex.indexPath,
                    vm.search.selectedAnalyser,
                    vm.search.searchQuery,
                    vm.search.defaultField.label)
                .then(function (response) {
                    vm.search.searchResults = response.data;

                    if (vm.search.searchResults.length > 0) {
                        vm.search.hasSearchResults = true;
                        vm.search.showNoSearchResultsMesssage = false;

                        var searchResult = vm.search.searchResults[0];

                        Object.keys(searchResult.Fields).forEach(function (key) {
                            vm.search.fieldsToDisplay.fields.push({
                                label: key.replace(/[\. ,:-]+/g, ''),
                                selected: true
                            });
                        });
                    } else {
                        vm.search.hasSearchResults = false;
                        vm.search.showNoSearchResultsMesssage = true;
                    }

                    vm.dashboard.ready = true;
                },
                    function (error) {
                        vm.dashboard.setError(error);
                    });
        };

        vm.search.expand = function ($event) {
            var $this = $($event.currentTarget);
            var $divContainer = $this.parent().find('div');
            var $icon = $this.parent().find('div i');
            var $docId = $this.siblings('td.docId').find(':first-child').text();

            if ($divContainer.hasClass('td-preview')) {
                $divContainer.removeClass('td-preview');
                $icon.addClass('icon-navigation-down').removeClass('icon-navigation-right');

                vm.dashboard.ready = false;

                examineInspectorDashboardResources.getDocumentFromIndex(vm.dashboard.selectedIndex.indexPath, $docId)
                    .then(function (response) {
                        vm.search.document.data = response.data;
                        vm.search.docId = $docId;
                        vm.search.document.data.fields.forEach(function (item) {
                            vm.search.document.keysToRemove.forEach(function (key) {
                                delete item[key];
                            });
                        });

                        vm.search.hasDocument = true;
                        vm.dashboard.ready = true;
                    },
                        function (error) {
                            vm.dashboard.setError(error);
                        }
                    );
            } else {
                $divContainer.addClass('td-preview');
                $icon.addClass('icon-navigation-right').removeClass('icon-navigation-down');

                var documentInfoContainerDocId = $('.document-info-container').data().docid;

                if ($docId == documentInfoContainerDocId) {
                    vm.search.document.data = {};
                    vm.search.hasDocument = false;
                }
            }
        };

        vm.search.renderDocumentInfo = function (value) {
            if (value === true) {
                return 'green';
            } else if (value === false) {
                return 'red';
            }

            return 'none';
        };

        vm.search.horizontalScroll = function ($event) {
            $event.preventDefault();

            var $this = $($event.currentTarget);
            var $table = $this.parents('.more-info-container').siblings('table');
            var $arrowRight = $this.parent().find('a.arrow-right');
            var $arrowLeft = $this.parent().find('a.arrow-left');

            if ($this.hasClass('arrow-right')) {
                $table.animate({
                    scrollLeft: $table.scrollLeft() + 500
                }, {
                    duration: 800,
                    complete: function () {
                        disableArrows($table, $arrowRight, $arrowLeft);
                    }
                }
                );
            } else {
                $table.animate({
                    scrollLeft: $table.scrollLeft() - 500
                }, {
                    duration: 800,
                    complete: function () {
                        disableArrows($table, $arrowRight, $arrowLeft);
                    }
                }
                );
            }
        };

        function disableArrows($table, $arrowRight, $arrowLeft) {
            var scrolledPercentage = 100 * $table.scrollLeft() / ($table[0].scrollWidth - $table[0].clientWidth);

            if (scrolledPercentage >= 100) {
                $arrowRight.addClass('disabled');
                $arrowLeft.removeClass('disabled');
            } else if (scrolledPercentage <= 0) {
                $arrowLeft.addClass('disabled');
                $arrowRight.removeClass('disabled');
            } else if (scrolledPercentage < 100) {
                $arrowRight.removeClass('disabled');
                $arrowLeft.removeClass('disabled');
            }
        }

        ///////////////////////////////////Custom Dropdown/////////////////////////////////

        vm.search.fieldChanged = function (field) {
            var checkboxIndex = vm.search.fieldsToRemove.indexOf(field.label);
            checkboxIndex === -1 ? vm.search.fieldsToRemove.push(field.label) : vm.search.fieldsToRemove.splice(checkboxIndex, 1);

            vm.search.hideFieldFromSearchResults(field.label);
        }

        vm.search.hideFieldFromSearchResults = function (fieldLabel) {
            var field = {};

            vm.search.fieldsToDisplay.fields.forEach(function (item) {
                if (item.label === fieldLabel) {
                    field = item;
                    return false;
                }
            });

            var $fields = $('.search-table th.' + field.label + ', ' + '.search-table td.' + field.label);

            if (field.selected === false) {
                $fields.addClass('hidden');
            } else {
                $fields.removeClass('hidden');
            }
        }

        ///////////////////////////////////Index Files/////////////////////////////////

        function getSelectedIndexTotalFileSize(indexFiles) {
            var fileSizeTotal = 0;

            for (var i = 0; i < indexFiles.length; i++) {
                fileSizeTotal += indexFiles[i].fileSize;
            }
            vm.files.selectedIndexTotalFileSize = fileSizeTotal;
        };

        ///////////////////////////////////Analyse/////////////////////////////////
        vm.analyse = {};
        vm.analyse.executeAnalyser = {};
        vm.analyse.selectedAnalyser = {};
        vm.analyse.textToAnalyse = '';
        vm.analyse.analyseResults = '';

        vm.analyse.analyserChanged = function () {
            vm.analyse.analyseResults = '';
        };

        vm.analyse.executeAnalyser = function () {
            vm.dashboard.ready = false;
            examineInspectorDashboardResources.analyse(vm.analyse.selectedAnalyser, vm.analyse.textToAnalyse)
                .then(function (response) {
                    vm.analyse.analyseResults = response.data;
                    vm.dashboard.ready = true;
                },
                function (error) {
                    vm.dashboard.setError(error);
                });
        };
    }
})();