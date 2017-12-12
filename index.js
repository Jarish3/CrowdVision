//express config
var express = require('express');
var app = express();

//pg config
var pg = require('pg');
//Sostituire con link di Heroku vvvvv <------------------------------------------------------
var connectionString = 'postgresql://dbuser:secretpassword@database.server.com:3211/mydb';

//GET tutte le mense - menù a tendina
app.get('/mensaALL', function(request, response, next) {
	pg.connect(connectionString, function(err, client, done) {
		if (err)
			return console.error('Errore durante il recupero del client dal pool', err);
		
		console.log("Connesso al database");
		
		client.query('SELECT Id_mensa, Nome_mensa FROM MENSE', function(err, result) {
			done();
			if (err)
				return console.error('Errore durante esecuzione della query', err);
			
			response.send(result);
		});
	});
});

//GET singola mensa - pagina info
app.get('/:id_mensa', function(request, response, next) {
	pg.connect(connectionString, function(err, client, done) {
		if (err)
			return console.error('Errore durante il recupero del client dal pool', err);
		
		console.log("Connesso al database");
		
		//Struttura request: params??? <---------------------------------------------------
		client.query('SELECT * FROM MENSE M INNER JOIN ORARI O ON M.Id_mensa=O.Id_mensa AND M.Id_mensa=$1 INNER JOIN FOTO F ON M.Id_mensa=F.Id_mensa AND F.Data = (SELECT MIN(Data) FROM FOTO F WHERE F.Id_mensa=$1)', [request.params.id], function(err, result) {
			done();

			if (err)
				return console.error('Errore durante esecuzione della query', err);
			
			response.send(result);
		});
	});
});

//GET mensa più veloce di gruppo - confronto mense più veloci nella home
app.get('/:id_gruppo', function(request, response, next) {
	pg.connect(connectionString, function(err, client, done) {
		if (err)
			return console.error('Errore durante il recupero del client dal pool', err);
		
		console.log("Connesso al database");
		
		//Struttura request: params??? <---------------------------------------------------
		client.query('SELECT Id_mensa, Nome_mensa, Tempo_sevizio FROM MENSE WHERE Id_gruppo=$1', [request.params.id], function(err, result) {
			done();
			if (err)
				return console.error('Errore durante esecuzione della query', err);
			
			var id_fast = 0;
			var time = result[0].N_persone * result[0].Tempo_servizio;
			var temp;
			 
			for (var i=1; i<result.size(); i++) {
				temp = result[i].N_persone * result[i].Tempo_servizio;
				if (temp < time) {
					id_fast = i;
					time = temp;
				}
			}
			
			response.send(result[id_fast], time);
		});
	});
});

module.exports = app;
