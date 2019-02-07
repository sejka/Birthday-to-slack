using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BirthdayToSlack
{
    public static class BirthdayToSlackFunction
    {
        private static readonly IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

        private static readonly string calendarFileUrl = config["CalendarFileUrl"];

        private static readonly string slackWebhookUrl = config["SlackWebhookUrl"];

        private static readonly HttpClient _httpClient = new HttpClient();

        [FunctionName("BirthdayToSlackFunction")]
        public static async Task Run([TimerTrigger("0 0 9 * * *")]TimerInfo myTimer)
        {
            var icalStream = await _httpClient.GetStreamAsync(calendarFileUrl);
            Calendar calendar = Calendar.Load(icalStream);

            var todaysBirthday = calendar.Children
                .Cast<CalendarEvent>()
                .Where(x => x.DtStart.Date == DateTime.UtcNow.Date);

            foreach (var birthday in todaysBirthday)
            {
                PostBirthdayToSlack(birthday);
            }
        }

        private static async void PostBirthdayToSlack(CalendarEvent birthday)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AzureFunctions");

            //"Tommy Lee Jones - Birthday"
            var names = birthday
                .Summary
                .Split('-')[0]
                .Trim();

            //"Tommy Lee Jones"

            StringContent requestContent = new StringContent($"{{\"text\": \"It's {names} birthday!\", \"username\": \"Birthday Bot\", \"icon_emoji\": \":birthday:\"}}");

            HttpResponseMessage response = await _httpClient.PostAsync(slackWebhookUrl, requestContent);
        }
    }
}