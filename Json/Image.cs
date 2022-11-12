using SmartFormat;
namespace tvpgo.Json
{
    public class Image
    {
        public string type { get; set; }
        public string title { get; set; }
        public object point_of_origin { get; set; }
        public string url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public object description { get; set; }
        public string ImageUrl => Smart.Format(url, this);
    }
}

