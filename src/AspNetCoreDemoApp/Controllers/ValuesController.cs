using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace AspNetCoreDemoApp.Controllers
{
	[Route("api/[controller]")]
	public class ValuesController : ControllerBase
	{
		// GET: api/values
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new[] { "value1", "value2" };
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value";
		}
	}

	[Route("api/[controller]")]
	public class MenseController : ControllerBase
	{
		// GET: api/Mense
		[HttpGet]
		public IEnumerable<string> Get()
		{
			NpgsqlCommand selectCmd = new NpgsqlCommand("SELECT Id_mensa, Nome_mensa FROM MENSE;");

			var x = selectCmd.ExecuteReader();

			List<string> retu = new List<string>();
			while(x.IsOnRow)
			{
				string retStr = "";
				for (int i = 0; i < x.FieldCount; i++)
					retStr += x.GetString(i);
				retu.Add(retStr);
				x.NextResult();
			}
			return retu;
		}

		// GET api/mense/0
		[HttpGet("{id}")]
		public string Get(string id)
		{
			NpgsqlCommand selectCmd = new NpgsqlCommand("SELECT * FROM MENSE where id_mensa=" + id);

			var x = selectCmd.ExecuteReader();
			string retStr = "";
			for (int i = 0; i < x.FieldCount; i++)
				retStr += x.GetString(i);

			return retStr;
		}
	}
}