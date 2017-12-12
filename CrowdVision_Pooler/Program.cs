using Npgsql;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Configuration;
using System.Timers;

namespace CrowdVision_Pooler
{
    static class Program
    {
        // **********************************************
        // *** Update or verify the following values. ***
        // **********************************************

        // Replace the subscriptionKey string value with your valid subscription key.
        const string subscriptionKey = "ff568f5f4f174f82a892e19b07da9c76";

        // Replace or verify the region.
        //
        // You must use the same region in your REST API call as you used to obtain your subscription keys.
        // For example, if you obtained your subscription keys from the westus region, replace 
        // "westcentralus" in the URI below with "westus".
        //
        // NOTE: Free trial subscription keys are generated in the westcentralus region, so if you are using
        // a free trial subscription key, you should not need to change this region.
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";

        static private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\images");
            foreach (string s in files)
            {
                analyzeImage(s);
            }
        }


        static NpgsqlConnection postgresqlConn;
        static void Main()
        {
            // Get the path and filename to process from the user.
            Console.WriteLine("Detect faces:");

            if(!connectSQL())
            {
                Console.WriteLine("SQL connection error! Aborting..");
                Console.ReadLine();
                return;

            }

            // Execute the REST API call.



            Timer r = new System.Timers.Timer(30000);
            r.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            r.AutoReset = true;
            r.Start();

            Console.WriteLine("Write something to exit.");


            Console.ReadLine();
            r.Stop();
        }

        static bool connectSQL()
        {
            try
            {
                
                // PostgeSQL-style connection string
                string connstring = ConfigurationManager.ConnectionStrings["postgresql"].ConnectionString;
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
                return false;
            }

        }
        /// <summary>
        /// Gets the analysis of the specified image file by using the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file.</param>
        static async void analyzeImage(string imageFilePath)
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

                var vp = new VisionProcesser("mensa0",contentString, byteData);
                vp.pushSQL(postgresqlConn);
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
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }


       
    }
}