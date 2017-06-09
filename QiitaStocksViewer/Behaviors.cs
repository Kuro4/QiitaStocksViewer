using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interactivity;

namespace QiitaStocksViewer
{
    class HyperlinkBehavior : Behavior<Hyperlink>
    {
        #region uriプロパティ
        public Uri uri
        {
            get
            {
                return (Uri)GetValue(UriProperty);
            }
            set
            {
                SetValue(UriProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UriProperty =
            DependencyProperty.Register("uri", typeof(Uri), typeof(HyperlinkBehavior), new UIPropertyMetadata(null));
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OpenLinkTo; ; ;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= OpenLinkTo;
        }

        private void OpenLinkTo(object sender, RoutedEventArgs e)
        {
            Process.Start(uri.AbsoluteUri);
        }
    }
}
