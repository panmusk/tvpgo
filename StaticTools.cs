namespace tvpgo
{
    public static class StaticTools
    {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        public static long DateTimeToUnixTimeStamp(DateTime date)
        {
            return (long)date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
        public static string NormalizeFileName(string FileName)
        {
            return string.Join("-", FileName.Split(Path.GetInvalidFileNameChars()));
        }

    }
}