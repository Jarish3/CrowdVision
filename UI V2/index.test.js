const index = require('./index.js');
//const values = require('./values.json');

let outputData = "";
storeLog = inputs => (outputData += inputs);


//Per la route /mensaAll, se tutto funziona, ritorna i valori nel json
test('Data una query valida, il risultato deve matchare il valore presente del json di partenza', () => {
    expect(result).toBe([
        {id: 1, idgruppo: 1, nome: 'Povo 0', indirizzo: 'via Sommarive', disponibilità: true, numerodipersone: 50, tempodiattesa: 10, orariapertura: 'Lun - Ven, 11.50 - 14.00'},
    {id: 2, idgruppo: 1, nome: 'Povo 1', indirizzo: 'via Sommarive 12', disponibilità: true, numerodipersone: 60, tempodiattesa: 30, orariapertura: 'Lun - Ven, 11.50 - 14.00'}
    ]);
});

//Per la route /mensaAll, in caso di errore da parte del DB dovrebbe ritornare errore
test('Data una query errata, deve ritornare errore', () => {
    expect(result).toBe(err());
});

//Per la route /mensaAll, se c'è un errore di connessione al db deve stamparlo a console
test('Data un id gruppo non valido, deve stampare errore a console', () => {
    console["log"] = jest.fn(storeLog);
    expect(outputData).toBe('Errore durante esecuzione della query');
});



//Per la route /:id_mensa, se id valido ritorna l'id e le relative informazioni
test('Dato un id valido, il risultato deve matchare il valore presente del json di partenza relativo allo id selezionato', () => {
    expect().toBe({
    id: 1,
    nome: 'Povo 0',
    indirizzo: 'via Sommarive',
    disponibilità: true,
    numerodipersone: 50,
    tempodiattesa: 10,
    orariapertura: 'Lun - Ven, 11.50 - 14.00'
    });
});

//Per la route /:id_mensa, se id non valido ritorna err()
test('Dato un id non valido, deve ritornare errore', () => {
    expect(result).toBe(err());
});

//Per la route /:id_mensa, se c'è un errore di connessione al db deve stamparlo a console
test('Data un id gruppo non valido, deve stampare errore a console', () => {
    console["log"] = jest.fn(storeLog);
    expect(outputData).toBe('Errore durante esecuzione della query');
});



//Per la route /:id_gruppo, se id valido ritorna l'id della mensa più veloce e il suo tempo di attesa
test('Dato un id di gruppo valido, ritorna lid della mensa più veloce e il suo tempo', () => {
    expect(result).toBe('1, 10');
});

//Per la route /:id_gruppo, se id non valido ritorna err()
test('Data un id gruppo non valido, deve ritornare errore', () => {
    expect(result).toBe(err());
});

//Per la route /:id_gruppo, se c'è un errore di connessione al db deve stamparlo a console
test('Data un id gruppo non valido, deve stampare errore a console', () => {
    console["log"] = jest.fn(storeLog);
    expect(outputData).toBe('Errore durante esecuzione della query');
});