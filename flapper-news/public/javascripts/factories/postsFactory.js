(function() {
    'use strict';

    angular
        .module('flapperNews')
        .factory('postsFactory', ['$http', postsFactory]);

    function postsFactory($http){
        var o = {
            posts: []
        };

        o.getAll = function() {
            return $http.get('/api/posts')
                .success(function(data){
                    angular.copy(data, o.posts);
                });
        };

        o.create = function(post) {
            return $http.post('/api/posts', post)
                .success(function(data){
                    o.posts.push(data);
                });
        };

        o.upvote = function(post) {
            return $http.put('/api/posts/'+post._id+'/upvote')
                .success(function(data){
                    angular.copy(data, post);
                });
        };

        o.downvote = function(post){
            return $http.put('/api/posts/'+post._id+'/downvote')
                .success(function(data){
                    angular.copy(data,post);
                })
        };

        o.get = function(id) {
            return $http.get('/api/posts/' + id).then(function(res){
                return res.data;
            });
        };

        o.addComment = function(id, comment) {
            return $http.post('/api/posts/' + id + '/comments', comment);
        };

        o.upvoteComment = function(post, comment) {
            return $http.put('/api/posts/' + post._id + '/comments/'+ comment._id + '/upvote')
                .success(function(data){
                    comment.upvotes += 1;
                });
        };

        o.downvoteComment = function(post, comment) {
            return $http.put('/api/posts/' + post._id + '/comments/'+ comment._id + '/downvote')
                .success(function(data){
                    comment.downvotes += 1;
                });
        };

        return o;
    }
    
})();