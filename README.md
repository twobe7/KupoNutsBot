# FCChan Discord bot

## Requirements
 * .NET Core 3 or later.
 * Visual Studio 2019 or equivalent.

## Getting Started
* Open `FCBot.sln`.
* Build and Run `FC.boot` to launch the bot.
* Navigate to `http://localhost:5000` in the web browser of your choice to access the mangement interface.
* Select "Settings" from the navigation bar on the left.
* Enter the necessary keys:
    - Discord Token, Key, and Secret (Mandatory) https://discordapp.com/developers/applications/
    - Twitter Keys (for Fashion Report) https://developer.twitter.com/en/apps
    - XIVAPI Key (character and item data) https://xivapi.com/account

## Notes
* Press [ESC] to shut the bot down safely.
* FC Chan fits comfortably in the Amazon AWS Free Tier for DynamoDB and EC2.
* To set up FCChan Bot on an EC2 isntance requires a web server to be configured.
* _never_ commit any Keys, tokens, or authentication information to GitHUB. New tokens should be added to the settings interface.

## Databases
In production, FCChan is backed by Amazon Dynamo DB, however in testing it is fine to let the bot fall back to local json document storage.
* To use FCChan Bot outside of an Amazon EC2 instance with DynamoDB, you will need to set up a DynamoDB connection.
    - an AWS account is required (free): https://aws.amazon.com/
    - Get your Access Key and secret: https://console.aws.amazon.com/iam/home?region=ap-southeast-2#/security_credentials
    - Add the key and secret to the Settings.json file: "DBKey": "[YOUR_KEY]", "DBSecret": "[YOUR_SECRET]"
    - Restart bot
