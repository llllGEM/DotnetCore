(function() {
    'use strict';

    angular
        .module('flapperNews', ['ui.router'])
        .config(['$stateProvider','$urlRouterProvider', config ]);
    
    function config($stateProvider, $urlRouterProvider) {

        $stateProvider
            .state('home', {
                url: '/home',
                templateUrl: '/home.html',
                controller: 'homeController',
                resolve: {
                    postPromise: ['postsFactory', function(postsFactory){
                        return postsFactory.getAll();
                    }]
                }
            })
            .state('posts', {
                url: '/posts/{id}',
                templateUrl: '/post.html',
                controller: 'postsController',
                resolve: {
                    post: ['$stateParams', 'postsFactory', function($stateParams, postsFactory) {
                        return postsFactory.get($stateParams.id);
                    }]
                }
            });

        $urlRouterProvider.otherwise('home');
        
    }

})();

