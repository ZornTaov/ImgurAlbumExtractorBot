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
        }

        internal static void OnText(EventArgs args) 
        {
            if (args is MessageEventArgs)
            {
                Message msg = (args as MessageEventArgs).msg;
                if (msg.text.ToLower().StartsWith("/ping"))
                {
                    Methods.sendMessage(msg.chat.id, "pong");
                }
                else if (msg.text.ToLower().StartsWith("/showalbum") || msg.text.ToLower().Contains("imgur.com/gallery"))
                {
                    MatchCollection mc = Regex.Matches(msg.text, "(imgur\\.com\\/gallery.*?)(?>\\s|$)");
                    if (mc.Count == 0)
                    {
                        Methods.sendMessage(msg.chat.id, "no gallery link");
                        return;
                    }
                    foreach (Match m in mc)
                    {
                        MatchCollection m2 = Regex.Matches(m.Value, "(?>gallery\\/)(.*?)$");
                        if (m2.Count == 0)
                        {
                            Methods.sendMessage(msg.chat.id, "no gallery hash");
                            return;
                        }
                        string uriMethod = "https://api.imgur.com/3/album/" + m2[0].Value.Split('/')[1] + "/images";
                        var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("Authorization", "Client-ID " + "b265968728f82eb");
                        var response = Task.Run(() => client.GetAsync(uriMethod)).Result;
                        client.Dispose();
                        Console.WriteLine("Got Response from API");
                        Stream stream = Task.Run(() => response.Content.ReadAsStreamAsync()).Result;
                        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(ImgurResult<Image[]>));
                        try
                        {
                            ImgurResult<Image[]> result = (dcjs.ReadObject(stream)) as ImgurResult<Image[]>;
                            if (result.success)
                            {
                                Methods.sendMessage(msg.chat.id, "SUCCESS: found " + result.data.Length + "images! Give me a moment to send them all!");
                                Methods.sendChatAction(msg.chat.id, "typing");
                                Console.WriteLine("SUCCESS: found " + result.data.Length + "images");
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
                                    }
                                    if (gifMedia.Count() > 0)
                                    {
                                        foreach (var gif in gifMedia)
                                        {
                                            Methods.sendChatAction(msg.chat.id, "upload_video");
                                            Methods.sendAnimation(msg.chat.id, gif.media, "");
                                        }
                                    }
                                    if (videoMedia.Count() > 0)
                                    {
                                        foreach (var vid in videoMedia)
                                        {
                                            Methods.sendChatAction(msg.chat.id, "upload_video");
                                            Methods.sendAnimation(msg.chat.id, vid.media, "");
                                        }
                                    }
                                    pos += subcount;
                                    count -= subcount;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Methods.sendMessage(msg.chat.id, "Not an album");
                        }
                    }
                }
            }
        }
        public void PostInit()
        {

        }
    }
}
