using System;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using HtmlAgilityPack;

namespace Parser
{
    static class GitHub
    {
        private static List<string> users = new List<string>();
        private static HttpWebRequest request;

        private static void GetUsersList(int page, string string_request, string type_request)
        {
            string URL = $"https://github.com/search?o=desc&p={page}&q={string_request}&s={type_request}&type=Users";
            request = (HttpWebRequest)WebRequest.Create(URL);

            request.Headers.Add("Cookie", "hth7bXLmHUJH26T0k4bMq_BG78PrRGVUmj2eSgzEpYewSDCM");
            request.Headers.Add("Host", "github.com");

            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch
            {
                return;
            }
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            HtmlDocument doc = new HtmlDocument();
            doc.Load(sr);
            sr.Close();
            HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//a[contains(@class, 'd-table')]");
            foreach (HtmlNode node in collection)
            {
                if (node.GetAttributeValue("data-hovercard-type", "none") == "organization") continue;
                else
                {
                    string to_list = node.GetAttributeValue("data-hovercard-url", "none");
                    if (to_list == "none") continue;
                    else
                    {
                        string[] values = to_list.Split('/');
                        users.Add(values[2]);
                    }
                }
            }
            System.Threading.Thread.Sleep(1000);
        }


        private static void GetDataTable()
        {
            for (int i = 0; i < users.Count; i++)
            {
                string URL = $"https://github.com/{users[i]}";
                request = (HttpWebRequest)WebRequest.Create(URL);
                request.Headers.Add("Cookie", "user_session=hth7bXLmHUJH26T0k4bMq_BG78PrRGVUmj2eSgzEpYewSDCM;");
                request.Headers.Add("Host", "github.com");

                WebResponse response = request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                HtmlDocument doc = new HtmlDocument();
                doc.Load(sr);
                sr.Close();
                CSVWrite(FindElements(doc));
            }
        }

        private static void GetAllRequest(string string_request, string type_request)
        {
            // Тут нужно до 100
            for (int i = 1; i < 2; i++)
            {
                GetUsersList(i, string_request, type_request);
                System.Threading.Thread.Sleep(2000);
            }
        }

        public static void ParseDB(string string_request, string type_request)
        {
            GetAllRequest(string_request, type_request);
            GetDataTable();
        }

        private static void CSVWrite(List<string> row)
        {
            using (var writer = new StreamWriter("githubData.csv", true, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.NextRecord();
                csvWriter.WriteField(row);
                writer.Flush();
            }
        }

        private static List<string> FindElements(HtmlDocument doc)
        {
            List<string> anotherRow = new List<string>();
            HtmlNode profileContriner = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'js-profile-editable-area')]");
            FindBio(profileContriner, anotherRow);
            FindPeoples(profileContriner, anotherRow);
            FindList(profileContriner, anotherRow);
            return anotherRow;
        }

        private static void FindBio(HtmlNode profileContriner, List<string> anotherRow)
        {
            HtmlNode profileBio = profileContriner.SelectSingleNode("//div[contains(@class, 'p-note user-profile-bio')]");
            if (profileBio != null)
            {
                anotherRow.Add(profileBio.GetAttributeValue("data-bio-text", "none"));
            }
        }

        private static void FindPeoples(HtmlNode profileContriner, List<string> anotherRow)
        {
            HtmlNode profileNumbers = profileContriner.SelectSingleNode("//div[contains(@class, 'flex-order-1 flex-md-order-none mt-2 mt-md-0')]");
            if (profileNumbers != null)
            {
                HtmlDocument innerDoc = new HtmlDocument();
                innerDoc.LoadHtml(profileNumbers.InnerHtml);
                HtmlNodeCollection innerCollection = innerDoc.DocumentNode.SelectNodes("//span");
                if (innerCollection != null)
                {
                    for (int j = 0; j < innerCollection.Count; j++)
                    {
                        anotherRow.Add(innerCollection[j].InnerText);
                    }
                }
                else
                {
                    anotherRow.Add("0");
                    anotherRow.Add("0");
                    anotherRow.Add("0");
                }
            }
        }

        private static void FindList(HtmlNode profileContriner, List<string> anotherRow)
        {
            HtmlNode profileList = profileContriner.SelectSingleNode("//ul[contains(@class, 'vcard-details')]");
            if (profileList != null)
            {
                HtmlDocument innerDoc = new HtmlDocument();
                innerDoc.LoadHtml(profileList.OuterHtml);
                HtmlNodeCollection innerCollection = innerDoc.DocumentNode.SelectNodes("//li");

                HtmlNode p_org = innerDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'p-org')]");
                if (p_org != null)
                    anotherRow.Add(p_org.InnerText);
                else
                    anotherRow.Add("none");

                HtmlNode p_label = innerDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'p-label')]");
                if (p_label != null)
                    anotherRow.Add(p_label.InnerText);
                else
                    anotherRow.Add("none");

                HtmlNode u_email = innerDoc.DocumentNode.SelectSingleNode("//a[contains(@class, 'u-email Link--primary')]");
                if (u_email != null)
                {
                    var plainText = ConvertToPlainText(u_email.InnerText);
                    anotherRow.Add(plainText);
                }
                else
                    anotherRow.Add("none");

                HtmlNode link = innerDoc.DocumentNode.SelectSingleNode("//li[contains(@data-test-selector, 'profile-website-url')]");
                if (link != null)
                {
                    var plainText = ConvertToPlainText(link.SelectSingleNode("//a[contains(@class, 'Link--primary ')]").InnerText);
                    anotherRow.Add(plainText);
                }
                else
                    anotherRow.Add("none");

                //HtmlNode twitter = innerDoc.DocumentNode.SelectSingleNode("//li[contains(@itemprop, 'twitter')]");
                //if (twitter != null)
                //    anotherRow.Add(link.SelectSingleNode("//a[contains(@class, 'Link--primary ')]").InnerText);
                //else
                //    anotherRow.Add("none");
            }
        }

        public static string ConvertToPlainText(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "br":
                            outText.Write("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }
    }
}
