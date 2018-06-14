using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExtractorCore;

namespace DiscordBot.Services
{
    public class AudioService : ModuleBase<ICommandContext>
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private List<string> playlist = new List<string>();
        private bool currentlyPlaying = false;

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;
            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                await client.StopAsync();
                currentlyPlaying = false;
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }


        public string QueueTrack(string audioFile)
        {
            playlist.Add(audioFile);
            return "Queued the song " + audioFile;
        }

        public async Task QueueOrPlay(string audioFile, IGuild guild, IMessageChannel channel)
        {
            audioFile = await CheckDownloadAndQueue(audioFile);
            if (!currentlyPlaying)
            {
                currentlyPlaying = true;
                await channel.SendMessageAsync("Playing " + audioFile);
                await PlayMusic(guild, channel);
            }
            else
            {
                await channel.SendMessageAsync("Queued the song " + audioFile);
            }
        }


        public async Task PlayMusic(IGuild guild, IMessageChannel channel)
        {
            IAudioClient client;
            Console.WriteLine("Status: Checking if currently playing");
            while (currentlyPlaying)
            {
                if (playlist.Count == 0)
                {
                    currentlyPlaying = false;
                    Console.WriteLine("Status: Playlist count = 0. STopping playback...");
                }
                else if (ConnectedChannels.TryGetValue(guild.Id, out client))
                {
                    Console.WriteLine("Status: Streaming audio...");
                    Console.WriteLine($"Starting playback of {playlist[0]} in {guild.Name}");
                    try
                    {
                        var output = CreateStream(playlist[0]).StandardOutput.BaseStream;
                        var stream = client.CreatePCMStream(AudioApplication.Music, 48000);
                        await output.CopyToAsync(stream);
                        await stream.FlushAsync().ConfigureAwait(false);
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    playlist.Remove(playlist[0]);
                }
            }
            Console.WriteLine("Status: Not playing ...");
        }

        //public async Task On

        public async Task<string> CheckDownloadAndQueue(string path)
        {
            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            try
            {
                //string temp = null;
                if (true)
                {
                    Console.WriteLine("Calling function DownloadMP4");
                    //await channel.SendMessageAsync("Status: Downloading audio...");
                    path = await DownloadMp4(path);

                    Console.WriteLine("Status: Finished");
                    //Console.WriteLine("Finished, starting to stream...");
                }

                /*if (!File.Exists(path))
                {
                    Console.WriteLine("File '"+path+"' does not exist.");
                    return "UNKNOWN_FILE_PATH";
                }*/

                QueueTrack(path);
                return path;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = @"D:\ffmpeg.exe",
                Arguments = $"-loglevel warning -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        private string ConvertToMp3(string pathmp4)
        {
            string pathmp3 = pathmp4.Replace(".mp4", ".mp3");
            Process.Start(new ProcessStartInfo
            {
                FileName = @"D:\ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{pathmp4}\" -vn -f mp3 -ab 192k \"{pathmp3}\" -n",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            Console.WriteLine("Converted to mp3");
            return pathmp3;
        }

        public async Task<string> DownloadMp4(string videoUrl)
        {
            /*
                     * Get the available video formats.
                     * We'll work with them in the video and audio download examples.
                     */
            Console.WriteLine("Getting Download URL");
            IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetVideoUrlsAsync(videoUrl, info => info.Resolution == 0);
            VideoInfo video = videoInfos.FirstOrDefault();

            Console.WriteLine("Got it... getting videoType mp4 and resolution0");
            /*VideoInfo video = videoInfos
                .FirstOrDefault(info => info.VideoType == VideoType.Mp4 && info.Resolution == 0);*/

            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                Console.WriteLine("Video requires decryption... decrypting");
                //DownloadUrlResolverDecryptDownloadUrl(video);
                Console.WriteLine("Finished decrypting");
            }

            var downloadPath = Path.Combine(@"D:\Downloads\miesmuschel\", RemoveIllegalPathCharacters(video.Title + video.VideoExtension));
            Console.WriteLine("Downloadpath will be: " + downloadPath);
            if (!File.Exists(downloadPath))
            {
                Console.WriteLine("Downloading audio...");
                HttpClient web = new HttpClient();
                Stream response = await web.GetStreamAsync(video.DownloadUrl);
                using (FileStream fileStream = new FileStream(downloadPath, FileMode.Create))
                {
                    //copy the content from response to filestream
                    await response.CopyToAsync(fileStream);
                }
                Console.Write("\n");
                Console.WriteLine("Finished Download");
            }
            else
            {
                Console.WriteLine("Skipping download, file already exists...");
            }
            return ConvertToMp3(downloadPath);
        }

        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }
    }
}