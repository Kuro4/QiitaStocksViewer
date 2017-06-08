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
    public class ViewModel
    {
        public Model _Model { get; set; } = new Model();

#if DEBUG
        public ViewModel()
        {
            _Model._PostList.Add(new PostInformation()
            {
                _Title = "test1",
                _PostTime = DateTime.Now,
                _UpDatedTime = DateTime.Now,
                _LimitedShared = true,
                _URL = new Uri("http://qiita.com/kuro4"),
            });
        }
#endif
    }
}
