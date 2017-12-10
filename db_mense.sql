CREATE TYPE settimana AS ENUM ('Lun', 'Mar', 'Mer', 'Gio', 'Ven', 'Sab', 'Dom');

CREATE TABLE GRUPPI (
	Id_gruppo	INTEGER UNIQUE		NOT NULL,
	Nome_gruppo	VARCHAR(30)			NOT NULL,
	
	PRIMARY KEY (Id_gruppo)
);

CREATE TABLE MENSE (
	Id_mensa		VARCHAR(10)	UNIQUE						NOT NULL,
	Id_gruppo		INTEGER REFERENCES GRUPPI(Id_gruppo) 	NOT NULL,
	Nome_mensa		VARCHAR(75)								NOT NULL,
	Indirizzo		VARCHAR(75)								NOT NULL,
	Latitudine 	 	DOUBLE PRECISION						NOT NULL,
	Longitudine	 	DOUBLE PRECISION						NOT NULL,
	Capacità		INTEGER									NOT NULL,
	Tempo_servizio	REAL									NOT NULL,
	
	PRIMARY KEY (Id_mensa, Id_gruppo)
);

CREATE TABLE FOTO (
	Id_mensa	VARCHAR(10)	REFERENCES MENSE(Id_mensa) 	NOT NULL,
	Data		TIMESTAMP								NOT NULL,
	Immagine	BYTEA									NOT NULL,
	N_Persone	INTEGER									NOT NULL,
	
	PRIMARY KEY (Id_mensa, Data)
);

CREATE TABLE ORARI (
	Id_mensa		VARCHAR(10)	REFERENCES MENSE(Id_mensa)	NOT NULL,
    Giorno			settimana	 							NOT NULL,
	Ora_apertura	TIME 									NOT NULL,
	Ora_chiusura	TIME 									NOT NULL,
	
	PRIMARY KEY (Id_mensa, Giorno, Ora_apertura, Ora_chiusura)
);