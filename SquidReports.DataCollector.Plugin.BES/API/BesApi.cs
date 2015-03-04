using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using RestSharp;

namespace SquidReports.DataCollector.Plugin.BES.API
{
    public class BesApi
    {
        // Fields
        private Uri baseURL;
        private HttpBasicAuthenticator authenticator;

        // Properties
        public Uri BaseURL
        {
            get { return this.baseURL; }
            private set { this.baseURL = value; }
        }

        public HttpBasicAuthenticator Authenticator
        {
            get { return this.authenticator; }
            private set { this.authenticator = value; }
        }

        // Constructors
        public BesApi(string aBaseURL, string aUsername, string aPassword)
        {
            // Use to ignore SSL errors if specified in App.config
            if (AppSettings.Get<bool>("IgnoreSSL"))
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            }

            this.BaseURL = new Uri(aBaseURL);
            this.authenticator = new HttpBasicAuthenticator(aUsername, aPassword);
        }


        // Methods
        public List<Model.Action> GetActions()
        {
            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            List<Model.Action> actions = new List<Model.Action>();

            // We need to use Session Relevance to acquire the list of Sites, REST API sucks
            // We'll use the following Relevance query, no parameters are required:
            string relevance = "(((name of it) of site of it) of source fixlets of it, id of it, name of it) of BES Actions";

            // Let's compose the request string
            RestRequest request = new RestRequest("query", Method.GET);
            request.AddQueryParameter("relevance", relevance);

            XDocument response = Execute(request);

            Console.WriteLine("");

            // Let's check if the Result element is empty
            if (response.Element("BESAPI").Element("Query").Element("Result").Elements().Count() > 0)
            {
                // We'll need to fetch the list of Sites from the DB in order to retrieve the SiteID
                // TODO: BesDb DB = new BesDb(ConfigurationManager.ConnectionStrings["DB"].ToString());

                // All answers are wrapped inside a "Tuple" element
                foreach (XElement tupleElement in response.Element("BESAPI").Element("Query").Element("Result").Elements("Tuple"))
                {
                    // The Result consists of three parts:
                    //  1) The Site Name
                    //  2) The ActionID
                    //  3) The Action Name
                    XElement siteElement = tupleElement.Elements("Answer").First();
                    XElement actionIDElement = tupleElement.Elements("Answer").ElementAt(1);
                    XElement valueElement = tupleElement.Elements("Answer").Last();

                    // Resolve Site Name to Site ID
                    // TODO: Site dbSite = DB.Connection.Query<Site>("SELECT * FROM BESEXT.SITE WHERE @Name = Name", new { Name = siteElement.Value }).Single();

                    // Add the new action
                    actions.Add(new Model.Action(Convert.ToInt32(actionIDElement.Value), siteElement.Value, valueElement.Value));
                }
            }

            return actions;
        }

        public XDocument Execute(RestRequest request)
        {
            RestClient client = new RestClient();
            client.BaseUrl = this.BaseURL;
            client.Authenticator = this.Authenticator;

            IRestResponse response = client.Execute(request);

            try
            {
                if (response.ErrorException != null)
                {
                    // TODO: logger.ErrorException(response.ErrorException.Message, response.ErrorException);
                    throw new Exception(response.ErrorMessage);
                }

                // Return non-deserialized XML document
                return XDocument.Parse(response.Content, LoadOptions.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error encountered: {0}", ex.Message);
                return null;
            }
        }

        public T Execute<T>(RestRequest request) where T : new()
        {
            RestClient client = new RestClient();
            client.BaseUrl = this.BaseURL;
            client.Authenticator = this.Authenticator;

            var response = client.Execute<T>(request);

            try
            {
                if (response.ErrorException != null)
                {
                    throw new Exception(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error encountered: {0}", ex.Message);
            }

            return response.Data;
        }
    }
}
