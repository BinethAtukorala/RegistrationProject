
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using System;

using System.Threading.Tasks;

namespace RegistrationProject.Services.DiscordBot
{
    public class BotService
    {
        public DiscordClient Client { get; private set; }
        public static BotConfig Config { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public async Task ExecuteAsync(BotConfig botConfig)
        {



            DiscordConfiguration config = new()
            {
                Token = botConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,




            };
            Config = botConfig;
            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = new string[] { botConfig.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                DmHelp = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<Commands>();


            await Client.ConnectAsync();


        }



        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
