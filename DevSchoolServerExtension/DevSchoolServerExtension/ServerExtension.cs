using DevSchoolServerExtension.Models;
using DocsVision.Platform.StorageServer.Extensibility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DevSchoolServerExtension
{
    /// <summary>
    /// Класс серверного расширения с методом получения суммарной стоимости билетов
    /// </summary>
    /// <param name="airportCode">Код аэропорта</param>
    /// <param name="dateFrom">Дата вылета</param>
    /// <param name="dateTo">Дата прилёта</param>
    public class ServerExtension : StorageServerExtension
    {
        [ExtensionMethod]
        public decimal GetTicketsPrice(string airportCode, DateTime dateFrom, DateTime dateTo)
        {
            decimal priceFrom = GetPrice(airportCode, dateFrom);
            if (priceFrom == -1) { return 0; }
            decimal priceTo = GetPrice("LED", dateTo, airportCode);
            if (priceTo == -1) { return 0; }
            decimal ticketsPrice = priceFrom + priceTo;

            return ticketsPrice;
        }

        private decimal GetPrice(string airportCode, DateTime date, string airportSource="LED") 
        {
            string yearValue = date.Year.ToString();
            string monthValue = date.Month.ToString();
            string url = $@"http://map.aviasales.ru/prices.json?origin_iata={airportSource}&period={yearValue}-{monthValue}-01:month&direct=true&one_way=true&no_visa=false&schengen=false&need_visa=false&locale=ru";
            string result = getContent(url);
            List<Ticket> ticketsData = JsonConvert.DeserializeObject<List<Ticket>>(result);
            decimal minPrice;

            bool isTicketExist = ticketsData
                    .Where(x => x.depart_date == date && x.destination == airportCode).Any();

            if (isTicketExist)
                minPrice = ticketsData
                    .Where(x => x.depart_date == date && x.destination == airportCode)
                    .Select(x => x.value)
                    .Min();
            else
                return -1;

            return minPrice;
        }

        private string getContent(string url) 
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Accept = "application/json";
            request.UserAgent = "Mozilla / 5.0(Linux; Android) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 88.0.4324.109 Safari / 537.36 CrKey / 1.54.248666";
            request.Proxy = WebRequest.DefaultWebProxy;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            response.Close();
            return output.ToString();
        }
    }
}