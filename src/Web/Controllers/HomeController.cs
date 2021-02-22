using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Domain;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAlbumSearchService _searchService;
        private readonly ILogger _logger;
        private readonly int _pageSize;

        public HomeController(IAlbumSearchService searchService, ILogger logger, int pageSize)
        {
            _searchService = searchService;
            _logger = logger;
            _pageSize = pageSize;
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Index([FromForm] string search, [FromForm] int page = 1, [FromForm] int albumId = 0)
        {
            _logger.Information("Performing search...");

            var (results, pageCount) = await _searchService.SearchAsync(search, page, _pageSize);
            var model = new SearchViewModel
            {
                SearchValue = search,
                Page = page,
                PageCount = pageCount,
                Albums = (
                    from r in results
                    let suite = r.User.Address.Suite
                    let cityZip = $"{r.User.Address.City}, {r.User.Address.ZipCode}"
                    select new AlbumViewModel
                    {
                        AlbumId = r.AlbumId,
                        Selected = r.AlbumId == albumId,
                        Title = r.Title,
                        Thumbnail = r.Thumbnail,
                        Photos = (
                            from p in r.Photos
                            select new PhotoViewModel
                            {
                                Title = p.Title,
                                Thumbnail = p.Url
                            }).ToList(),
                        UserName = r.User.Name,
                        Email = r.User.Email,
                        Phone = r.User.Phone,
                        AddressLine1 = r.User.Address.Street,
                        AddressLine2 = !String.IsNullOrWhiteSpace(suite) ? suite : cityZip,
                        AddressLine3 = !String.IsNullOrWhiteSpace(suite) ? cityZip : String.Empty,
                        GeoLocation = r.User.Address.GeoLocation
                    }).ToList()
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
