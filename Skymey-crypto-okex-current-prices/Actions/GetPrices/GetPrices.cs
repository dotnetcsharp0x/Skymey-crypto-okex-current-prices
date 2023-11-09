using MongoDB.Bson;
using MongoDB.Driver;
using Nancy.Json;
using RestSharp;
using Skymey_crypto_okex_current_prices.Data;
using Skymey_main_lib.Models.Prices;
using Skymey_main_lib.Models.Prices.Binance;
using Skymey_main_lib.Models.Prices.Okex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skymey_crypto_okex_current_prices.Actions.GetPrices
{
    public class GetPrices
    {
        private RestClient _client;
        private RestRequest _request;
        private MongoClient _mongoClient;
        private ApplicationContext _db;
        public GetPrices()
        {
            _client = new RestClient("https://www.okx.com/api/v5/public/mark-price?instType=SWAP");
            _request = new RestRequest("https://www.okx.com/api/v5/public/mark-price?instType=SWAP", Method.Get);
            _mongoClient = new MongoClient("mongodb://127.0.0.1:27017");
            _db = ApplicationContext.Create(_mongoClient.GetDatabase("skymey"));
        }
        public void GetCurrentPricesFromOkex()
        {
            _request.AddHeader("Content-Type", "application/json");
            var r = _client.Execute(_request).Content;
            OkexCurrentPricesView ticker = new JavaScriptSerializer().Deserialize<OkexCurrentPricesView>(r);
            foreach (var tickers in ticker.data)
            {
                Console.WriteLine(tickers.instId);
                var ticker_find = (from i in _db.OkexCurrentPricesView where i.Ticker == tickers.instId select i).FirstOrDefault();
                var ticker_findc = (from i in _db.CurrentPrices where i.Ticker == tickers.instId select i).FirstOrDefault();
                if (ticker_find == null)
                {
                    OkexCurrentPrices ocp = new OkexCurrentPrices();
                    ocp._id = ObjectId.GenerateNewId();
                    ocp.Ticker = tickers.instId;
                    ocp.Price = Convert.ToDouble(tickers.markPx);
                    ocp.Update = DateTime.UtcNow;
                    _db.OkexCurrentPricesView.Add(ocp);
                }
                else
                {
                    ticker_find.Price = Convert.ToDouble(tickers.markPx);
                    ticker_find.Update = DateTime.UtcNow;
                    _db.OkexCurrentPricesView.Update(ticker_find);
                }
                if (ticker_findc == null)
                {
                    CurrentPrices ocpc = new CurrentPrices();
                    ocpc._id = ObjectId.GenerateNewId();
                    ocpc.Ticker = tickers.instId;
                    ocpc.Price = Convert.ToDouble(tickers.markPx);
                    ocpc.Update = DateTime.UtcNow;
                    _db.CurrentPrices.Add(ocpc);
                }
                else
                {
                    ticker_findc.Price = (ticker_findc.Price+Convert.ToDouble(tickers.markPx))/2;
                    ticker_findc.Update = DateTime.UtcNow;
                    _db.CurrentPrices.Update(ticker_findc);
                }

            }
            _db.SaveChanges();
        }
    }
}
