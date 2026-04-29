(function () {
    'use strict';

    var app = angular.module('pointsTableApp', []);

    app.controller('PointsTableController', ['$scope', '$http', function ($scope, $http) {
        $scope.departments = [];
        $scope.isLoading = true;
        $scope.sortColumn = 'totalPoints';
        $scope.sortReverse = true;
        $scope.selectedCategory = '';

        $scope.loadPoints = function () {
            $scope.isLoading = true;
            const urlParams = new URLSearchParams(window.location.search);
            const year = urlParams.get('year') || '';
            
            $http.get('/Home/GetPointsTableData?year=' + year + '&category=' + $scope.selectedCategory)
                .then(function (response) {
                    $scope.departments = response.data;
                    $scope.isLoading = false;
                }, function (error) {
                    console.error('Error fetching points table data:', error);
                    $scope.isLoading = false;
                });
        };

        $scope.filterByCategory = function() {
            $scope.loadPoints();
        };

        $scope.toggleDetails = function(dept) {
            dept.showDetails = !dept.showDetails;

            if (dept.showDetails) {
                // Determine ID (handle case sensitivity)
                var rawId = dept.deptID || dept.DeptID || dept.deptId;
                var id = Number(rawId);
                
                if (!id) {
                    console.error("Invalid Department ID found:", dept);
                    dept.errorDetails = "Invalid Department ID";
                    return;
                }

                // Always reload if category changed or not loaded
                if (!dept.matchDetails || dept.lastLoadedCategory !== $scope.selectedCategory) {
                    dept.loadingDetails = true;
                    dept.errorDetails = null; // Clear previous errors
                    
                    const urlParams = new URLSearchParams(window.location.search);
                    const year = urlParams.get('year') || '';
                    
                    var url = '/Home/GetDepartmentMatchDetails?id=' + id + '&year=' + year + '&category=' + ($scope.selectedCategory || '');
                    
                    $http.get(url)
                        .then(function(response) {
                            dept.matchDetails = response.data || [];
                            dept.lastLoadedCategory = $scope.selectedCategory;
                            dept.loadingDetails = false;
                        }, function(error) {
                            console.error('Error fetching match details:', error);
                            dept.loadingDetails = false;
                            dept.errorDetails = "Failed to load details.";
                        });
                }
            }
        };

        $scope.sortBy = function (columnName) {
            if ($scope.sortColumn === columnName) {
                $scope.sortReverse = !$scope.sortReverse;
            } else {
                $scope.sortColumn = columnName;
                $scope.sortReverse = false;
            }
        };

        // Initial load
        $scope.loadPoints();
    }]);

})();
