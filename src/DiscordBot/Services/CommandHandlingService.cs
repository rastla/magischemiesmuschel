using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot.Services
{
    public class CommandHandlingService
    {
        System.Random rand = new System.Random();

        string[] magischeAntworten = new string[]
            {
                "Auf jeden Fall!",
                "Nein!",
                "Ja!",
                "Aber sicher doch!",
                "Vielleicht irgendwann...",
                "Garnichts...",
                "Ich glaube eher nicht.",
                "Eines Tages vielleicht...",
                "Frag doch einfach nochmal!",
                "BLBLBLBLBLBLB <:BrokeBack:302446038231416833>"
            };

        string[] twitchEmotes = new string[]
            {
                "<:BrokeBack:302446038231416833>",
                "<:Kappa:302446038458040324>",
                "<:4Head:302446038135078913>",
                "<:BabyRage:302446038164176898>",
                "<:BibleThump:302446038281879553>",
                "<:BrokeBack:302446038231416833>",
                "<:CoolCat:302446039062020098>",
                "<:CoolStoryBob:302446039149969410>",
                "<:DansGame:302446038592126977>",
                "<:DendiFace:302446038822813697>",
                "<:EleGiggle:302446038776807424>",
                "<:FailFish:302446038902374400>",
                "<:HeyGuys:302446038541664267>",
                "<:HotPokket:302446038839459841>",
                "<:Jebaited:302446039305289730>",
                "<:KAPOW:302446038948511744>",
                "<:Kappa:302446038458040324>",
                "<:KappaClaus:302446038877339648>",
                "<:KappaPride:302446038705373185>",
                "<:KappaRoss:302446038952706049>",
                "<:KappaWealth:302446039032528897>",
                "<:Keepo:302446038776807426>",
                "<:Kreygasm:302446039128866816>",
                "<:LUL:302495302173065226>",
                "<:MingLee:302446038810361857>",
                "<:MrDestructoid:302446039011688458>",
                "<:NotLikeThis:302446038965551105>",
                "<:OSfrog:302446039020077056>",
                "<:PJSalt:302446038738927618>",
                "<:PogChamp:302446038957162497>",
                "<:ResidentSleeper:302446039313416192>",
                "<:SMOrc:302446039288250378>",
                "<:SeemsGood:302446039196106752>",
                "<:SoBayed:302446038726475778>",
                "<:SwiftRage:302446039284056064>",
                "<:TriHard:302446038831071232>",
                "<:VoHiYo:302446039187587082>",
                "<:VoteNay:302446039288512512>",
                "<:VoteYea:302446039204495360>",
                "<:cmonBruh:302446039238180865>",
                "<:gachiGASM:303306045223075840>",
                "<:herbert:303300490936975362>",
                "<:WutFace:302446038801973249> ",
                "<:PuppeyFace:306127948841353217>",
            };

        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private IServiceProvider _provider;

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;

            _discord.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            // Add additional initialization code here...
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            int argPos = 0;
            /*if (!(message.HasMentionPrefix(_discord.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos))) return;

            var context = new SocketCommandContext(_discord, message);*/

            var context = new SocketCommandContext(_discord, message);
            // Fragen beantworten
            if (message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                var user = context.Message.Author;
                var antwort = "";
                antwort = magischeAntworten[rand.Next(magischeAntworten.Length)];
                if (message.Content.Contains("Welcome Back!")) antwort = "♿ Rastla ♿ Programming ♿ Coming ♿ Through ♿";
                if (message.Content.Contains("bist jetzt nur mehr im botchannel")) antwort = "Immer han i die Pech :(";
                await context.Channel.SendMessageAsync(user.Mention + " " + antwort);
            }
            if (!message.HasCharPrefix('!', ref argPos)) return;

            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            if (result.Error.HasValue && 
                result.Error.Value != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ToString());

        }
    }
}
