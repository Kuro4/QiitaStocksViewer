namespace QiitaStocksViewer
{
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
    public static class JsonKeysEx
    {
        public static string getKeyName(this JsonKeys key)
        {
            switch (key)
            {
                case JsonKeys.e_renderd_body: return "renderd_body";
                case JsonKeys.e_body: return "body";
                case JsonKeys.e_coediting: return "coediting";
                case JsonKeys.e_created_at: return "created_at";
                case JsonKeys.e_group: return "group";
                case JsonKeys.e_id: return "id";
                case JsonKeys.e_private: return "private";
                case JsonKeys.e_tags: return "tags";
                case JsonKeys.e_title: return "title";
                case JsonKeys.e_updated_at: return "updated_at";
                case JsonKeys.e_url: return "url";
                case JsonKeys.e_user: return "user";
                default: return "";
            }
        }
    }

    public enum XPaths
    {
        myPage_ItemCount,
        myPage_Contributions,
        Post_LikeCount,
        Post_CommentCOunt,
    }
    public static class XPathEx
    {
        public static string getXPath(this XPaths path)
        {
            switch (path)
            {
                case XPaths.myPage_ItemCount: return @"//*[@id=""main""]/div/div/div[2]/div[1]/div[2]/a[1]/span[1]";
                case XPaths.myPage_Contributions: return @"//*[@id=""main""]/ div/div/div[2]/div[1]/div[2]/a[2]/span[1]";
                case XPaths.Post_LikeCount: return @"//*[@id=""main""]/article/div[1]/div[2]/div/div[2]/div/ul/li[1]/div[1]/span[2]";
                case XPaths.Post_CommentCOunt: return @"//*[@id=""main""]/article/div[1]/div[2]/div/div[2]/div/ul/li[2]/div[1]/text()";
                default: return "";
            }
        }
    }
}