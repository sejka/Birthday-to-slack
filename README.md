# Birthday-to-slack
Azure function that posts events from calendar to slack channel.

## Configuration
* Fill both `SlackWebhookUrl` and `CalendarFileUrl` in `local.settings.json`
* Optionally you can fork this repo, setup a [continuous deployment](https://docs.microsoft.com/en-us/azure/azure-functions/functions-continuous-deployment) and fill above variables in [application settings](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings#settings)
* Change trigger function cron in `Run` method header
