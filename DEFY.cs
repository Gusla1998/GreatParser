using System;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using HtmlAgilityPack;


namespace Parser
{
    static class DEFY {
        private static List<string> users = new List<string>();
        private static HttpWebRequest request;

        private static string GetMask(string number) {
            string URL = $"https://polygon-rpc.com/";
            request = (HttpWebRequest)WebRequest.Create(URL);
            
            request.Accept = "*/*";
            request.Host = "polygon-rpc.com";
            request.ContentType = "application/json";
            request.Method = "POST";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{ \"jsonrpc\":\"2.0\"," +
                              "\"id\":1,\"method\":\"eth_call\"," +
                              "\"params\":[{ \"from\":\"0x0000000000000000000000000000000000000000\"," +
                              $"\"data\":\"0xd435003700000000000000000000000000000000000000000000000000000000000{number}\"," +
                              "\"to\":\"0xfd257ddf743da7395470df9a0517a2dfbf66d156\"},\"latest\"]}";
                streamWriter.Write(json);
            }

            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            JToken sJson = JToken.Parse(sr.ReadToEnd());
            string result = "0";
            result = sJson["result"]?.ToString();
            return result;
        }
        
        public static void ParseMask()
        {
            Dictionary<int, int> results = new Dictionary<int, int>();
            for (int i = 5169; i < 8721; i++)
            {
                StringBuilder sb = new StringBuilder();
                var hex = i.ToString("X");
                string number = new string('0', 5-hex.Length) + hex;
                int value = Convert.ToInt32(GetMask(number), 16);
                results.Add(i, value);
                if (value >= 1000)
                {
                    Console.WriteLine($"{i} : {value}");
                    CheckOpenSea(i.ToString());
                }

            }
        }


        private static void CheckOpenSea(string num) 
        {
            string URL = $"https://opensea.io/assets/matic/0xfd257ddf743da7395470df9a0517a2dfbf66d156/{num}";
            request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "GET";
            request.Host = "opensea.io";
            request.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 100.0) Gecko / 20100101 Firefox / 100.0";
            request.Accept = "text / html,application / xhtml + xml,application / xml; q = 0.9,image / avif,image / webp,*/*;q=0.8";

            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            HtmlDocument doc = new HtmlDocument();
            doc.Load(sr);
            sr.Close();

            HtmlNodeCollection buttons = doc.DocumentNode.SelectNodes("//button[contains(@class, 'Blockreact__Block-sc-1xf18x6-0 Buttonreact__StyledButton-sc-glfma3-0 dpXlkZ fzwDgL')]");
            HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//div[contains(@class, 'Overflowreact__OverflowContainer-sc-7qr9y8-0 jPSCbX Price--amount')]");
            if (buttons is null)
                Console.WriteLine("Not sale");
            else
                Console.WriteLine(collection[0].InnerText);
        }
    }
}
