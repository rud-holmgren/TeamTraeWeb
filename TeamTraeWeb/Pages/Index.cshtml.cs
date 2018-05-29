using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;

namespace TeamTraeWeb.Pages
{
    public class IndexModel : PageModel
    {
        public class TTPhoto
        {
            public string Id { get; set; }
            public DateTime Timestamp { get; set; }
            public double LocationLong { get; set; }
            public double LocationLat { get; set; }
            public bool IsKeyFrame { get; set; }
        }

        private readonly ConnectionStrings cstrs;

        public IndexModel(IServiceProvider serviceProvider)
        {
            cstrs = serviceProvider.GetService<ConnectionStrings>();
        }

        private DocumentClient GetDocClient()
        {
            var client = new DocumentClient(new Uri("https://teamtrae.documents.azure.com:443/"), cstrs.Cosmos);
            return client;
        }

        public string GetPhotoCoord(TTPhoto photo)
        {
            return $"{photo.LocationLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {photo.LocationLong.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        }

        public TTPhoto[] Photos { get; set; } 

        public async Task<IActionResult> OnGet()
        {
            var client = GetDocClient();
            var documentUri = UriFactory.CreateDocumentCollectionUri("TeamTrae", "Photos");
            Photos = await client.CreateDocumentQuery<TTPhoto>(documentUri, "select P.Id, P.LocationLong, P.LocationLat, P.IsKeyFrame from Photos as P where P.IsKeyFrame=true order by P.Timestamp desc").ToAsyncEnumerable().ToArray();

            return Page();
        }
    }
}
