using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CrowdVisionCoreApp.Controllers
{
	[Route("api/[controller]")]
	public class MenseController : ControllerBase
	{
		// GET: api/Mense
		[HttpGet]
		public IEnumerable<string> Get()
		{
			var conn = new NpgsqlConnection(CrowdVision.Pooler.Pooler.connstring);
			
			conn.Open();

			NpgsqlCommand selectCmd = new NpgsqlCommand("SELECT Id_mensa, Nome_mensa FROM MENSE ORDER BY nome_mensa;", conn);

			var x = selectCmd.ExecuteReader();
			
			List<string> retu = new List<string>();
			
			while(x.Read())
			{
				string retStr = "";
				for (int i = 0; i < x.FieldCount; i++)
					retStr += x.GetString(i) + ";";

				retStr = retStr.Remove(retStr.Length - 1);
				retu.Add(retStr);
			}
			conn.Close();
			return retu;
		}
	}

	[Route("api/[controller]")]
	public class InfoMensaController : ControllerBase
	{
		// GET: api/infoMensa
		[HttpGet("{id}")]
		public IEnumerable<string> Get(string id)
		{
			id = Utils.SanitizeString(id);
			var conn = new NpgsqlConnection(CrowdVision.Pooler.Pooler.connstring);
			conn.Open();
			NpgsqlCommand selectCmd = new NpgsqlCommand("select nome_mensa,indirizzo,capacità, n_persone, tempo_servizio*n_persone as stimaTempo from mense INNER JOIN (select id_mensa,n_persone from foto where id_mensa='"+id+ "' ORDER BY (data) LIMIT 1) as pers ON pers.id_mensa = mense.id_mensa where mense.id_mensa='" + id + "';", conn);

			var x = selectCmd.ExecuteReader();


			if (!x.Read())
				return new List<string>();
			else
			{ 
				List<string> retu = new List<string>
				{
					x.GetString(0),
					x.GetString(1),
					x.GetValue(2).ToString(),
					x.GetValue(3).ToString(),
					x.GetValue(4).ToString(),
				};
				string calStr = "";

				x.Close();

				selectCmd = new NpgsqlCommand("select giorno, ora_apertura, ora_chiusura from orari where id_mensa='" + id + "';", conn);

				var calen = selectCmd.ExecuteReader();
				while (calen.Read())
				{
					calStr += calen.GetString(0) + ":" + calen.GetValue(1).ToString() + " - " + calen.GetValue(2).ToString() + "<br>";
				}

				if (calStr != "")
				{
					retu.Add(calStr);
				}
				else
				{
					retu.Add("Lun-Ven");
				}

				calen.Close();
				conn.Close();
				return retu;
			}
			
		}
	}


	[Route("api/[controller]")]
	public class MensaVeloceController : ControllerBase
	{
		// GET api/MensaVeloce
		[HttpGet("{id}")]
		public IEnumerable<string> Get(string id)
		{
			id = Utils.SanitizeString(id);
			var conn = new NpgsqlConnection(CrowdVision.Pooler.Pooler.connstring);
			conn.Open();
			NpgsqlCommand selectCmd = new NpgsqlCommand("select nome_mensa, n_persone, n_persone*tempo_servizio as tempoStimato, subqFoto.id_mensa from mense INNER JOIN (select * from foto where data IN(select MAX(data) from foto group by(id_mensa))) as subqFoto on mense.id_mensa = subqFoto.id_mensa where id_gruppo = '" + id + "' ORDER BY n_persone;", conn);

			var x = selectCmd.ExecuteReader();
			if(!x.Read())
				return new List<string>();

			List<string> retu = new List<string>
			{
				x.GetString(0),
				x.GetFloat(1).ToString(),
				x.GetValue(2).ToString(),
				x.GetString(3)
			};
			conn.Close();
			return retu;
		}
	}
}