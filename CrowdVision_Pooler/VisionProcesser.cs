using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CrowdVision_Pooler
{
    class VisionProcesser
    {



        private int peopleCount = 0;
        private byte[] imageData;
        private string responseString;
        private string idMensa;

        public int PeopleCount
        {
            get
            {
                return peopleCount;
            }
        }

        public byte[] ImageData
        {
            get
            {
                return imageData;
            }
        }

        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }

        public string IDMensa
        {
            get
            {
                return idMensa;
            }
        }
        public VisionProcesser(string idMensa, string responseJsonString, byte[] imgData)
        {
            responseString = responseJsonString;
            imageData = imgData;
            this.idMensa = idMensa;
            var xml = JsonConvert.DeserializeXmlNode("{\"Row\":" + responseJsonString + "}", "root");
            peopleCount = xml.ChildNodes[0].ChildNodes.Count;
            Console.WriteLine("Number of faces detected:" + peopleCount);
        }

        public bool pushSQL(NpgsqlConnection connection)
        {
            try
            { 
                NpgsqlCommand insertCmd = new NpgsqlCommand("INSERT INTO foto (id_mensa,data,immagine,n_persone) VALUES('" +
                                    IDMensa + "',:data,:imgParam,'" +
                                    peopleCount + "')", connection);

                NpgsqlParameter imgParam = new NpgsqlParameter("imgParam", NpgsqlDbType.Bytea);
                NpgsqlParameter dataParam = new NpgsqlParameter("data", NpgsqlDbType.Timestamp);

                imgParam.Value = imageData;
                dataParam.Value = DateTime.Now;
                insertCmd.Parameters.Add(imgParam);
                insertCmd.Parameters.Add(dataParam);
                insertCmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("SQL Error");
                // if things fail
                return false;
            }
        }

        /// <summary>
        /// Formats the given JSON string by adding line breaks and indents.
        /// </summary>
        /// <param name="json">The raw JSON string to format.</param>
        /// <returns>The formatted JSON string.</returns>
        public static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}
