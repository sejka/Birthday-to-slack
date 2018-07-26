using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
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
        private static IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

        private static readonly string calendarFileUrl = config["CalendarFileUrl"];

        private static readonly string slackWebhookUrl = config["SlackWebhookUrl"];

        private static readonly HttpClient _httpClient = new HttpClient();

        [FunctionName("BirthdayToSlackFunction")]
        public static async Task Run([TimerTrigger("0 0 9 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var icalStream = await _httpClient.GetStreamAsync(calendarFileUrl);
            Calendar calendar = Calendar.Load(icalStream);

            var todaysBirthday = calendar.Children
                .Cast<CalendarEvent>()
                .Where(x => x.DtStart.Date == DateTime.UtcNow.Date);

            foreach (var birthday in todaysBirthday)
            {
                PostBirthdayToSlack(birthday);
                log.Info($"Posted {birthday.Summary}");
            }
        }

        private static async void PostBirthdayToSlack(CalendarEvent birthday)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "AzureFunctions");

                //"Joe, Doe - Birthday"
                var surnameAndName = birthday.Summary.Split(' ').Take(2);

                //["Joe,", "Doe"]

                var firstName = surnameAndName.ElementAt(1);
                var lastName = surnameAndName.First().Replace(",", "");

                StringContent requestContent = new StringContent($"{{\"text\": \"It's {firstName} {lastName} birthday!\", \"username\": \"Birthday Bot\", \"icon_emoji\": \":birthday:\"}}");

                HttpResponseMessage response = await client.PostAsync(slackWebhookUrl, requestContent);
            }
        }
    }
}