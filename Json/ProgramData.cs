namespace tvpgo.Json
{
    public class ProgramData
    {
        public object error { get; set; }
        public ProgramDetails data { get; set; }
    }

    public class ProgramDetails
    {
        public string type { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public ProgramImage image { get; set; }
        public string station_code { get; set; }
        public object record_id { get; set; }
        public string stream_url { get; set; }
        public bool violence_check { get; set; }
        public bool sex_check { get; set; }
        public bool profanity_check { get; set; }
        public bool drugs_check { get; set; }
        public object plrating { get; set; }
    }

    public class ProgramImage
    {
        public string type { get; set; }
        public string title { get; set; }
        public object point_of_origin { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object description { get; set; }
    }

}
