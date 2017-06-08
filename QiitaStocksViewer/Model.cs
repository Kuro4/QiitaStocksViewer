using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
    public class Model
    {
        public ReactiveProperty<string> _UserID { get; set; } = new ReactiveProperty<string>();
        public string _AccessToken { get; set; }
        public ReactiveCollection<PostInformation> _PostList { get; } = new ReactiveCollection<PostInformation>();
        public ReactiveCommand C_GetPostList { get; } = new ReactiveCommand();
        public ReactiveCommand C_OutputToCSV { get; } = new ReactiveCommand();
        public Model()
        {
            C_GetPostList = _UserID
                .Select(x => !string.IsNullOrWhiteSpace(x))
                .ToReactiveCommand();
            C_GetPostList.Subscribe(LoadFromQiita);
            C_OutputToCSV.Subscribe(OutputToCSV);
        }
        private async void LoadFromQiita()
        {
            if (!string.IsNullOrWhiteSpace(_UserID.Value))
            {
                var client = new HttpClient();
                Uri uri = new Uri("https://qiita.com/api/v2/users/" + _UserID.Value + "/items");
                string result;
                try
                {
                    if (string.IsNullOrWhiteSpace(_AccessToken))
                    {
                        result = await client.GetStringAsync(uri);
                    }
                    else
                    {
                        var request = new HttpRequestMessage();
                        request.Method = HttpMethod.Get;
                        request.RequestUri = uri;
                        request.Headers.Host = "qiita.com";
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _AccessToken);
                        var respons = await client.SendAsync(request);
                        result = await respons.Content.ReadAsStringAsync();
                    }
                    JArray data = (JArray)JsonConvert.DeserializeObject(result);
                    _PostList.Clear();
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
                catch (HttpRequestException e)
                {
                    MessageBox.Show("エラーが発生しました" + Environment.NewLine
                        + "ユーザーID,AccessTokenが間違っているか、リクエスト制限を超えた可能性があります" + Environment.NewLine
                        + "（リクエスト回数はIPアドレス毎に \n　認証無：60回/h \n　認証有：1000回/h \n　までです）", e.GetType().ToString());
                }
            }
        }
        private DateTime ISO8601ToDateTime(string dateTime)
        {
            return DateTime.Parse(dateTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }
        private void OutputToCSV()
        {
            if (_PostList.Count != 0)
            {
                var dialog = new SaveFileDialog();
                dialog.Title = "保存";
                dialog.Filter = "csvファイル(*.csv)|*.csv";
                if ((bool)dialog.ShowDialog())
                {
                    string str = "タイトル,ストック者数,ストック者ID,URL,投稿日,最終更新日,限定共有" + Environment.NewLine;
                    foreach (var postInfo in _PostList)
                    {
                        str += postInfo._Title + ",";
                        str += postInfo._StockInfo._StockCount.Value + ",\"";
                        foreach (var item in postInfo._StockInfo._StockedPerson)
                        {
                            str += item + ",";
                        }
                        str += "\"," + postInfo._URL + ",";
                        str += postInfo._PostTime + ",";
                        str += postInfo._UpDatedTime + ",";
                        str += postInfo._LimitedShared + "," + Environment.NewLine;
                    }
                    try
                    {
                        var stream = dialog.OpenFile();
                        StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
                        streamWriter.Write(str);
                        streamWriter.Close();
                        stream.Close();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("エラーが発生しました" + Environment.NewLine
                            + "ファイルの保存に失敗しました", e.GetType().ToString());
                    }
                }
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
    public class PostInformation
    {
        public string _Title { get; set; } = "";
        public DateTime _PostTime { get; set; } = new DateTime();
        public DateTime _UpDatedTime { get; set; } = new DateTime();
        public bool _LimitedShared { get; set; } = false;
        public Uri _URL { get; set; } = new Uri("http://qiita.com/");
        public StockInformation _StockInfo { get; set; } = new StockInformation();
        public ReactiveProperty<bool> _isPopupOpen { get; private set; } = new ReactiveProperty<bool>() { Value = false };
        public ReactiveCommand C_PopupChange { get; } = new ReactiveCommand();
        public PostInformation()
        {
            C_PopupChange.Subscribe(_ => _isPopupOpen.Value = !_isPopupOpen.Value);
        }
        public class StockInformation
        {
            public string _PostID { get; } = "";
            public ReactiveProperty<int> _StockCount { get; private set; } = new ReactiveProperty<int>(0);
            public ReactiveCollection<string> _StockedPerson { get; private set; } = new ReactiveCollection<string>();
            public StockInformation(string PostID)
            {
                _PostID = PostID;
                getStockInfo();
            }
            public StockInformation()
            {
            }

            private async void getStockInfo()
            {
                Uri uri = new Uri("https://qiita.com/api/v2/items/" + _PostID + "/stockers");
                var result = await new HttpClient().GetStringAsync(uri);
                JArray data = (JArray)JsonConvert.DeserializeObject(result);
                foreach (JObject item in data)
                {
                    _StockCount.Value++;
                    _StockedPerson.Add(item[JsonKeysEx.getKeyName(JsonKeys.e_id)].ToString());
                }
            }
        }
    }
}
