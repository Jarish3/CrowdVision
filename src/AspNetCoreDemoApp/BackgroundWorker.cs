using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using System.IO;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(CrowdVision.App_Start.PodMonitorConfig), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(CrowdVision.App_Start.PodMonitorConfig), "Shutdown")]

namespace CrowdVision.App_Start
{

    public static class PodMonitorConfig
    {
        private static PodMonitorJob m_job;
        public static void Start()
        {
			string str = Environment.GetEnvironmentVariable("poolerRate");
			Console.WriteLine("time:" + str);
			
			m_job = new PodMonitorJob(TimeSpan.FromMilliseconds(Math.Max(4000,Convert.ToInt32(str))));
			
		}

        public static void Shutdown()
        {
            m_job.Dispose();
        }
    }
}

namespace CrowdVision
{


    public class PodMonitorJob : IDisposable
    {
        public static int timesRan = 0;
        private CancellationTokenSource m_cancel;
        private Task m_task;
        private TimeSpan m_interval;
        private bool m_running;

        public PodMonitorJob(TimeSpan interval)
        {
            m_interval = interval;
            m_running = true;
            m_cancel = new CancellationTokenSource();
            m_task = Task.Run(() => TaskLoop(), m_cancel.Token);
            CrowdVision.Pooler.Pooler.Setup();
        }

        private void TaskLoop()
        {
            while (m_running)
            {
                timesRan++;
                Pooler.Pooler.ProcessTick();
                Thread.Sleep(m_interval);
            }
        }

        public void Dispose()
        {
            m_running = false;

            if (m_cancel != null)
            {
                try
                {
                    m_cancel.Cancel();
                    m_cancel.Dispose();
                }
                catch
                {
                }
                finally
                {
                    m_cancel = null;
                }
            }
        }
    }
}

namespace CrowdVision.Pooler
{

    static class Pooler
    {
        // **********************************************
        // *** Update or verify the following values. ***
        // **********************************************

        // Replace the subscriptionKey string value with your valid subscription key.
        static string subscriptionKey = "ff568f5f4f174f82a892e19b07da9c76";

        // Replace or verify the region.
        //
        // You must use the same region in your REST API call as you used to obtain your subscription keys.
        // For example, if you obtained your subscription keys from the westus region, replace 
        // "westcentralus" in the URI below with "westus".
        //
        // NOTE: Free trial subscription keys are generated in the westcentralus region, so if you are using
        // a free trial subscription key, you should not need to change this region.
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";

        public static NpgsqlConnection postgresqlConn;
        public static bool connected;
        public static string connstring;
        public static void Setup()
        {
			subscriptionKey = Environment.GetEnvironmentVariable("subKey");

			string baseStr = Environment.GetEnvironmentVariable("DATABASE_URL").Substring(11);

			string userID = baseStr.Substring(0, baseStr.IndexOf(':'));
			baseStr = baseStr.Substring(userID.Length + 1);

			string password = baseStr.Substring(0, baseStr.IndexOf('@'));
			baseStr = baseStr.Substring(password.Length + 1);

			string server = baseStr.Substring(0, baseStr.IndexOf(':'));
			baseStr = baseStr.Substring(server.Length + 1);

			string port = baseStr.Substring(0, baseStr.IndexOf('/'));
			string database = baseStr.Substring(port.Length + 1); ;

			connstring = String.Format("Server = {0}; Port = {1}; User Id = {2}; Password = {3}; Database = {4}; ", server,port,userID,password,database);
			connected = connectSQL();
			
        }

        public static void ProcessTick()
        {

            if (!connected)
            {
                Console.WriteLine("Process waiting for SQL Connection..");
                return;
            }

            // Execute the REST API call.

            string[] files = Environment.GetEnvironmentVariable("pathMense").Split('|');
            foreach (string s in files)
            {
                string[] str = s.Split(';');
                analyzeImage(str[0],str[1]);
            }
        }

        static bool connectSQL()
        {
            try
            {

			// PostgeSQL-style connection string
			// Making connection with Npgsql provider

				NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                postgresqlConn = conn;
                return true;
            }
            catch (Exception msg)
            {
                // something went wrong, and you wanna know why
                Console.WriteLine(msg);
				Console.WriteLine("ConnString:");
				Console.WriteLine(connstring);
				throw msg;
            }

        }
        /// <summary>
        /// Gets the analysis of the specified image file by using the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file.</param>
        static async void analyzeImage(string idMensa, string imageFilePath)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                var vp = new VisionProcesser(idMensa, contentString, byteData);
                vp.PushSQL(postgresqlConn);
                Console.WriteLine("Analyzed image:" + Path.GetFileName(imageFilePath));
            }
        }


        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            var client = new WebClient();
            return client.DownloadData(new Uri(imageFilePath));
        }



    }
}

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

    public bool PushSQL(NpgsqlConnection connection)
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
			insertCmd.Dispose();
            return true;
        }
        catch (Exception e)
        {
			Console.WriteLine("-------------------------------");
			Console.WriteLine("SQL Error");

			Console.WriteLine(e.ToString());
			Console.WriteLine("-------------------------------");
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
