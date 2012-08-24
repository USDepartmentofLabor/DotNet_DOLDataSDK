using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Data.Services.Client;

namespace gov.dol.doldata.util
{
    public static class DOLDataUtil
    {
        //Define API Key and Shared Secret
        private const string ApiKey = "ADD YOUR API KEY";
        private const string SharedSecret = "ADD YOUR SHARED SECRET";

        /// <summary>
        /// Event handler for the SendingRequest event of the Service Reference class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void service_SendingRequest(object sender, SendingRequestEventArgs e)
        {
            //Get Uri (Without host name or http://
            string requestUri = e.Request.RequestUri.PathAndQuery;

            //Build a timestamp in the format required by the API. ISO-8601
            string Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            //Build the signature
            string signature = Hash(string.Format("{0}&Timestamp={1}&ApiKey={2}", requestUri, Timestamp, ApiKey), SharedSecret);

            //Add the Authorization header
            e.RequestHeaders["Authorization"] = string.Format("Timestamp={0}&ApiKey={1}&Signature={2}", Timestamp, ApiKey, signature);
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
