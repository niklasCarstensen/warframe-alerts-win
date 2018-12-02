using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Xml;
using WarframeNET;

namespace Warframe_Alerts
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public static class WarframeHandler
    {
        public static WorldState worldState;

        public static string GetXml(ref string ret)
        {
            if (ret == null) throw new ArgumentNullException(nameof(ret));
            try
            {
                var request = WebRequest.Create("http://content.warframe.com/dynamic/rss.php");
                var response = request.GetResponse();
                var status = (((HttpWebResponse)response).StatusDescription);
                ret = status;
                var dataStream = response.GetResponseStream();
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    var responseFromServer = reader.ReadToEnd();
                    return responseFromServer;
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                ret = "ERROR";
            	return "An Error Occurred: " + ex.Message;
            }
        }

        public static string GetJson(ref string ret)
        {
            if (ret == null) throw new ArgumentNullException(nameof(ret));
            try
            {
                var request = WebRequest.Create("https://ws.warframestat.us/pc/");
                var response = request.GetResponse();
                var status = (((HttpWebResponse)response).StatusDescription);
                ret = status;
                var dataStream = response.GetResponseStream();
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    var responseFromServer = reader.ReadToEnd();
                    return responseFromServer;
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                ret = "ERROR";
                return "An Error Occurred: " + ex.Message;
            }
        }

        public static void GetJsonObjects(string response)
        {
            worldState = JsonConvert.DeserializeObject<WorldState>(response);
        }

        public static void GetXMLObjects(string response, ref List<Alert> alerts, ref List<Invasion> invasions, ref List<Outbreak> outbreaks)
        {
            var doc = new XmlDocument();
            doc.LoadXml(response);

            var itemList = doc.SelectNodes("/rss/channel/item");

            foreach (XmlNode item in itemList)
            {
                var author = item["author"].InnerText;

                switch (author)
                {
                    case "Alert":
                    {
                        Alert temp;
                        
                        temp.ID = item["guid"].InnerText;
                        temp.Title = item["title"].InnerText;
                        temp.Description = item["description"].InnerText;
                        temp.StartDate = item["pubDate"].InnerText;
                        
                        var fParse = item["wf:faction"].InnerText.Split('_');
                        var faction = fParse[1].ToLower();
                        
                        temp.Faction = faction.Substring(0, 1).ToUpper() + faction.Substring(1, faction.Length - 1);
                        temp.ExpiryDate = item["wf:expiry"].InnerText;
                        
                        alerts.Add(temp);
                    }
                        break;

                    case "Invasion":
                    {
                        Invasion temp;
                        
                        temp.ID = item["guid"].InnerText;
                        temp.Title = item["title"].InnerText;
                        temp.StartDate = item["pubDate"].InnerText;
                        
                        invasions.Add(temp);
                    }
                        break;

                    case "Outbreak":
                    {
                        Outbreak temp;
                        
                        temp.ID = item["guid"].InnerText;
                        temp.Title = item["title"].InnerText;
                        temp.StartDate = item["pubDate"].InnerText;
                        
                        outbreaks.Add(temp);
                    }
                        break;
                }
            }
        }
    }
}
