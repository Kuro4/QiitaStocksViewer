using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public ReactiveCollection<PostInformation> _PostList { get; set; } = new ReactiveCollection<PostInformation>();
        public ReactiveCommand C_GetPostList { get; } = new ReactiveCommand();

        public Model()
        {
            C_GetPostList.Subscribe(testAdd);
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

        private async void testAdd()
        {
            var client = new HttpClient();
            Uri uri = new Uri("https://qiita.com/api/v2/users/kuro4/items");
            var result = await client.GetStringAsync(uri);
            JArray data = (JArray)JsonConvert.DeserializeObject(result);

            foreach(JObject item in data)
            {
                _PostList.Add(new PostInformation()
                {
                    _Title = item[JsonKeysEx.getKeyName(JsonKeys.e_title)].ToString(),
                    _PostTime = ISO8601ToDateTime(item[JsonKeysEx.getKeyName(JsonKeys.e_created_at)].ToString()),
                    _UpDatedTime = ISO8601ToDateTime(item[JsonKeysEx.getKeyName(JsonKeys.e_updated_at)].ToString()),
                    _LimitedShared = bool.Parse(item[JsonKeysEx.getKeyName(JsonKeys.e_private)].ToString()),
            });
            }

            //foreach (JObject item in data)
            //{
            //    _PostList.Add(new PostInformation());
            //    foreach (var value in item)
            //    {
            //        switch (value.Key)
            //        {
            //            case "title":
            //                _PostList.Last()._Title = value.Value.ToString();
            //                break;
            //            case "created_at":
            //                _PostList.Last()._PostTime = DateTime.Parse(value.Value.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind);
            //                break;
            //            case "updated_at":
            //                _PostList.Last()._UpDatedTime = DateTime.Parse(value.Value.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind);
            //                break;
            //            case "private":
            //                _PostList.Last()._LimitedShared = bool.Parse(value.Value.ToString());
            //                break;
            //            case "url":
            //                _PostList.Last()._URL = new Uri(value.Value.ToString());
            //                break;
            //            case "id":
            //                //_PostList.Last(). = value.Value.ToString();
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}
            MessageBox.Show("終了");
        }

        private DateTime ISO8601ToDateTime(string dateTime)
        {
            return DateTime.Parse(dateTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        private void getPostsList()
        {
            if (!string.IsNullOrWhiteSpace(_UserID.Value))
            {
                
            }
        }
    }

    class PostInformation
    {
        public string _Title { get;  set; }
        public DateTime _PostTime { get;  set; } = new DateTime();
        public DateTime _UpDatedTime { get;  set; } = new DateTime();
        public bool _LimitedShared { get;  set; }
        public int _StockCount { get;  set; }
        public Uri _URL { get; set; }
        public List<string> _StockedPerson { get;  set; } = new List<string>();

        public PostInformation()
        {

        }
    }
}
