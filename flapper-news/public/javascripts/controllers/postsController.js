(function(){
    'use strict';

    angular
        .module('flapperNews')
        .controller('postsController', ['$scope','postsFactory','post',postsController]);

    function postsController($scope, postsFactory, post){
        $scope.post = post;
        $scope.addComment = function(){
            if($scope.body === '') { return; }
            //if(!$scope.post.comments){$scope.post.comments = [];}
            postsFactory.addComment(post._id, {
                body: $scope.body,
                author: 'user',
            }).success(function(comment) {
                $scope.post.comments.push(comment);
            });
            $scope.body = '';
        };
        $scope.incrementUpvotes = function(comment){
            postsFactory.upvoteComment(post, comment);
        };

        $scope.downvote = function(comment){
            postsFactory.downvoteComment(post, comment);
        }
    }
    
})();