using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiitaStocksViewer
{
    class ViewModel
    {
        public Model _Model { get; set; } = new Model();
        public ViewModel()
        {
            var s = new ViewModel2();
        }
    }

    public class ViewModel2
    {
        private Model model = new Model();
        public ReactiveProperty<string> _UserID { get; set; } = new ReactiveProperty<string>();
        public string _AccessToken { get; set; }
        public ReactiveCollection<PostInformation> _PostList { get; } = new ReactiveCollection<PostInformation>();
        public ReactiveCommand C_GetPostList { get; } = new ReactiveCommand();
        public ReactiveCommand C_OutputToCSV { get; } = new ReactiveCommand();

        public ViewModel2()
        {
            //ViewModelはModelの影、Modelへの中継ができればよい=参照渡し
            _UserID = model._UserID;
            _AccessToken = model._AccessToken;
            _PostList = model._PostList;


            C_GetPostList = _UserID
                .Select(x => !string.IsNullOrWhiteSpace(x))
                .ToReactiveCommand();
            C_GetPostList.Subscribe(model.LoadFromQiita);
            C_OutputToCSV.Subscribe(model.OutputToCSV);
        }
    }
}
