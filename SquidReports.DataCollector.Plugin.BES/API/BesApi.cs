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

        public List<ActionDetail> GetActionDetails()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetActionDetails()");

            IEnumerable<Model.Action> actions = DbRelay.Get<Model.Action>();
            List<ActionDetail> details = new List<ActionDetail>();

            foreach (Model.Action action in actions)
            {
                details.Add(GetActionDetails(action));
            }

            return details;
        }

        public ActionDetail GetActionDetails(Model.Action action)
        {
            ActionDetail detail = new ActionDetail();

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            RestRequest request = new RestRequest("action/{id}/status", Method.GET);
            request.AddUrlSegment("id", action.ActionID.ToString());

            try
            {
                // Execute the request
                XDocument response = Execute(request);

                detail = new ActionDetail(
                                Int32.Parse(response.Element("BESAPI").Element("ActionResults").Element("ActionID").Value.ToString()),
                                response.Element("BESAPI").Element("ActionResults").Element("Status").Value.ToString(),
                                response.Element("BESAPI").Element("ActionResults").Element("DateIssued").Value.ToString());
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }

            return detail;
        }

        public List<ActionResult> GetActionResults()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetActionResults()");

            IEnumerable<Model.Action> actions = DbRelay.Get<Model.Action>();
            List<ActionResult> results = new List<ActionResult>();

            foreach (Model.Action action in actions)
            {
                results.AddRange(GetActionResults(action));
            }

            return results;
        }

        public List<ActionResult> GetActionResults(Model.Action action)
        {
            List<ActionResult> results = new List<ActionResult>();
            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            RestRequest request = new RestRequest("action/{id}/status", Method.GET);
            request.AddUrlSegment("id", action.ActionID.ToString());

            try
            {
                // Execute the request
                XDocument response = Execute(request);

                foreach (XElement computerElement in response.Element("BESAPI").Element("ActionResults").Elements("Computer"))
                {
                    DateTime startTime = new DateTime();
                    DateTime endTime = new DateTime();

                    if (computerElement.Element("StartTime") != null)
                    {
                        startTime = Convert.ToDateTime(computerElement.Element("StartTime").Value.ToString());
                    }
                    if (computerElement.Element("EndTime") != null)
                    {
                        endTime = Convert.ToDateTime(computerElement.Element("EndTime").Value.ToString());
                    }

                    results.Add(new ActionResult(
                                        action.ActionID,                                                            // Action ID
                                        Int32.Parse(computerElement.Attribute("ID").Value.ToString()),              // Computer ID
                                        computerElement.Element("Status").Value.ToString(),                         // Status
                                        Int32.Parse(computerElement.Element("ApplyCount").Value.ToString()),        // Times applied
                                        Int32.Parse(computerElement.Element("RetryCount").Value.ToString()),        // Times retried
                                        Int32.Parse(computerElement.Element("LineNumber").Value.ToString()),        // Which script line is being executed
                                        // Time execution started
                                        (computerElement.Element("StartTime") != null) ? Convert.ToDateTime(computerElement.Element("StartTime").Value.ToString()) : (DateTime?)null,
                                        // Time execution started
                                        (computerElement.Element("EndTime") != null) ? Convert.ToDateTime(computerElement.Element("EndTime").Value.ToString()) : (DateTime?)null
                        ));
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }         

            return results;
        }

        public List<Analysis> GetAnalyses()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetAnalyses()");

            IEnumerable<Site> sites = DbRelay.Get<Site>();
            List<Analysis> analyses = new List<Analysis>();

            foreach (Site site in sites)
            {
                analyses.AddRange(GetAnalyses(site));
            }

            return analyses;
        }

        public List<Analysis> GetAnalyses(Site site)
        {
            List<Analysis> analyses = new List<Analysis>();

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            RestRequest request = new RestRequest("analyses/{sitetype}/{site}", Method.GET);
            request.AddUrlSegment("sitetype", site.Type);
            request.AddUrlSegment("site", site.Name);

            // TODO: Handle master action site properly
            if (site.Type == "master")
            {
                request = new RestRequest("analyses/{sitetype}", Method.GET);
                request.AddUrlSegment("sitetype", site.Type);
            }

            try
            {
                analyses.AddRange(Execute<List<Analysis>>(request));

                foreach (Analysis analysis in analyses)
                {
                    analysis.SiteID = site.ID;
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }

            return analyses;
        }

        public List<AnalysisProperty> GetAnalysisProperties()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetAnalysisProperties()");

            IEnumerable<Analysis> analyses = DbRelay.Get<Analysis>();
            List<AnalysisProperty> properties = new List<AnalysisProperty>();

            foreach (Analysis analysis in analyses)
            {
                properties.AddRange(GetAnalysisProperties(analysis));
            }

            return properties;
        }

        public List<AnalysisProperty> GetAnalysisProperties(Analysis analysis)
        {
            List<AnalysisProperty> properties = new List<AnalysisProperty>();

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            // The API does not assign an ID to the Site. Therefore, we use the ID assigned by the DB.
            // For this reason we're fetching the list of sites from the DB again, so we can resolve ID->Name
            Site dbSite = DbRelay.Get<Site>(new { ID = analysis.SiteID }).Single();

            RestRequest request = new RestRequest("analysis/{sitetype}/{site}/{analysisid}", Method.GET);
            request.AddUrlSegment("sitetype", dbSite.Type);
            request.AddUrlSegment("site", dbSite.Name);
            request.AddUrlSegment("analysisid", analysis.AnalysisID.ToString());

            try
            {
                XDocument response = Execute(request);

                foreach (XElement propertyElement in response.Element("BES").Element("Analysis").Elements("Property"))
                {
                    properties.Add(new AnalysisProperty(
                                                analysis.ID,
                                                Convert.ToInt32(propertyElement.Attribute("ID").Value),
                                                propertyElement.Attribute("Name").Value)
                                            );
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
            
            return properties;
        }

        public List<AnalysisPropertyResult> GetAnalysisPropertyResults()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetAnalysisPropertyResults()");

            IEnumerable<AnalysisProperty> properties = DbRelay.Get<AnalysisProperty>();
            List<AnalysisPropertyResult> results = new List<AnalysisPropertyResult>();

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            foreach (AnalysisProperty property in properties)
            {
                results.AddRange(GetAnalysisPropertyResults(property));
            }

            return results;
        }

        public List<AnalysisPropertyResult> GetAnalysisPropertyResults(AnalysisProperty property)
        {
            this.Logger.LogMessage(LogLevel.Trace, String.Format("Collecting Property - {0}: Property: {1}", property.ID, property.Name));

            List<AnalysisPropertyResult> results = new List<AnalysisPropertyResult>();

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            // We need to use Session Relevance to acquire property results
            // We'll use the following Relevance query:
            // {0}: The SequenceNo/Source ID of the Analysis property
            // {1}: The Name of the Analysis
            string relevance = "((id of it) of computer of it, values of it) of results from (BES Computers) of BES Properties whose ((source id of it = {0}) and (name of source analysis of it = \"{1}\"))";

            try
            {
                // Unfortunately, we'll also need the name of the Parent Analysis. For that, we'll need to query the DB
                Analysis dbAnalysis = DbRelay.Get<Analysis>(new { ID = property.AnalysisID }).Single();

                // Let's compose the request string
                RestRequest request = new RestRequest("query", Method.GET);
                //request.AddQueryParameter("relevance", String.Format(relevance, computer.ComputerID.ToString(), property.SequenceNo.ToString(), analysis.Name));
                request.AddQueryParameter("relevance", String.Format(relevance, property.SequenceNo.ToString(), dbAnalysis.Name));

                XDocument response = Execute(request);

                // Let's check if the Result element is empty
                if (response.Element("BESAPI").Element("Query").Element("Result").Elements().Count() > 0)
                {
                    // All answers are wrapped inside a "Tuple" element
                    foreach (XElement tupleElement in response.Element("BESAPI").Element("Query").Element("Result").Elements("Tuple"))
                    {
                        // The Result consists of two parts:
                        //  1) The ComputerID
                        //  2) The value of the retrieved property sequence for said ComputerID
                        XElement computerElement = tupleElement.Elements("Answer").First();
                        XElement valueElement = tupleElement.Elements("Answer").Last();
                        results.Add(new AnalysisPropertyResult(property.ID, Convert.ToInt32(computerElement.Value.ToString()), valueElement.Value.ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }

            return results;
        }

        public List<Baseline> GetBaselines()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetBaselines()");

            IEnumerable<Site> sites = DbRelay.Get<Site>();
            List<Baseline> baselines = new List<Baseline>();
            
            // Loop through the complete list of sites provided
            foreach (Site site in sites)
            {
                baselines.AddRange(GetBaselines(site));
            }

            return baselines;
        }

        public List<Baseline> GetBaselines(Site site)
        {
            List<Baseline> baselines = new List<Baseline>();

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            // The list of baselines is contained within the site content
            RestRequest request = new RestRequest("site/{sitetype}/{site}/content", Method.GET);
            request.AddUrlSegment("sitetype", site.Type);
            request.AddUrlSegment("site", site.Name);

            // TODO: Handle master action site properly
            if (site.Type == "master")
            {
                request = new RestRequest("site/{sitetype}/content", Method.GET);
                request.AddUrlSegment("sitetype", site.Type);
            }

            try
            {
                XDocument response = Execute(request);

                if (response.Element("BESAPI").Elements("Baseline") != null)
                {
                    foreach (XElement baselineElement in response.Element("BESAPI").Elements("Baseline"))
                    {
                        baselines.Add(new Baseline(
                                        Convert.ToInt32(baselineElement.Element("ID").Value),
                                        site.ID,
                                        baselineElement.Element("Name").Value
                                    ));
                    }
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }

            return baselines;
        }

        public List<BaselineResult> GetBaselineResults()
        {
            IEnumerable<Baseline> baselines = DbRelay.Get<Baseline>();
            List<BaselineResult> results = new List<BaselineResult>();

            foreach (Baseline baseline in baselines)
            {
                results.AddRange(GetBaselineResults(baseline));
            }

            return results;
        }

        public List<BaselineResult> GetBaselineResults(Baseline baseline)
        {
            List<BaselineResult> results = new List<BaselineResult>();

            // We need to acquire some info concerning the site
            // Let's fetch the site object now
            Site dbSite = DbRelay.Get<Site>(new { ID = baseline.SiteID }).Single();

            // The list of baselines is contained within the site content
            RestRequest request = new RestRequest("fixlet/{sitetype}/{site}/{baselineid}/computers", Method.GET);
            request.AddUrlSegment("sitetype", dbSite.Type);
            request.AddUrlSegment("site", dbSite.Name);
            request.AddUrlSegment("baselineid", baseline.BaselineID.ToString());

            // TODO: Handle master action site properly
            if (dbSite.Type == "master")
            {
                request = new RestRequest("fixlet/{sitetype}/{site}/{baselineid}/computers", Method.GET);
                request.AddUrlSegment("sitetype", dbSite.Type);
                request.AddUrlSegment("baselineid", baseline.BaselineID.ToString());
            }

            try
            {
                XDocument response = Execute(request);

                // The returned document should contain 0 or more Computer resource URIs
                if (response.Element("BESAPI").Elements().Count(e => e.Name == "Computer") > 0)
                {
                    foreach (XElement computerElement in response.Element("BESAPI").Elements("Computer"))
                    {
                        // We only need the last part of the resource URI -- the ID
                        Uri resourceUri = new Uri(computerElement.Attribute("Resource").Value.ToString());
                        results.Add(new BaselineResult(baseline.BaselineID, Int32.Parse(resourceUri.Segments.Last())));
                    }
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }

            return results;
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

        public List<ComputerGroup> GetComputerGroups()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetComputerGroups()");

            List<ComputerGroup> groups = new List<ComputerGroup>();

            foreach (Site dbSite in DbRelay.Get<Site>())
            {
                groups.AddRange(GetComputerGroups(dbSite));
            }

            return groups;
        }

        public List<ComputerGroup> GetComputerGroups(Site site)
        {
            List<ComputerGroup> groups = new List<ComputerGroup>();

            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            RestRequest request = new RestRequest("computergroups/{sitetype}/{site}", Method.GET);
            request.AddUrlSegment("sitetype", site.Type);
            request.AddUrlSegment("site", site.Name);

            // TODO: Handle master action site properly
            if (site.Type == "master")
            {
                request = new RestRequest("computergroups/{sitetype}", Method.GET);
                request.AddUrlSegment("sitetype", site.Type);
            }      

            try
            {
                groups.AddRange(Execute<List<ComputerGroup>>(request));

                // The API does not assign an ID to the Site. Therefore, we use the ID assigned by the DB.
                // Let's fetch the Site from the DB first
                Site dbSite = DbRelay.Get<Site>(new { Name = site.Name }).Single();
                
                // Assign SiteID if the corresponding Site was found in the DB
                foreach (ComputerGroup group in groups)
                {
                    if (dbSite != null)
                    {
                        group.SiteID = dbSite.ID;
                    }

                    // Check if it's a manual group
                    group.Manual = IsManualGroup(group);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }

            return groups;
        }

        private bool IsManualGroup(ComputerGroup group)
        {
            RestClient client = new RestClient(this.BaseURL);
            client.Authenticator = this.Authenticator;

            // We need to use Session Relevance to check if a group is manual or not
            // We'll use the following Relevance query:
            // {0}: The ID of the Computer Group
            string relevance = "(manual flag of it) of BES computer groups whose (id of it = {0})";

            // Let's compose the request string
            RestRequest request = new RestRequest("query", Method.GET);
            request.AddQueryParameter("relevance", String.Format(relevance, group.GroupID));

            try
            {
                XDocument response = Execute(request);

                // Let's check if the Result element is empty
                if (response.Element("BESAPI").Element("Query").Element("Result").Elements().Count() > 0)
                {
                    return Convert.ToBoolean(response.Element("BESAPI").Element("Query").Element("Result").Element("Answer").Value);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }      

            return false;
        }

        public List<ComputerGroupMember> GetGroupMembers()
        {
            this.Logger.LogMessage(LogLevel.Info, "Starting GetGroupMembers()");

            IEnumerable<ComputerGroup> groups = DbRelay.Get<ComputerGroup>();
            List<ComputerGroupMember> members = new List<ComputerGroupMember>();

            foreach (ComputerGroup group in groups)
            {
                members.AddRange(GetGroupMembers(group));
            }

            return members;
        }

        public List<ComputerGroupMember> GetGroupMembers(ComputerGroup group)
        {
            List<ComputerGroupMember> members = new List<ComputerGroupMember>();

            // Let's fetch the relevant Site from the DB
            Site dbSite = DbRelay.Get<Site>(new { ID = group.SiteID }).Single();

            if (dbSite != null)
            {
                if (group.Manual)
                {
                    // If it's a manual group, we need to collect the group members using Relevance FOR SOME REASON
                    // We'll use the following Relevance query:
                    // {0}: The ID of the Computer Group
                    string relevance = "((id of it, name of it) of members of it) of BES Computer Group whose (id of it = {0})";

                    // Let's compose the request string
                    RestRequest request = new RestRequest("query", Method.GET);
                    request.AddQueryParameter("relevance", String.Format(relevance, group.GroupID));

                    try
                    {
                        XDocument response = Execute(request);

                        // Let's check if the Result element is empty
                        if (response.Element("BESAPI").Element("Query").Element("Result").Elements().Count() > 0)
                        {
                            // All answers are wrapped inside a "Tuple" element
                            foreach (XElement tupleElement in response.Element("BESAPI").Element("Query").Element("Result").Elements("Tuple"))
                            {
                                // The Result consists of two parts:
                                //  1) The ComputerID
                                //  2) The ComputerName (name for debug purposes)
                                XElement computerElement = tupleElement.Elements("Answer").First();
                                members.Add(new ComputerGroupMember(group.GroupID, Convert.ToInt32(computerElement.Value)));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.Logger.LogException(LogLevel.Error, e.Message, e);
                    }                   
                }
                else
                {
                    RestClient client = new RestClient(this.BaseURL);
                    client.Authenticator = this.Authenticator;

                    RestRequest request = new RestRequest("computergroup/{sitetype}/{site}/{id}/computers", Method.GET);
                    request.AddUrlSegment("sitetype", dbSite.Type);
                    request.AddUrlSegment("site", dbSite.Name);
                    request.AddUrlSegment("id", group.GroupID.ToString());

                    try
                    {
                        XDocument response = Execute(request);

                        if (response.Element("BESAPI").Elements("Computer") != null)
                        {
                            foreach (XElement computerElement in response.Element("BESAPI").Elements("Computer"))
                            {
                                Uri resourceUri = new Uri(computerElement.Attribute("Resource").Value.ToString());
                                members.Add(new ComputerGroupMember(group.GroupID, Int32.Parse(resourceUri.Segments.Last())));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.Logger.LogException(LogLevel.Error, e.Message, e);
                    }
                }
            }

            return members;
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
