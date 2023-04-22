using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using DreadBot;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Runtime.Serialization.Json;

namespace ImgurAlbumExtractor
{
    public class ImgurMain : IDreadBotPlugin
    {
        public string PluginID { get { return "ImgurAlbumExtractor"; } }

        public void Init()
        {
            Events.TextEvent += OnText;
            Logger.CurrentLogLevel = LogLevel.Debug;
        }

        internal static void OnText(EventArgs args) 
        {
            if (args is MessageEventArgs)
            {
                Message msg = (args as MessageEventArgs).msg;
                if (msg.text.ToLower().StartsWith("/ping"))
                {
                    Methods.sendMessage(msg.chat.id, "pong");
                    Logger.LogInfo("pong");
                }
                else if (msg.text.ToLower().StartsWith("/showalbum") || msg.text.ToLower().Contains("imgur.com/gallery") 
                    || msg.text.ToLower().Contains("imgur.com/a/"))
                {
                    Task.Run(() => ParseAlbum(msg));
                }
            }
        }

        private static void ParseAlbum(Message msg)
        {
            MatchCollection mc = Regex.Matches(msg.text, "(imgur\\.com\\/(?>gallery|a).*?)(?>\\s|$)");
            if (mc.Count == 0)
            {
                Methods.sendMessage(msg.chat.id, "no gallery link");
                Logger.LogInfo("no gallery link");
                return;
            }
            foreach (Match m in mc)
            {
                MatchCollection m2 = Regex.Matches(m.Value, "(?>gallery|a)\\/(.*?)$");
                if (m2.Count == 0)
                {
                    Methods.sendMessage(msg.chat.id, "no gallery hash");
                    Logger.LogInfo("no gallery hash");
                    continue;
                }
                string uriMethod = "https://api.imgur.com/3/album/" + m2[0].Value.Split('/')[1] + "/images";
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Client-ID " + "b265968728f82eb");
                var response = Task.Run(() => client.GetAsync(uriMethod)).Result;
                client.Dispose();
                Logger.LogInfo("Got Response from API for link: " + m.Value);
                Stream stream = Task.Run(() => response.Content.ReadAsStreamAsync()).Result;
                ExtractAlbum(msg, stream);
            }
        }

        private static void ExtractAlbum(Message msg, Stream stream)
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(ImgurResult<Image[]>));
            try
            {
                ImgurResult<Image[]> result = (dcjs.ReadObject(stream)) as ImgurResult<Image[]>;
                if (result.success)
                {
                    Methods.sendMessage(msg.chat.id, "SUCCESS: found " + result.data.Length + "images! Give me a moment to send them all!");
                    Methods.sendChatAction(msg.chat.id, "typing");
                    Logger.LogInfo("SUCCESS: found " + result.data.Length + " images.");
                    int count = result.data.Length;
                    int pos = 0;
                    while (count > 0)
                    {
                        int subcount = 0;
                        List<InputMedia> media = new List<InputMedia>();
                        List<InputMedia> gifMedia = new List<InputMedia>();
                        List<InputMedia> videoMedia = new List<InputMedia>();
                        for (int i = 0; i < Math.Min(count, 10); i++)
                        {
                            InputMedia curMedia;
                            if (result.data[i + pos].link.EndsWith("mp4")) // should be video
                            {
                                curMedia = new InputMediaVideo()
                                {
                                    type = "video",
                                    media = result.data[i + pos].link,
                                    height = result.data[i + pos].height,
                                    width = result.data[i + pos].width
                                };
                                videoMedia.Add(curMedia);
                                subcount++;
                                break;
                            }
                            if (result.data[i + pos].link.EndsWith("gif")) // should be video
                            {
                                curMedia = new InputMediaAnimation()
                                {
                                    type = "animation",
                                    media = result.data[i + pos].link,
                                    height = result.data[i + pos].height,
                                    width = result.data[i + pos].width
                                };
                                gifMedia.Add(curMedia);
                                subcount++;
                                break;
                            }
                            else //should be image?
                            {
                                curMedia = new InputMediaPhoto()
                                {
                                    type = "photo",
                                    media = result.data[i + pos].link
                                };
                                media.Add(curMedia);
                                subcount++;
                            }
                        }

                        if (media.Count() > 0)
                        {
                            Methods.sendChatAction(msg.chat.id, "upload_photo");
                            Methods.sendMediaGroup(msg.chat.id, media.ToArray());
                            Task.Delay(1000);
                        }
                        if (gifMedia.Count() > 0)
                        {
                            foreach (var gif in gifMedia)
                            {
                                Methods.sendChatAction(msg.chat.id, "upload_video");
                                Methods.sendAnimation(msg.chat.id, gif.media, "");
                                Task.Delay(1000);
                            }
                        }
                        if (videoMedia.Count() > 0)
                        {
                            foreach (var vid in videoMedia)
                            {
                                Methods.sendChatAction(msg.chat.id, "upload_video");
                                Methods.sendAnimation(msg.chat.id, vid.media, "");
                                Task.Delay(1000);
                            }
                        }
                        pos += subcount;
                        count -= subcount;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogInfo(e.Message);
                Methods.sendMessage(msg.chat.id, "Not an album");
            }
        }

        public void PostInit()
        {

        }
    }
}
