SELECT Id_mensa, Nome_mensa FROM MENSE;
SELECT * FROM MENSE M INNER JOIN ORARI O ON M.Id_mensa=O.Id_mensa AND M.Id_mensa=$1 INNER JOIN FOTO F ON M.Id_mensa=F.Id_mensa AND F.Data = (SELECT MIN(Data) FROM FOTO F WHERE F.Id_mensa=$1)
SELECT Id_mensa, Nome_mensa, Tempo_sevizio FROM MENSE WHERE Id_gruppo=$1;
