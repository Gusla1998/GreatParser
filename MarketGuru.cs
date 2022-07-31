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
    public class MarketGuru
    {
        public int counter = 0;
        
        private string BASE_URL = "https://my.marketguru.io/api/wb-hub/v1/categories/stats?start=2022-06-26T21:00:00.000Z&end=2022-07-25T21:00:00.000Z";
        private string TREE_URL = "https://marketguru.io:3000/api/v2/competitors/tree/";
        HttpWebRequest request;
        List<JToken> list;

        public MarketGuru()
        {
            list = new List<JToken>();
            request = (HttpWebRequest)WebRequest.Create(BASE_URL);
            request.Method = "GET";
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Host", "my.marketguru.io");
            string token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6ImM3MWYyYTA0LTIwMTEtNGQyOC05NWY1LWIyNmQ1NjUzMjBhNiIsInBlcm1pc3Npb25zIjp7ImlzU2VhcmNoRW5hYmxlZE9uS2V5d29yZEFuYWx5c2lzIjp0cnVlLCJtYXhPZlJlZ2lvbnNPbkNoYXJ0T25HZW9ncmFwaHkiOjEsIm1heE9mV29yZHNPbk1hZ25ldG8iOjMwLCJkYXRhVXBkYXRlRnJlcXVlbmN5IjoxODAsIm1heE9mSXRlbXNPblBvc2l0aW9uQW5hbHl0aWNzIjoyLCJtYXhPZk5vbWVuY2xhdHVyZXNPblNlYXJjaFRhZ3NCeU5vbWVuY2xhdHVyZXMiOjEsIm1heFJlcXVlc3RzUGVyRGF5T25TY3JlZW5lciI6MiwiaXNTZXR0aW5nRm9ySXRlbUVuYWJsZWRPblN1cHBseUNvbnRyb2wiOnRydWUsIm1heE9mSXRlbXNPbkdlb2dyYXBoeSI6MSwibWF4T2ZJdGVtc09uU3VwcGxpZXJBbmFseXNpcyI6NSwibWF4T2ZJdGVtc09uU3VwcGx5Q29udHJvbCI6MiwibWF4T2ZDYXRlZ29yaWVzT25FeHRlcm5hbEl0ZW1EZXRhaWxzIjoyLCJtYXhPZkl0ZW1zT25LZXl3b3JkQW5hbHlzaXMiOjEwLCJtYXhPZkJyYW5kc09uQnJhbmRBbmFseXNpcyI6MjUsIm1heE9mSXRlbXNPbkJyYW5kQW5hbHlzaXMiOjUsIm1heE9mS2V5d29yZHNPblNlYXJjaE9uTWFnbmV0byI6MSwibWF4T2ZDYXRlZ29yaWVzT25LZXl3b3JkQW5hbHlzaXMiOjUsIm1heE9mSXRlbXNPbldiU2VhcmNoVGFnc0FuYWx5c2lzIjoxMCwibWF4T2ZTZWFyY2hUYWdzT25NYWduZXRvIjozMCwibWF4T2ZLZXl3b3Jkc09uRXh0ZXJuYWxJdGVtRGV0YWlscyI6NSwibWF4Q2F0ZWdvcnlMZXZlbE9uQ2F0ZWdvcnlBbmFseXNpcyI6MywibWF4T2ZLZXl3b3Jkc09uU2VhcmNoVGFnc0J5Tm9tZW5jbGF0dXJlcyI6MTAsIm1heFdiQVBJS2V5cyI6MSwiZnJlZVdiQVBJS2V5cyI6MSwibWF4T2ZJdGVtc0luVHJhY2tpbmdMaXN0IjoxMCwibWF4T2ZTZXNzaW9ucyI6MiwibWF4T2ZJdGVtc09uRmluYW5jaWFsQW5hbHl0aWNzIjoyLCJtYXhPZkJyYW5kc09uU3VwcGxpZXJBbmFseXNpcyI6MiwibWF4T2ZJdGVtc09uQ2F0ZWdvcnlBbmFseXNpcyI6MTAsIm1heE9mSXRlbXNPbkNvbW1vbkFuYWx5dGljcyI6MiwibWF4T2ZLZXl3b3Jkc09uS2V5d29yZEFuYWx5c2lzIjoxMCwibWF4UmVxdWVzdHNQZXJEYXlPbkV4dGVybmFsSXRlbURldGFpbHMiOjEwLCJtYXhPZlN1cHBsaWVyc09uU3VwcGxpZXJBbmFseXNpcyI6MjV9LCJzZXNzaW9uSWQiOiJjN2Q1ZTMwMi0zNjI3LTRmM2UtOGM0ZS1iOGMxNzUzZTgzNTQiLCJpYXQiOjE2NTg5MTgxODQsImV4cCI6MTY2MTUxMDE4NCwic3ViIjoic2VydmljZSJ9.iG8wueKMp8ZawbOBdAglR33YMtPWugtN1NBRyAWB5sQ";
            request.Headers.Add("Authorization", token);
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
                request = (HttpWebRequest)WebRequest.Create(BASE_URL + "&parentId=" + category["id"]);
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
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Host", "my.marketguru.io");
            string token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6ImM3MWYyYTA0LTIwMTEtNGQyOC05NWY1LWIyNmQ1NjUzMjBhNiIsInBlcm1pc3Npb25zIjp7ImlzU2VhcmNoRW5hYmxlZE9uS2V5d29yZEFuYWx5c2lzIjp0cnVlLCJtYXhPZlJlZ2lvbnNPbkNoYXJ0T25HZW9ncmFwaHkiOjEsIm1heE9mV29yZHNPbk1hZ25ldG8iOjMwLCJkYXRhVXBkYXRlRnJlcXVlbmN5IjoxODAsIm1heE9mSXRlbXNPblBvc2l0aW9uQW5hbHl0aWNzIjoyLCJtYXhPZk5vbWVuY2xhdHVyZXNPblNlYXJjaFRhZ3NCeU5vbWVuY2xhdHVyZXMiOjEsIm1heFJlcXVlc3RzUGVyRGF5T25TY3JlZW5lciI6MiwiaXNTZXR0aW5nRm9ySXRlbUVuYWJsZWRPblN1cHBseUNvbnRyb2wiOnRydWUsIm1heE9mSXRlbXNPbkdlb2dyYXBoeSI6MSwibWF4T2ZJdGVtc09uU3VwcGxpZXJBbmFseXNpcyI6NSwibWF4T2ZJdGVtc09uU3VwcGx5Q29udHJvbCI6MiwibWF4T2ZDYXRlZ29yaWVzT25FeHRlcm5hbEl0ZW1EZXRhaWxzIjoyLCJtYXhPZkl0ZW1zT25LZXl3b3JkQW5hbHlzaXMiOjEwLCJtYXhPZkJyYW5kc09uQnJhbmRBbmFseXNpcyI6MjUsIm1heE9mSXRlbXNPbkJyYW5kQW5hbHlzaXMiOjUsIm1heE9mS2V5d29yZHNPblNlYXJjaE9uTWFnbmV0byI6MSwibWF4T2ZDYXRlZ29yaWVzT25LZXl3b3JkQW5hbHlzaXMiOjUsIm1heE9mSXRlbXNPbldiU2VhcmNoVGFnc0FuYWx5c2lzIjoxMCwibWF4T2ZTZWFyY2hUYWdzT25NYWduZXRvIjozMCwibWF4T2ZLZXl3b3Jkc09uRXh0ZXJuYWxJdGVtRGV0YWlscyI6NSwibWF4Q2F0ZWdvcnlMZXZlbE9uQ2F0ZWdvcnlBbmFseXNpcyI6MywibWF4T2ZLZXl3b3Jkc09uU2VhcmNoVGFnc0J5Tm9tZW5jbGF0dXJlcyI6MTAsIm1heFdiQVBJS2V5cyI6MSwiZnJlZVdiQVBJS2V5cyI6MSwibWF4T2ZJdGVtc0luVHJhY2tpbmdMaXN0IjoxMCwibWF4T2ZTZXNzaW9ucyI6MiwibWF4T2ZJdGVtc09uRmluYW5jaWFsQW5hbHl0aWNzIjoyLCJtYXhPZkJyYW5kc09uU3VwcGxpZXJBbmFseXNpcyI6MiwibWF4T2ZJdGVtc09uQ2F0ZWdvcnlBbmFseXNpcyI6MTAsIm1heE9mSXRlbXNPbkNvbW1vbkFuYWx5dGljcyI6MiwibWF4T2ZLZXl3b3Jkc09uS2V5d29yZEFuYWx5c2lzIjoxMCwibWF4UmVxdWVzdHNQZXJEYXlPbkV4dGVybmFsSXRlbURldGFpbHMiOjEwLCJtYXhPZlN1cHBsaWVyc09uU3VwcGxpZXJBbmFseXNpcyI6MjV9LCJzZXNzaW9uSWQiOiJjN2Q1ZTMwMi0zNjI3LTRmM2UtOGM0ZS1iOGMxNzUzZTgzNTQiLCJpYXQiOjE2NTg5MTgxODQsImV4cCI6MTY2MTUxMDE4NCwic3ViIjoic2VydmljZSJ9.iG8wueKMp8ZawbOBdAglR33YMtPWugtN1NBRyAWB5sQ";
            request.Headers.Add("Authorization", token);
        }
        private JArray Request_For_Cost(JToken category)
        {
            request = (HttpWebRequest)WebRequest.Create(BASE_URL + "&parentId=" + category["id"]);
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
            catch (Exception e)
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
            foreach (var record in records)
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
}
