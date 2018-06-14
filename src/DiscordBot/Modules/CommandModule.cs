using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System;
using Discord.WebSocket;

namespace DiscordBot.Modules
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        
        [Command("info")]
        public Task Info()
            => ReplyAsync(
                $"Hello, I am a bot called {Context.Client.CurrentUser.Username} written in Discord.Net 1.0\n");


        [Command("seas", RunMode = RunMode.Async)]
        [Summary("Begrüßt User im Discord Channel")]
        public async Task SayHello()
        {
            var user = Context.Message.Author;
            await ReplyAsync("Seas, " + user.Mention);
        }

        [Command("scheiss", RunMode = RunMode.Async)]
        public async Task ScheissUsername(string username)
        {
            IGuildUser userx = null;
            var channel = Context.Guild.GetTextChannel(328917865786900480);
            var channel2 = (ITextChannel)channel;
            var users = await channel2.GetUsersAsync().Flatten();
            foreach (var user in users)
            {
                if (user.Username.Contains(username))
                {
                    userx = user;
                }
            }



            await channel2.SendMessageAsync("Scheiß " + userx.Mention + "!");
            //await ReplyAsync("Scheiß " + userx.Mention + "!");
        }
    }
}
