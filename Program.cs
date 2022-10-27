using MinimalisticWeatherAPI;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string Datapath = "Data/weatherData.json";

List<Weather>? weatherList = new List<Weather>();

await ReadWeatherList();

app.Run(async (context) =>
{
    HttpResponse response = context.Response;
    HttpRequest request = context.Request;
    var path = request.Path;

    string expresion = @"^/weather/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
    if (path == "/weather" && request.Method == "GET")
    {
        await GetAllWeather(response);
    }
    else if (Regex.IsMatch(path, expresion) && request.Method == "GET")
    {
        string? id = path.Value?.Split('/')[2];
        await GetWeather(id, response);
    }
    else if (Regex.IsMatch(path, expresion) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split('/')[2];
        await DeleteWeather(id, response);
    }
    else if(path ==  "/weather" && request.Method == "POST")
    {
        await CreateWeather(request, response);
    }
    else if(path == "/weather" && request.Method == "PUT")
    {
        await UpdateWeather(response,request);
    }
    else if (File.Exists("WeatherImages" + path))
    {
        await response.SendFileAsync("WeatherImages" + path);
    }
    else if (path == "/css")
    {
        await response.SendFileAsync("html/StyleSheet.css");
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/client.html");
    }
}); 

app.Run();

async Task GetAllWeather(HttpResponse response)
{
    if (weatherList?.Count > 0)
        await response.WriteAsJsonAsync(weatherList.ToArray());
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync("There is no weather for you");
    }
}

async Task GetWeather(string? id, HttpResponse response)
{
    Weather? weather = weatherList?.FirstOrDefault((x) => x.Id == id);
    if (weather != null)
        await response.WriteAsJsonAsync(weather);
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync("Cannot find that weather");
    }
}

async Task CreateWeather(HttpRequest request, HttpResponse response)
{
    Weather? weather = await request.ReadFromJsonAsync<Weather>();
    if(weather != null)
    {
        weather.Id = Guid.NewGuid().ToString();
        weather.ImageId = GetWeatherImage(weather);
        weatherList?.Add(weather);
        await UpdateWeatherList();
        await response.WriteAsJsonAsync(weather);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync("Unable to set your weather");
    }
}

async Task DeleteWeather(string? id, HttpResponse response)
{
    Weather? weather = weatherList?.FirstOrDefault(x => x.Id == id);
    if(weather != null)
    {
        weatherList?.Remove(weather);
        await UpdateWeatherList();
        await response.WriteAsJsonAsync(weather);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync("Unable to delete this weather");
    }
}

async Task UpdateWeather(HttpResponse response, HttpRequest request)
{
    Weather? weather = await request.ReadFromJsonAsync<Weather>();
    if (weather != null)
    {
        Weather? w = weatherList?.FirstOrDefault(x => x.Id == weather.Id);
        if(w != null)
        {

            w.Temp = weather.Temp;
            w.Humidity = weather.Humidity;
            await response.WriteAsJsonAsync(w);
            await UpdateWeatherList();
        }
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync("Unable to find thit weather");
    }
}

async Task ReadWeatherList()
{
    using (FileStream fs = new FileStream(Datapath, FileMode.OpenOrCreate))
    {
        if(fs.Length > 0)
        {
            weatherList = await JsonSerializer.DeserializeAsync<List<Weather>>(fs);
        }
        if (weatherList != null)
        {
            foreach (var w in weatherList)
            {
                if (w.Id == "")
                {
                    w.Id = Guid.NewGuid().ToString();
                }
                if (w.ImageId == "")
                {
                    w.ImageId = GetWeatherImage(w);
                }
            }
        }
    }
}
async Task UpdateWeatherList()
{
    using (FileStream fs = new FileStream(Datapath, FileMode.Truncate))
    {
        if(weatherList != null)
        {
            await JsonSerializer.SerializeAsync<List<Weather>>(fs, weatherList);
        }
    }
}

string GetWeatherImage(Weather w)
{
    if (w.Humidity < 25) return "https://localhost:7232/sun.png"; //"https://solarsystem.nasa.gov/system/basic_html_elements/11561_Sun.png"
    else if (w.Humidity > 25 && w.Humidity < 75) return "https://localhost:7232/cloudy.png";
    else return "https://localhost:7232/cloud.png";
}


