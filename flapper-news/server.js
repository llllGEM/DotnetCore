var express = require('express');
var path = require('path');
var favicon = require('serve-favicon');
var logger = require('morgan');
var cookieParser = require('cookie-parser');
var bodyParser = require('body-parser');

//db
var mongoose = require('mongoose');
mongoose.Promise = global.Promise;
require('./models/Posts');
require('./models/Comments');
//add other models 
mongoose.connect('mongodb://localhost/news')
    .then(() =>  console.log('MongoDB connection succesful'))
    .catch((err) => console.error(err));

//routes
var index = require('./routes/index');
var api = require('./routes/api');
var users = require('./routes/users');

var server = express();

// view engine setup
server.set('views', path.join(__dirname, 'views'));
server.set('view engine', 'ejs');

server.use(favicon(path.join(__dirname, 'public', 'favicon.png')));
server.use(logger('dev'));
server.use(bodyParser.json());
server.use(bodyParser.urlencoded({ extended: false }));
server.use(cookieParser());
server.use(express.static(path.join(__dirname, 'public')));

server.use('/', index);
server.use('/api', api);
server.use('/users', users);

// catch 404 and forward to error handler
server.use(function(req, res, next) {
  var err = new Error('Not Found');
  err.status = 404;
  next(err);
});

// error handler
server.use(function(err, req, res, next) {
  // set locals, only providing error in development
  res.locals.message = err.message;
  res.locals.error = req.app.get('env') === 'development' ? err : {};

  // render the error page
  res.status(err.status || 500);
  res.render('error');
});

module.exports = server;
