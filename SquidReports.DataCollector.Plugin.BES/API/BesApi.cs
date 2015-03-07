using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using SquidReports.DataCollector.Interface;
using SquidReports.DataCollector.Plugin.BES.Model;
using RestSharp;

namespace SquidReports.DataCollector.Plugin.BES.API
{
    public class BesApi
    {
        // Fields
        private Uri baseURL;
        private HttpBasicAuthenticator authenticator;
        private IDbRelay dbRelay;
        private ILogger logger;

        // Properties
        public Uri BaseURL { get; set; }
        public HttpBasicAuthenticator Authenticator { get; set; }
        public IDbRelay DbRelay { get; set; }
        public ILogger Logger { get; set; }

        // Constructors
        public BesApi(ILogManager logManager, IDbRelay dbRelay, string aBaseURL, string aUsername, string aPassword)
        {
            // Use to ignore SSL errors if specified in App.config
            if (AppSettings.Get<bool>("IgnoreSSL"))
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            }

            this.BaseURL = new Uri(aBaseURL);
            this.Authenticator = new HttpBasicAuthenticator(aUsername, aPassword);
            ILogger logger = logManager.GetCurrentClassLogger();
            this.DbRelay = dbRelay;
            this.Logger = logger;
        }


        // Methods
        public List<Model.Action> GetActions()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetActions()");

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            List<Model.Action> actions = new List<Model.Action>();

            // We need to use Session Relevance to acquire the list of Sites, REST API sucks
            // We'll use the following Relevance query, no parameters are required:
            string relevance = "(((name of it) of site of it) of source fixlets of it, id of it, name of it) of BES Actions";

            // Let's compose the request string
            RestRequest request = new RestRequest("query", Method.GET);
            request.AddQueryParameter("relevance", relevance);

            // Prepare the XML document
            XDocument response = new XDocument();

            try
            {
                // Execute the request we built
                response = Execute(request);

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
                        //Site dbSite = DB.Connection.Query<Site>("SELECT * FROM BESEXT.SITE WHERE @Name = Name", new { Name = siteElement.Value }).Single();
                        Site dbSite = DbRelay.Get<Site>(new { Name = siteElement.Value }).Single();

                        // Add the new action
                        actions.Add(new Model.Action(Convert.ToInt32(actionIDElement.Value), dbSite.ID, valueElement.Value));
                    }
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }

            return actions;
        }

        public List<Computer> GetComputers()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetComputers()");

            List<Computer> computers = new List<Computer>();

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            RestRequest request = new RestRequest("computers", Method.GET);
            
            try
            {
                computers.AddRange(Execute<List<Computer>>(request));

                foreach (Computer computer in computers)
                {
                    request = new RestRequest("computer/{id}", Method.GET);
                    request.AddUrlSegment("id", computer.ComputerID.ToString());

                    XDocument response = Execute(request);
                    string hostName = response.Element("BESAPI").Element("Computer").Elements("Property")
                        .Where(e => e.Attribute("Name").Value.ToString() == "Computer Name").Single().Value.ToString();
                    computer.ComputerName = hostName;
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }

            return computers;
        }

        public List<Site> GetSites()
        {
            List<Site> sites = new List<Site>();

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            RestRequest request = new RestRequest("sites", Method.GET);

            try
            {
                // Execute the request
                XDocument response = Execute(request);

                foreach (XElement siteElement in response.Element("BESAPI").Elements())
                {
                    if (siteElement.Name.ToString() == "ActionSite")
                    {
                        sites.Add(new Site(siteElement.Element("Name").Value.ToString(), "master"));
                    }
                    else
                    {
                        sites.Add(new Site(siteElement.Element("Name").Value.ToString(), siteElement.Name.ToString().Replace("Site", "").ToLower()));
                    }

                }
            }
            catch(Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
            
            return sites;
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
            catch (Exception e)
            {
                throw;
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
            catch (Exception e)
            {
                throw;
            }

            return response.Data;
        }
    }
}
