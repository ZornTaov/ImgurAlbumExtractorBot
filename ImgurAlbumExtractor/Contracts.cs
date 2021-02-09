using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
namespace ImgurAlbumExtractor
{
    [DataContract]
    class ImgurResult<T>
    {
        [DataMember]
        public T data { get; set; }

        [DataMember]
        public bool success { get; set; }

        [DataMember]
        public int status { get; set; }
}
    [DataContract]
    class Image
    {

        [DataMember]
        //The ID for the image
        public string id { get; set; }

        [DataMember]
        //The title of the image.
        public string title { get; set; }

        [DataMember]
        //Description of the image.
        public string description { get; set; }

        [DataMember]
        //Image MIME type.
        public string type { get; set; }

        [DataMember]
        //is the image animated
        public bool? animated { get; set; }

        [DataMember]
        //The width of the image in pixels
        public int width { get; set; }

        [DataMember]
        //The height of the image in pixels
        public int height { get; set; }

        [DataMember]
        //The size of the image in bytes
        public int size { get; set; }

        [DataMember]
        //The direct link to the the image. (Note: if fetching an animated GIF that was over 20MB in original size, a .gif thumbnail will be returned)
        public string link { get; set; }

        [DataMember(IsRequired = false)]
        //OPTIONAL, The .gifv link. Only available if the image is animated and type is 'image/gif'.
        public string gifv { get; set; }

        [DataMember(IsRequired = false)]
        //OPTIONAL, The direct link to the .mp4. Only available if the image is animated and type is 'image/gif'.
        public string mp4 { get; set; }

        [DataMember(IsRequired = false)]
        //OPTIONAL, The Content-Length of the .mp4. Only available if the image is animated and type is 'image/gif'. Note that a zero value (0) is possible if the video has not yet been generated
        public int mp4_size { get; set; }

        [DataMember(IsRequired = false)]
        //OPTIONAL, Whether the image has a looping animation. Only available if the image is animated and type is 'image/gif'.
        public bool? looping { get; set; }

        [DataMember]
        //Indicates if the image has been marked as nsfw or not. Defaults to null if information is not available.
        public bool? nsfw { get; set; }
    }
}
