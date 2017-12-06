//express config
var express = require('express');
var router = express.Router();

//pg config
var pg = require('pg');
var connectionString = 'postgresql://dbuser:secretpassword@database.server.com:3211/mydb'; //Heroku forma?

//Mensa
//GET tutte le mense
router.get('/users', function(request, response, next) { //nome?
	pg.connect(connectionString, function(err, client, done) {
	if (err) {	return console.error('Errore durante il recupero del client dal pool', err);	}
	console.log("Connesso al database");
	
	client.query('SELECT * FROM users', function(err, result) { //nome?
		done();
		if (err) {	return console.error('Errore durante esecuzione della query', err);	}
		response.send(result);
		});
	});
});

//GET singola mensa
router.get('/users/:id', function(request, response, next) { //nome?
	pg.connect(connectionString, function(err, client, done) {
	if (err) {	return console.error('Errore durante il recupero del client dal pool', err);	}
	console.log("Connesso al database");
	
	client.query('SELECT * FROM users WHERE id = $1', [request.params.id], function(err, result) { //nome?
		done();
		if (err) {	return console.error('Errore durante esecuzione della query', err);	}
		response.send(result);
		});
	});
});

module.exports = router;