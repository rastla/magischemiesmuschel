using Discord;
using Discord.Commands;
using Discord.Audio;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using YoutubeExtractor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace rastla_bot
{
    class MyBot
    {
        DiscordClient discord;
        CommandService commands;
        AudioService audios;

        IAudioClient radioClient;

        Random rand;

        string[] twitchEmotes;
        string[] magischeAntworten;


        public MyBot()
        {
            rand = new Random();

            magischeAntworten = new string[]
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

            twitchEmotes = new string[]
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

            // Initialisiere DiscordClientService
            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            // Befehle um Bot anzusteuern
            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            // Voice Einrcihtung
            discord.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;

            });

            // Initialisiere Command Service
            commands = discord.GetService<CommandService>();
            audios = discord.GetService<AudioService>();

            try
            {
                RegisterVoiceChatCommand();
                RegisterCommands();
                trackChat();
            }
            catch (Exception ex)
            {
                discord.Servers.LastOrDefault().TextChannels.LastOrDefault().SendMessage(ex.Message);
                // e.Channel.SendMessage(ex.Message);
            }

            // Bot Login
            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect("<your bot token here>", TokenType.Bot);
            });
        }

        private void trackChat()
        {
            discord.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor)
                {
                    if (e.Message.RawText.ToLower() == "ping")
                        await e.Message.Channel.SendMessage("pong");

                    if (e.Message.IsMentioningMe())
                    {
                        if (e.Message.RawText[e.Message.RawText.Length - 1].Equals('?'))
                        {
                            var antwort = "";
                            if (e.Message.RawText.Contains("dota oder lol?"))
                            {
                                antwort = "https://www.youtube.com/watch?v=MzuAFBr2hI4";
                            }
                            else
                            {
                                antwort = magischeAntworten[rand.Next(magischeAntworten.Length)];
                            }
                            await e.Channel.SendMessage(antwort);
                        }
                        else
                        {
                            int randomMemeIndex = rand.Next(twitchEmotes.Length);
                            string memeToPost = twitchEmotes[randomMemeIndex];
                            await e.Channel.SendMessage(memeToPost);
                        }
                    }
                }

            };
        }

        private void RegisterCommands()
        {
            commands.CreateCommand("haas")
                .Do(async (e) =>
                {
                    await e.Server.FindChannels("allgemein", ChannelType.Text).FirstOrDefault().SendMessage("Scheiß Haas!");
                });

            commands.CreateCommand("kraller")
                .Do(async (e) =>
                {
                    await e.Server.FindChannels("allgemein", ChannelType.Text).FirstOrDefault().SendMessage("Scheiß Kraller!");
                });

            


        }

        private void RegisterVoiceChatCommand()
        {
            commands.CreateCommand("joinvoice")
                .Parameter("VoiceChannel", ParameterType.Required)
                .Do(async (e) =>
                {
                    Console.WriteLine($"received radio command with parameter {e.GetArg("VoiceChannel")}");
                    var radioChannel = e.Server.FindChannels(e.GetArg("VoiceChannel"), ChannelType.Voice).FirstOrDefault();
                    if(radioChannel != null)
                    {
                        Console.WriteLine("found voice channel: " + radioChannel.ToString());
                        Console.WriteLine("attempting to join...");

                        radioClient = await radioChannel.JoinAudio();
                        Console.WriteLine("joined voice...");
                    } else
                    {
                        await e.Channel.SendMessage("Schreib den Channel Namen richtig... <:FailFish:302446038902374400>");
                    }
                });


            commands.CreateCommand("playlocal")
                .Parameter("fileUrl", ParameterType.Required)
                .Do((e) =>
                {
                    if (radioClient != null)
                    {
                        SendAudio(e.GetArg("fileUrl"));
                        //SendAudio("K:\\music\\00000\\Smash Mouth - All Star.mp3");
                    }
                    else e.Channel.SendMessage("I bin in kan Voice Channel <:FailFish:302446038902374400>");
                });

            commands.CreateCommand("playinternet")
                .Parameter("videoUrl", ParameterType.Required)
                .Do((e) =>
                {
                    try
                    {
                        //string link = "https://www.youtube.com/watch?v=y6120QOlsfU";
                        string link = e.GetArg("videoUrl");


                        e.Channel.SendMessage("Downloading audio...");
                        string pfad = DownloadMp4(link);

                        e.Channel.SendMessage("Finished, starting to stream...");
                        SendAudioFFMPEG(Path.Combine("D:\\Downloads", pfad));
                    } catch(Exception ex)
                    {
                        e.Channel.SendMessage(ex.Message);
                    }
                    
                });

            commands.CreateCommand("volume")
                .Parameter("vol",ParameterType.Required)
                .Do((e) =>
                {
                    /**/
                });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        public void SendAudio(string filePath)
        {
            var channelCount = discord.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
            var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
            using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
            using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
            {
                resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                byte[] buffer = new byte[blockSize];
                int byteCount;

                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                {
                    if (byteCount < blockSize)
                    {
                        // Incomplete Frame
                        for (int i = byteCount; i < blockSize; i++)
                            buffer[i] = 0;
                    }

                    // Rücksicht auf Lautstärke
                    for (int i = 0; i < blockSize / 2; ++i)
                    {

                        // convert to 16-bit
                        short sample = (short)((buffer[i * 2 + 1] << 8) | buffer[i * 2]);

                        // scale
                        const double gain = 0.5; // value between 0 and 1.0
                        sample = (short)(sample * gain + 0.5);

                        // back to byte[]
                        buffer[i * 2 + 1] = (byte)(sample >> 8);
                        buffer[i * 2] = (byte)(sample & 0xff);
                    }
                    // Rücksicht auf Lautstärke

                    radioClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                }
            }
        }

        public string DownloadMp4(string videoUrl)
        {
            /*
                     * Get the available video formats.
                     * We'll work with them in the video and audio download examples.
                     */
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(videoUrl);

            /*
             * Select the first .mp4 video with 360p resolution
             */
            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 0);

            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */
            var videoDownloader = new VideoDownloader(video, Path.Combine("D:/Downloads", video.Title + video.VideoExtension));

            // Register the ProgressChanged event and print the current progress
            videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            videoDownloader.Execute();

            return Path.Combine("D:/Downloads", video.Title + video.VideoExtension);
        }

        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }


        public void SendAudioFFMPEG(string pathOrUrl)
        {
            var process = Process.Start(new ProcessStartInfo
            { // FFmpeg requires us to spawn a process and hook into its stdout, so we will create a Process
                FileName = "ffmpeg",
                Arguments = $"-i \"{pathOrUrl}\" " + // Here we provide a list of arguments to feed into FFmpeg. -i means the location of the file/URL it will read from
                            "-f s16le -ar 48000 -ac 2 pipe:1", // Next, we tell it to output 16-bit 48000Hz PCM, over 2 channels, to stdout.
                UseShellExecute = false,
                RedirectStandardOutput = true // Capture the stdout of the process
            });
            Thread.Sleep(2000); // Sleep for a few seconds to FFmpeg can start processing data.

            int blockSize = 3840; // The size of bytes to read per frame; 1920 for mono
            byte[] buffer = new byte[blockSize];
            int byteCount;

            while (true) // Loop forever, so data will always be read
            {
                byteCount = process.StandardOutput.BaseStream // Access the underlying MemoryStream from the stdout of FFmpeg
                        .Read(buffer, 0, blockSize); // Read stdout into the buffer

                if (byteCount == 0) // FFmpeg did not output anything
                    break; // Break out of the while(true) loop, since there was nothing to read.

                radioClient.Send(buffer, 0, byteCount); // Send our data to Discord
            }
            radioClient.Wait(); // Wait for the Voice Client to finish sending data, as ffMPEG may have already finished buffering out a song, and it is unsafe to return now.
        }
    }
}
