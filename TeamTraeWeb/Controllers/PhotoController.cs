using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.MetaData;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace TeamTraeWeb.Controllers
{
    [Produces("application/json")]
    [Route("api/Photo")]
    public class PhotoController : Controller
    {
        private readonly ConnectionStrings cstrs;

        public PhotoController(IServiceProvider serviceProvider)
        {
            cstrs = serviceProvider.GetService<ConnectionStrings>();
        }

        private DocumentClient GetDocClient()
        {
            var client = new DocumentClient(new Uri("https://teamtrae.documents.azure.com:443/"), cstrs.Cosmos);
            return client;
        }

        // GET: api/Photo
//        [HttpGet]
//        [Produces("image/jpeg")]
//        public void Get(string )
//        {
//        }

        // GET: api/Photo/5
        [HttpGet("{id}", Name = "Get")]
        [Produces("image/jpeg")]
        public async Task Get(string id)
        {
#if false
            var client = GetDocClient();
            var documentUri = UriFactory.CreateDocumentCollectionUri("TeamTrae", "Photos");
            var query = client.CreateDocumentQuery<TTPhoto>(documentUri, $"select top 1 P.WebPhotoData from Photos as P where P.Id='{id}'").AsEnumerable().FirstOrDefault();

            var bdata = Convert.FromBase64String(query.WebPhotoData);
#endif

            var bdata = await DownloadBlob("scaled", id);

            Response.ContentType = "image/jpeg";
            Response.Body.Write(bdata, 0, bdata.Length);
        }

        public class TTPhoto
        {
            public string Id { get; set; }
            public DateTime Timestamp { get; set; }
            public double LocationLong { get; set; }
            public double LocationLat { get; set; }
            // public string PhotoData { get; set; }
            public bool IsKeyFrame { get; set; }
            // public string WebPhotoData { get; set; }
        }

        private bool SetAsKeyFrame(DateTime photoTime)
        {
            var client = GetDocClient();
            var documentUri = UriFactory.CreateDocumentCollectionUri("TeamTrae", "Photos");
            var query = client.CreateDocumentQuery<TTPhoto>(documentUri, "select top 1 P.Timestamp from Photos as P where P.IsKeyFrame=true order by P.Timestamp desc").AsEnumerable().FirstOrDefault();

            return query == null || (photoTime - query.Timestamp).TotalMinutes > 10;
        }

        class GeoLoc
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public DateTime Timestamp { get; set; }

            static double ToDeg(ExifValue val)
            {
                var parts = val.Value as SixLabors.ImageSharp.Primitives.Rational[];
                return parts[0].ToDouble() + parts[1].ToDouble() / 60 + parts[2].ToDouble() / 3600;
            }

            public static GeoLoc FromExif(ImageMetaData metaData)
            {
                var lat = metaData.ExifProfile.Values.FirstOrDefault(v => v.Tag == ExifTag.GPSLatitude);
                var lng = metaData.ExifProfile.Values.FirstOrDefault(v => v.Tag == ExifTag.GPSLongitude);

                var dte = metaData.ExifProfile.Values.FirstOrDefault(v => v.Tag == ExifTag.DateTime);
                var strval = dte.Value.ToString();
                strval = strval.Substring(0, 10).Replace(":", "-") + strval.Substring(10);

                return new GeoLoc()
                {
                    Latitude = ToDeg(lat),
                    Longitude = ToDeg(lng),
                    Timestamp = DateTime.Parse(strval)
                };
            }

        }

        private byte[] RotateAndScale(byte[] photo, out GeoLoc geoLoc)
        {
            using (var image = Image.Load(photo))
            {
                try
                {
                    geoLoc = GeoLoc.FromExif(image.MetaData);
                }
                catch
                {
                    geoLoc = null;
                }

                image.Mutate(x => x
                     .AutoOrient()
                     .Resize(new ResizeOptions() { Size = new SixLabors.Primitives.Size(1024, 768), Mode = ResizeMode.Max }));

                var ms = new MemoryStream();
                image.SaveAsJpeg(ms);
                // return Convert.ToBase64String(ms.ToArray());
                return ms.ToArray();
            }
        }

        private Task UploadBlob(string containerName, string id, byte[] data)
        {
            var storage = CloudStorageAccount.Parse(cstrs.Blob);
            var bclient = storage.CreateCloudBlobClient();
            var container = bclient.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(id);
            return blob.UploadFromByteArrayAsync(data, 0, data.Length);
        }

        private async Task<byte[]> DownloadBlob(string containerName, string id)
        {
            var storage = CloudStorageAccount.Parse(cstrs.Blob);
            var bclient = storage.CreateCloudBlobClient();
            var container = bclient.GetContainerReference(containerName);
            var blob = await container.GetBlobReferenceFromServerAsync(id);

            var target = new byte[blob.Properties.Length];
            var n = await blob.DownloadToByteArrayAsync(target, 0);

            return target;
        }

        // POST: api/Photo
        [HttpPost]
        public async Task Post()
        {
            var b64photo = new StreamReader(Request.Body).ReadToEnd();

            var client = GetDocClient();
            var documentUri = UriFactory.CreateDocumentCollectionUri("TeamTrae", "Photos");

            var photodata = Convert.FromBase64String(b64photo);
            var webphoto = RotateAndScale(photodata, out GeoLoc loc);
            var tnow = loc?.Timestamp ?? DateTime.Now;
            var isKeyFrame = SetAsKeyFrame(tnow);

            TTPhoto photo = new TTPhoto()
            {
                Id = tnow.ToString("yyyy_MM_dd_HH_mm_ss_", CultureInfo.InvariantCulture) + Guid.NewGuid().ToString(),
                Timestamp = tnow,
                // PhotoData = b64photo,
                LocationLat = loc?.Latitude ?? 0.0,
                LocationLong = loc?.Longitude ?? 0.0,
                // WebPhotoData = webphoto,
                IsKeyFrame = isKeyFrame
            };

            try
            {
                await UploadBlob("originals", photo.Id, photodata);
                await UploadBlob("scaled", photo.Id, webphoto);

                var resp = await client.CreateDocumentAsync(documentUri, photo);
            }
            catch
            {
                // I died?!
            }
        }

        // PUT: api/Photo/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
