namespace SignalR.Services
{
    public class WeatherGet
    {
        readonly HttpClient _httpClient;
        public WeatherGet(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> GetWeather(string city)
        {
            var response = await _httpClient.GetAsync($"https://api.open-meteo.com/v1/forecast?latitude=35.6895&longitude=139.6917&current_weather=true");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            else
            {
                throw new Exception("Failed to fetch weather data");
            }
        }
    }
}
