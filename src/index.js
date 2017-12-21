var express = require("express");
var app = express();

app.set('views', __dirname + '/views');
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');

var places = [	{ id:"i1", name:"n1"},
				{ id:"i2", name:"n2"}	];

var place = [{id:"id", name:"na", location:"lo"}];

app.get('/',function(request, response) {
	response.sendFile(__dirname + '/views/client.html');
});

//dropdown-menu
app.get('/list', function(request, response) {
	response.render('client.html', {data: places})
});

//info page + confronto
app.get('/info', function(request, response) {
	response.render('client.html', {data: place});
});

module.exports = app;

console.log(places);
app.listen(3000, function() {
  console.log("Started on PORT 3000");
});

