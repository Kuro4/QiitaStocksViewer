using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Windows;


namespace QiitaStocksViewer
{
    public class Model
    {
        public ReactiveProperty<string> _UserID { get;private set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> _AccessToken { get;private set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> _ItemCount { get;private set; } = new ReactiveProperty<string>("0");
        public ReactiveProperty<string> _Contributions { get; private set; } = new ReactiveProperty<string>("0");
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
                    var html = await client.GetStringAsync("http://qiita.com/" + _UserID.Value);
                    _ItemCount.Value = ScrapeMyPage(html,XPaths.myPage_ItemCount.getXPath());
                    _Contributions.Value = ScrapeMyPage(html, XPaths.myPage_Contributions.getXPath());
                    if (string.IsNullOrWhiteSpace(_AccessToken.Value))
                    {
                        result = await client.GetStringAsync(uri);
                    }
                    else
                    {
                        var request = new HttpRequestMessage();
                        request.Method = HttpMethod.Get;
                        request.RequestUri = uri;
                        request.Headers.Host = "qiita.com";
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _AccessToken.Value);
                        var respons = await client.SendAsync(request);
                        result = await respons.Content.ReadAsStringAsync();
                    }
                    var data = (JArray)JsonConvert.DeserializeObject(result);
                    _PostList.Clear();
                    foreach (JObject item in data)
                    {
                        _PostList.Add(new PostInformation(client, "http://qiita.com/" + _UserID.Value + "/items/" + item[JsonKeys.e_id.getKeyName()].ToString())
                        {
                            _Title = item[JsonKeys.e_title.getKeyName()].ToString(),
                            _PostTime = ISO8601ToDateTime(item[JsonKeys.e_created_at.getKeyName()].ToString()),
                            _UpDatedTime = ISO8601ToDateTime(item[JsonKeys.e_updated_at.getKeyName()].ToString()),
                            _LimitedShared = bool.Parse(item[JsonKeys.e_private.getKeyName()].ToString()),
                            _URL = new Uri(item[JsonKeys.e_url.getKeyName()].ToString()),
                            _StockInfo = new PostInformation.StockInformation(item[JsonKeys.e_id.getKeyName()].ToString(),client),
                        });
                    }
                }
                catch (Exception e) when (e is HttpRequestException || e is InvalidCastException)
                {
                    MessageBox.Show("エラーが発生しました" + Environment.NewLine
                        + "ユーザーID,AccessTokenが間違っているか、リクエスト制限を超えた可能性があります" + Environment.NewLine
                        + "（リクエスト回数はIPアドレス毎に \n　認証無：60回/h \n　認証有：1000回/h \n　までです）", e.GetType().ToString());
                }
            }
        }
        private string ScrapeMyPage(string html, string XPath)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.SelectSingleNode(XPath).InnerText;
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
                    string str = "タイトル,いいね数,ストック者数,コメント数,ストック者ID,URL,投稿日,最終更新日,限定共有" + Environment.NewLine;
                    foreach (var postInfo in _PostList)
                    {
                        str += postInfo._Title + ",";
                        str += postInfo._LikeCount.Value + ",";
                        str += postInfo._StockInfo._StockCount.Value + ",";
                        str += postInfo._CommentCount.Value + ",\"";
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

    public class PostInformation
    {
        public string _Title { get; set; } = "";
        public ReactiveProperty<string> _LikeCount { get; set; } =new ReactiveProperty<string>();
        public ReactiveProperty<string> _CommentCount { get; set; } = new ReactiveProperty<string>();
        public DateTime _PostTime { get; set; } = new DateTime();
        public DateTime _UpDatedTime { get; set; } = new DateTime();
        public bool _LimitedShared { get; set; } = false;
        public Uri _URL { get; set; } = new Uri("http://qiita.com/");
        public StockInformation _StockInfo { get; set; } = new StockInformation();
        public ReactiveProperty<bool> _isPopupOpen { get; private set; } = new ReactiveProperty<bool>() { Value = false };
        public ReactiveCommand C_PopupChange { get; } = new ReactiveCommand();
        public PostInformation(HttpClient client, string url)
        {
            ScrapePost(client, url);
            C_PopupChange.Subscribe(_ => _isPopupOpen.Value = !_isPopupOpen.Value);
        }
        public PostInformation()
        {
        }
        private async void ScrapePost(HttpClient client, string url)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(await client.GetStringAsync(url));
            _LikeCount.Value = doc.DocumentNode.SelectSingleNode(XPaths.Post_LikeCount.getXPath()).InnerText;
            _CommentCount.Value = doc.DocumentNode.SelectSingleNode(XPaths.Post_CommentCOunt.getXPath()).InnerText;
        }

        public class StockInformation
        {
            public string _PostID { get; } = "";
            public ReactiveProperty<int> _StockCount { get; private set; } = new ReactiveProperty<int>(0);
            public ReactiveCollection<string> _StockedPerson { get; private set; } = new ReactiveCollection<string>();
            public StockInformation(string PostID,HttpClient client)
            {
                _PostID = PostID;
                getStockInfo(PostID,client);
            }
            public StockInformation()
            {
            }
            private async void getStockInfo(string postID,HttpClient client)
            {
                Uri uri = new Uri("https://qiita.com/api/v2/items/" + postID + "/stockers");
                var result = await client.GetStringAsync(uri);
                JArray data = (JArray)JsonConvert.DeserializeObject(result);
                _StockCount.Value = data.Count();
                foreach (JObject item in data)
                {
                    _StockedPerson.Add(item[JsonKeys.e_id.getKeyName()].ToString());
                }
            }
        }
    }
}
