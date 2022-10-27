namespace tvpgo.Json
{
    
public class Tokenizer
{
    public string url { get; set; }
    public string status { get; set; }
    public int videoId { get; set; }
    public string platform { get; set; }
    public string userIp { get; set; }
    public bool adaptive { get; set; }
    public bool live { get; set; }
    public string title { get; set; }
    public int duration { get; set; }
    public bool countryIsDefault { get; set; }
    public string mimeType { get; set; }
    public object ads_enabled { get; set; }
    public int payment_type { get; set; }
    public string distribution_model { get; set; }
    public object access_provider { get; set; }
    public bool useFormats { get; set; }
    public bool timeShift { get; set; }
    public int assetId { get; set; }
    public object hostname { get; set; }
    public string type { get; set; }
    public string date { get; set; }
    public long ip { get; set; }
    public object token { get; set; }
    public Format[] formats { get; set; }
    public object status_failed_server_id { get; set; }
    public bool isGeoBlocked { get; set; }
}

public class Format
{
    public string mimeType { get; set; }
    public int totalBitrate { get; set; }
    public int videoBitrate { get; set; }
    public int audioBitrate { get; set; }
    public bool adaptive { get; set; }
    public string url { get; set; }
    public bool downloadable { get; set; }
    public object videoPackingPart { get; set; }
}

}