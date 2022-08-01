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
   
    class Program
    {
        static void Main(string[] args)
        {
            //Console.Write("Input desired category: ");
            //string input = Console.ReadLine();
            //Console.Write("Input desired type of matching(empty if best matched): ");
            //string type = Console.ReadLine();
            //GitHub.ParseDB(input, type);



            MarketGuru ozon = new MarketGuru();
            ozon.Run();

            //DEFY.ParseMask();
            //StepApp.Run();
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
