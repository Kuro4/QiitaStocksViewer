using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;


namespace QiitaStocksViewer
{
    class Model
    {
        public ReactiveProperty<string> _UserID { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<bool> _isSaveUserID { get; set; } = new ReactiveProperty<bool>();
        public ReactiveCollection<PostInformation> _PostList { get; set; } = new ReactiveCollection<PostInformation>();
        public ReactiveCommand C_GetPostList { get; } = new ReactiveCommand();

        public Model()
        {
            C_GetPostList = _UserID
                .Select(x => !string.IsNullOrWhiteSpace(x))
                .ToReactiveCommand();
            C_GetPostList.Subscribe(LoadFromQiita);
        }

        private async void LoadFromQiita()
        {
            if (!string.IsNullOrWhiteSpace(_UserID.Value))
            {
                var client = new HttpClient();
                Uri uri = new Uri("https://qiita.com/api/v2/users/" + _UserID.Value + "/items");
                var result = await client.GetStringAsync(uri);
                JArray data = (JArray)JsonConvert.DeserializeObject(result);
                foreach (JObject item in data)
                {
                    _PostList.Add(new PostInformation()
                    {
                        _Title = item[JsonKeysEx.getKeyName(JsonKeys.e_title)].ToString(),
                        _PostTime = ISO8601ToDateTime(item[JsonKeysEx.getKeyName(JsonKeys.e_created_at)].ToString()),
                        _UpDatedTime = ISO8601ToDateTime(item[JsonKeysEx.getKeyName(JsonKeys.e_updated_at)].ToString()),
                        _LimitedShared = bool.Parse(item[JsonKeysEx.getKeyName(JsonKeys.e_private)].ToString()),
                        _URL = new Uri(item[JsonKeysEx.getKeyName(JsonKeys.e_url)].ToString()),
                        _StockInfo = new PostInformation.StockInformation(item[JsonKeysEx.getKeyName(JsonKeys.e_id)].ToString()),
                    });
                }
            }
        }

        public enum JsonKeys
        {
            e_renderd_body,
            e_body,
            e_coediting,
            e_created_at,
            e_group,
            e_id,
            e_private,
            e_tags,
            e_title,
            e_updated_at,
            e_url,
            e_user,
        }

        public class JsonKeysEx
        {
            public static string getKeyName(JsonKeys key)
            {
                return key.ToString().Remove(0, 2);
            }
        }

        private DateTime ISO8601ToDateTime(string dateTime)
        {
            return DateTime.Parse(dateTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }
    }

    public class PostInformation
    {
        public string _Title { get;  set; }
        public DateTime _PostTime { get;  set; } = new DateTime();
        public DateTime _UpDatedTime { get;  set; } = new DateTime();
        public bool _LimitedShared { get;  set; }
        public Uri _URL { get; set; }
        public StockInformation _StockInfo { get; set; }

        public class StockInformation
        {
            public string _PostID { get;}
            public ReactiveProperty<int> _StockCount { get; set; } = new ReactiveProperty<int>();
            public ReactiveCollection<string> _StockedPerson { get; set; } = new ReactiveCollection<string>();

            public StockInformation(string PostID)
            {
                _PostID = PostID;
                getStockInfo();
            }

            private async void getStockInfo()
            {
                Uri uri = new Uri("https://qiita.com/api/v2/items/" + _PostID + "/stockers");
                var result =await new HttpClient().GetStringAsync(uri);
                JArray data = (JArray)JsonConvert.DeserializeObject(result);
                foreach (JObject item in data)
                {
                    _StockCount.Value++;
                    _StockedPerson.Add(item["id"].ToString());
                }
            } 
        }

    }
}
