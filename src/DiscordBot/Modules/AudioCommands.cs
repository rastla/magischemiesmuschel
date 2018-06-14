using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Collections.Generic;
using System;
using DiscordBot.Services;

namespace DiscordBot.Modules
{
    public class AudioCommands : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService audioService;

        public AudioCommands(AudioService service)
        {
            audioService = service;
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            try
            {
                await Context.Message.Channel.SendMessageAsync("Join cmd called");
                // Get the audio channel
                channel = channel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;
                if (channel == null)
                {
                    await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                    //return false;
                }
                await audioService.JoinAudio(Context.Guild, channel);
                //return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //return false;
            }
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveChannel()
        {
            await audioService.LeaveAudio(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayMusic(string audioFile)
        {
            /*Console.WriteLine("checking if bot is in voice channel");
            var curVC = (Context.User as IGuildUser);
            if (curVC.VoiceChannel == null)
            {
                await JoinChannel();
            }

            Console.WriteLine("bot is in voice channel");*/
            await audioService.QueueOrPlay(audioFile, Context.Guild, Context.Channel);
        }

        [Command("queue", RunMode = RunMode.Async)]
        public async Task QueueMusic(string audioFile)
        {
            Console.WriteLine("QueueMusic Cmd Called");
            string queueResult = audioService.QueueTrack(audioFile);
            await Context.Channel.SendMessageAsync(queueResult);
        }
    }
}
