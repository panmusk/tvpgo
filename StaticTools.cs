using Newtonsoft.Json;

namespace tvpgo
{
    public static class StaticTools
    {
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        public static double DateTimeToUnixTimeStamp(DateTime date)
        {
            return date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
        public static string NormalizeFileName(string FileName)
        {
            return string.Join("-", FileName.Split(Path.GetInvalidFileNameChars()));
        }
        public static async Task<T> WebDeserializeAsync<T>(string url)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, url);
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var stations = JsonConvert.DeserializeObject<T>(json);
            return stations;
        }
    }
}