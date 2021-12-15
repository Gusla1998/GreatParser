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

namespace Ozon
{
   
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Input desired category: ");
            string input = Console.ReadLine();
            Console.Write("Input desired type of matching(empty if best matched): ");
            string type = Console.ReadLine();
            GitHub.ParseDB(input, type);



            //MarketGuru ozon = new MarketGuru();
            ////ozon.JsonObjectParse();
            //ozon.FindSex(ozon.CSV_READ("check.csv"));

            //Ozon ozon = new Ozon();
            //ozon.GetAllRequests();
        }
    }
    class MarketGuru
    {
        public int counter = 0;
        private string BASE_URL = "https://marketguru.io:3000/api/v2/competitors/categories/stats";
        private string TREE_URL = "https://marketguru.io:3000/api/v2/competitors/tree/";
        HttpWebRequest request;
        List<JToken> list;
        public MarketGuru()
        {
            list = new List<JToken>();
            request = (HttpWebRequest)WebRequest.Create(BASE_URL);
            request.Method = "GET";
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Content-Length", "7967");
            request.Headers.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjlmM2ViMjlhLTc4ZjMtNGM1YS1iOGJhLWQ5ZWM3Nzc3N2EzMiIsImlhdCI6MTYzMTMwNDkzMiwiZXhwIjoxNjQ2ODU2OTMyfQ.eOzaalOiP40BAhDSbcrcq-rHUeHI_iY6PcP9MWLAnpc");
        }        
        public void JsonObjectParse()
        {             
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            JArray json = JArray.Parse(result);
            JObject json_object = new JObject();
            foreach (var category in json)
            {                
                request = (HttpWebRequest)WebRequest.Create(BASE_URL + "?parentId=" + category["id"]);
                ResetHeaders(request);
                response = request.GetResponse();
                sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                result = sr.ReadToEnd();
                sr.Close();
                json = JArray.Parse(result);
                foreach (var subcategory in json)
                {
                    request = (HttpWebRequest)WebRequest.Create(TREE_URL + subcategory["id"]);
                    ResetHeaders(request);
                    response = request.GetResponse();
                    sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                    result = sr.ReadToEnd();
                    sr.Close();
                    json_object = JObject.Parse(result);                    
                    List_Search(json_object);
                }
            }
            response.Close();
        }
        private void ResetHeaders(HttpWebRequest request)
        {
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Content-Length", "7967");
            request.Headers.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjBkYmU2MmY5LTM2YmMtNDE4NC04MmZmLTEzYTc3ZjM3MWJiNSIsImlhdCI6MTYyOTYyMDUwOCwiZXhwIjoxNjQ1MTcyNTA4fQ.mmSASL8aKi4CLbuC8fM7mHhOMJfBYAny2m3l7HbHQ8U");
        }
        private JArray Request_For_Cost(JToken category)
        {
            request = (HttpWebRequest)WebRequest.Create(BASE_URL + "?parentId=" + category["id"]);
            ResetHeaders(request);
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            JArray json = JArray.Parse(result);

            return json;
        }
        private void List_Search(JObject json_object)
        {
            try
            {
                //Это то что имеет только 2 уровня
                if (json_object["category"]["title"].ToString() == "Банты") return;
                if (json_object["category"]["title"].ToString() == "Для курения") return;
                if (json_object["category"]["title"].ToString() == "Шторы") return;
                if (json_object["category"]["title"].ToString() == "Запчасти") return;
                if (json_object["category"]["title"].ToString() == "Масла и жидкости") return;
                if (json_object["category"]["title"].ToString() == "Инструменты") return;
                if (json_object["category"]["title"].ToString() == "Мототовары") return;
                if (json_object["category"]["title"].ToString() == "Автоаксессуары и дополнительное оборудование") return;

                IJEnumerable<JToken> nodes = json_object["category"]["childNodes"];
                int i = 0;
                foreach (var node in nodes)
                {
                    if (node["childNodes"] != null)
                    {
                        IJEnumerable<JToken> nodes_deep = node["childNodes"];
                        int j = 0;
                        foreach (var node_deep in nodes_deep)
                        {
                            if (node_deep["childNodes"] != null)
                            {
                                IJEnumerable<JToken> nodes_deep_deep = node_deep["childNodes"];
                                int k = 0;
                                foreach (var node_deep_deep in nodes_deep_deep)
                                {
                                    if (node_deep_deep["childNodes"] != null)
                                    {
                                        IJEnumerable<JToken> nodes_deep_deep_deep = node_deep_deep["childNodes"];
                                        int z = 0;
                                        foreach (var node_deep_deep_deep in nodes_deep_deep_deep)
                                        {
                                            if (node_deep_deep_deep["childNodes"] != null)
                                            {
                                                counter++;
                                            }
                                            //Отсюда парсим 6 уровень
                                            else
                                            {
                                                JArray for_this_cat = Request_For_Cost(node_deep_deep);
                                                CSV_WRITE(for_this_cat[z]);
                                            }
                                            z++;
                                        }
                                    }
                                    //Отсюда парсим 5 уровень
                                    else
                                    {
                                        JArray for_this_cat = Request_For_Cost(node_deep);
                                        CSV_WRITE(for_this_cat[k]);
                                    }
                                    k++;
                                }
                            }
                            //Отсюда парсим 4 уровень
                            else
                            {
                                JArray for_this_cat = Request_For_Cost(node);
                                CSV_WRITE(for_this_cat[j]);
                            }
                            j++;
                        }
                    }
                    //Отсюда парсим 3 уровень
                    else
                    {
                        JArray for_this_cat = Request_For_Cost(json_object["category"]);
                        CSV_WRITE(for_this_cat[i]);
                    }
                    i++;
                }
            }
            catch(Exception e)
            {
                return;
            }
        }
        private void CSV_WRITE(JToken stroka)
        {
            using (var writer = new StreamWriter("checknew.csv", true, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.NextRecord();

                List<string> record = new List<string>();
                record.Add(stroka["avgArticlesCount"].ToString());
                record.Add(stroka["avgPrices"].ToString());
                record.Add(stroka["avgProfit"].ToString());
                record.Add(stroka["avgProfitPerItem"].ToString());
                record.Add(stroka["avgSalesCount"].ToString());
                record.Add(stroka["id"].ToString());
                record.Add(stroka["path"].ToString());
                record.Add(stroka["profit"].ToString());
                record.Add(stroka["salesCount"].ToString());
                record.Add(stroka["title"].ToString());
                record.Add(stroka["url"].ToString());
                csvWriter.WriteField(record);

                writer.Flush();
            }
        }
        public List<Record> CSV_READ(string file)
        {
            List<Record> records = new List<Record>();
            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                while (csv.Read())
                {
                    Record sample = new Record();
                    sample.CountProduct = float.Parse(csv.GetField<string>(0).Equals("") ? "0" : csv.GetField<string>(0));
                    sample.AveragePrice = float.Parse(csv.GetField<string>(1).Equals("") ? "0" : csv.GetField<string>(1));
                    sample.MonthEarn = float.Parse(csv.GetField<string>(7).Equals("") ? "0" : csv.GetField<string>(7));
                    sample.DayEarn = float.Parse(csv.GetField<string>(2).Equals("") ? "0" : csv.GetField<string>(2));
                    sample.MonthSale = float.Parse(csv.GetField<string>(8).Equals("") ? "0" : csv.GetField<string>(8));
                    sample.DaySale = float.Parse(csv.GetField<string>(4).Equals("") ? "0" : csv.GetField<string>(4));
                    sample.ProductEarn = float.Parse(csv.GetField<string>(3).Equals("") ? "0" : csv.GetField<string>(3));
                    sample.Name = csv.GetField<string>(6);

                    records.Add(sample);
                }
            }
            return records;
        }
        public void FindSex(List<Record> records)
        {
            foreach(var record in records)
            {
                //////Тут менять эти 2 числа///////////////////////////////////////////////////
                if (record.CountProduct <= 600 && record.DayEarn >= 400000) Console.WriteLine(record.Name);
            }
        }
        public class Record
        {
            public float CountProduct { get; set; }
            public float AveragePrice { get; set; }
            public float MonthEarn { get; set; }
            public float DayEarn { get; set; }
            public float MonthSale { get; set; }
            public float DaySale { get; set; }
            public float ProductEarn { get; set; }
            public string Name { get; set; }
        }
    }

    class Ozon
    {
        public int counter = 0;
        HttpWebRequest request;
        List<JToken> list;
        public Ozon()
        {
            list = new List<JToken>();         
        }

        private HtmlDocument OzonRequest(string URL)
        {
            ///// Тут заебись обработать
            try
            {
                request = (HttpWebRequest)WebRequest.Create(URL);
                request.Headers.Add("Host", "www.ozon.ru");
                WebResponse response = request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                HtmlDocument doc = new HtmlDocument();
                doc.Load(sr);
                sr.Close();
                return doc;
            }
            catch(WebException e)
            {
                return null;
            }
            catch
            {
                return null;
            }
        }
        private List<string> GetInitialCategory()
        {
            List<string> catUrls = new List<string>();
            HtmlDocument doc = OzonRequest($"https://www.ozon.ru/category");
            HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//a[contains(@class, 'a3p5 g5s4 g5s6')]");
            foreach (HtmlNode node in collection)
            {
                if (node.OuterHtml.Contains("category"))
                    catUrls.Add(node.GetAttributeValue("href","none"));
            }
            return catUrls;
        }

        private void GetCategory(string URL)
        {
            HtmlDocument doc = OzonRequest(URL);
            //HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//a[contains(@class, 'a3p5 g5s4 g5s6')]");
            //foreach (HtmlNode node in collection)
            //{
            //    if (node.OuterHtml.Contains("category"))
            //        catUrls.Add(node.GetAttributeValue("href", "none"));
            //}
            //return catUrls;
            /// Поехали вниз по категориям
        }
        private void GetRequest(int i)
        {
            request = (HttpWebRequest)WebRequest.Create($"https://www.ozon.ru/category/{i}");
            request.Headers.Add("Host", "www.ozon.ru");
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            HtmlDocument doc = new HtmlDocument();
            doc.Load(sr);
            sr.Close();

            HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//a[contains(@class, 'cw cw1')]");

            //// Обработать если бан прилетел размер меньше 900 байт
            ///
            if (collection != null)
            {
                HtmlNode node = collection[0].ParentNode.ParentNode;
                int check = 0;
                if (node.OuterHtml.Contains("cv7") && !node.OuterHtml.Contains("cw2")){
                    collection = doc.DocumentNode.SelectNodes("//a[contains(@class, 'cw cw1')]");
                    string href = collection[0].GetAttributeValue("href", "none");
                    collection = doc.DocumentNode.SelectNodes("//div[contains(@class, 'f9j0')]");
                    string count = collection[0].OuterHtml;
                    //// Отсюда потянем бестселлер и количество отзывов
                    collection = doc.DocumentNode.SelectNodes("//div[contains(@class, 'bi3 bi5')]");
                    //string f = collection[0].GetAttributeValue("href", "none");
                    //string g = collection[0].GetAttributeValue("href", "none");
                    list.Add(href);
                    list.Add(count);
                }    
            }
            System.Threading.Thread.Sleep(2000);
        }

        public void GetAllRequests()
        {
            List<string> catURLs = GetInitialCategory();
            for (int i = 0; i < catURLs.Count; i++)
            {
                GetCategory($"https://www.ozon.ru" + catURLs[i]);
                //GetRequest(i);
            }
        }
    }
}
