(function() {
    'use strict';

    angular
        .module('flapperNews')
        .controller('homeController', ['$scope','postsFactory',homeController]);
    
    function homeController($scope, postsFactory){
        $scope.test  = 'Hello world!';

        $scope.posts = postsFactory.posts;

        $scope.addPost = function () {
            if(!$scope.title || $scope.title === '') {return;}
            postsFactory.create({
                title: $scope.title,
                link : $scope.link
            });
            $scope.title = '';
            $scope.link = '';
        };

        $scope.incrementUpvotes = function(post) {
            postsFactory.upvote(post);
        };

        $scope.downvote = function(post) {
            postsFactory.downvote(post);
        };
    }

})();
