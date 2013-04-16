using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Data.Services.Client;
using System.Xml;
using System.Net;
using System.IO;

namespace gov.data.util
{
    public static class GOVDataUtil
    {
        //Define API Key and Shared Secret for DOL API Access ONLY!
        private const string DOLApiKey = "YOUR API KEY";
        private const string DOLSharedSecret = "YOUR SHARED SECRET";

        /// <summary>
        /// GOVDataRequestor is private method used by GOVDataRequest to use when accessing all non-DOL Government APIs
        /// </summary>
        /// <param name="uri">The uri of the target api less and query string parameters that involve key</param>
        /// <param name="key">The key required, if any, by the external api</param>
        /// <param name="username">The username required, if any, by the external api</param>
        private static string GOVDataRequestor(string uri, string key, string username)
        {
            try
            {
                string ret = string.Empty;
                int index = 0;

                if (!string.IsNullOrEmpty(key))
                {
                    Uri target = new Uri(uri);

                    if (string.IsNullOrEmpty(target.Query))
                    {
                        uri = uri + "?";
                        index = uri.Length;
                    }
                    else
                    {
                        index = uri.Length - target.Query.Length + 1;
                    }

                    if (target.Query.Contains("key") || target.Query.Contains("token") || target.Query.Contains("api_key") || target.Query.Contains("webKey") || target.Query.Contains("apiKey"))
                    {
                        throw new System.Exception("Key information should not be included in the URI.");
                    }

                    if (uri.ToUpper().Contains("GO.USA.GOV"))
                    {
                        uri = uri.Insert(index, "login=" + username + "&apiKey=" + key + "&");
                    }
                    else if (uri.ToUpper().Contains("CENSUS.GOV"))
                    {
                        uri = uri.Insert(index, "key=" + key + "&");
                    }
                    else if (uri.ToUpper().Contains("NOAA.GOV"))
                    {
                        uri = uri.Insert(index, "token=" + key + "&");
                    }
                    else if (uri.ToUpper().Contains("EIA.GOV") || uri.ToUpper().Contains("NREL.GOV") || uri.ToUpper().Contains("HEALTHFINDER.GOV")
                        || uri.ToUpper().Contains("STLOUISFED.ORG") || uri.ToUpper().Contains("HEALTHFINDER.GOV"))
                    {
                        uri = uri.Insert(index, "api_key=" + key + "&");
                    }
                    else if (uri.ToUpper().Contains("FMCSA.DOT.GOV"))
                    {
                        uri = uri + "webKey=" + key;
                    }
                }

                WebRequest request = WebRequest.Create(new Uri(uri));  // Create a request using a URL that can receive a post.              
                request.Method = "GET";  // Set the Method property of the request to POST.              
                //request.ContentType = "text/html";// Set the ContentType property of the WebRequest.               
                WebResponse response = request.GetResponse();// Get the response.

                ret = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();

                return ret;
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
        }

        public static string GOVDataRequest(string uri, string key)
        {
            return GOVDataRequestor(uri, key, "");
        }

        public static string GOVDataRequest(string uri)
        {
            return GOVDataRequestor(uri, "", "");
        }

        public static string GOVDataRequest(string uri, string key, string username)
        {
            return GOVDataRequestor(uri, key, username);
        }

        /// <summary>
        /// Event handler for the SendingRequest event of the Service Reference class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void service_SendingRequest(object sender, SendingRequestEventArgs e)
        {
            if (e.Request.RequestUri.ToString().ToUpper().Contains("DOL.GOV"))
            {
                //Get Uri (Without host name or http://
                string requestUri = e.Request.RequestUri.PathAndQuery;

                //Build a timestamp in the format required by the API. ISO-8601
                string Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

                //Build the signature
                string signature = Hash(string.Format("{0}&Timestamp={1}&ApiKey={2}", requestUri, Timestamp, DOLApiKey), DOLSharedSecret);

                //Add the Authorization header
                e.RequestHeaders["Authorization"] = string.Format("Timestamp={0}&ApiKey={1}&Signature={2}", Timestamp, DOLApiKey, signature);
            }
        }

        /// <summary>
        /// Utility method to hash a string using HMAC-SHA1
        /// </summary>
        /// <param name="data">The string to be hashed</param>
        /// <param name="seed">The key to sign with</param>
        /// <returns>The result of the HMAC-SHA1 hash operation</returns>
        public static string Hash(string data, string seed)
        {
            //Convert key to bytes as required by the HMACSHA1 function
            var secret = Encoding.UTF8.GetBytes(seed);
            //Build new HMACSHA1 instance with key bytes
            var signer = new HMACSHA1(secret);

            //Convert string to be signed into an array of bytes
            var toSign = Encoding.UTF8.GetBytes(data);
            //Compute hash
            var sigBytes = signer.ComputeHash(toSign);

            //Convert result to string
            string result = BitConverter.ToString(sigBytes).Replace("-", "");

            return result;
        }
    }
}
