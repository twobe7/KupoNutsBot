# Kupo Nuts, the Bot

## Installation

* Install .NET Core SDK 3 and your favourite Visual Studio equivalent (VS2019 and Rider work fine)
* Open `KupoNutsBot.sln` and build the solution so it grabs all the NuGet dependencies
* Run the `KupoNuts.Boot` target, you should get a console that spits errors and cries about stuff - this is acceptable. Your new Kupo Nuts bot must learn the pains of The Void before it can integrate with the human world.
* Kill the program (Ctrl+C the console app it spawns) and open `./bin/settings.json`

```json
{
    "Token": null,
    "LogChannel": null,
    "StatusChannel": null,
    "StatusMessage": null,
    "CalendarChannel": null,
    "CalendarMessage": null,
    "CalendarMessage2": null,
    "FashionReportChannel": null,
    "DBKey": null,
    "DBSecret": null,
    "DiscordKey": null,
    "DiscordSecret": null,
    "TwitterConsumerKey": null,
    "TwitterConsumerSecret": null,
    "TwitterToken": null,
    "TwitterTokenSecret": null,
    "XIVAPIKey": null,
    "UserLogChannel": null,
    "LodestoneChannel": null
}
```

> You'll need to supply the relative service keys before the web manager console is usable and the bot starts doing things. The `DBKey` and `DBSecret` tokens are DynamoDB tokens from AWS, so you'll need to set up your own account for that.

> TODO: Decipher the hidden meaning of the other token keys, and write a templating file or script to generate it easily.

* Once configured correctly, run the `KupoNutsBoot` target again, and check the manager console on `http://localhost:5000`. If nothing in the spawned console crashes you're good to go!

* Press [ESC] to shut the bot down safely.
