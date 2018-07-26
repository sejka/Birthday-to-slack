# Birthday-to-slack
Azure function that posts events from calendar to slack channel.

## Configuration
* Fill both `SlackWebhookUrl` and `CalendarFileUrl` in `local.settings.json`
* Change trigger function cron in `Run` method header
