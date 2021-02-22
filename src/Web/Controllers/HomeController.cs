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
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;
        private readonly int _pageSize;

        public HomeController(IAlbumSearchService searchService, IUserRepository userRepository, ILogger logger, int pageSize)
        {
            _searchService = searchService;
            _userRepository = userRepository;
            _logger = logger;
            _pageSize = pageSize;
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Index([FromForm] string search, [FromForm] int page = 1, [FromForm] int albumId = 0, [FromForm] int userId = 0)
        {
            _logger.Information("Performing search...");

            List<Album> albums = default;
            int pageCount = default;
            List<Post> blog = default;

            await Task.WhenAll(
                Task.Run(async () => (albums, pageCount) = await _searchService.SearchAsync(search, page, _pageSize)),
                Task.Run(async () => blog = userId != default ? await _userRepository.GetBlogAsync(userId) : new List<Post>()));

            var model = new SearchViewModel
            {
                SearchValue = search,
                Page = page,
                PageCount = pageCount,
                Albums = (
                    from a in albums
                    let suite = a.User.Address.Suite
                    let cityZip = $"{a.User.Address.City}, {a.User.Address.ZipCode}"
                    select new AlbumViewModel
                    {
                        AlbumId = a.AlbumId,
                        UserId = a.User.UserId,
                        AlbumSelected = a.AlbumId == albumId && userId == default,
                        BlogSelected = a.AlbumId == albumId && userId != default,
                        Title = a.Title,
                        Thumbnail = a.Thumbnail,
                        Photos = (
                            from p in a.Photos
                            select new PhotoViewModel
                            {
                                Title = p.Title,
                                Thumbnail = p.Url
                            }).ToList(),
                        UserName = a.User.Name,
                        Email = a.User.Email,
                        Phone = a.User.Phone,
                        AddressLine1 = a.User.Address.Street,
                        AddressLine2 = !String.IsNullOrWhiteSpace(suite) ? suite : cityZip,
                        AddressLine3 = !String.IsNullOrWhiteSpace(suite) ? cityZip : String.Empty,
                        GeoLocation = a.User.Address.GeoLocation,
                        Blog = (
                            from post in blog
                            select new PostViewModel
                            {
                                Title = post.Title,
                                Body = post.Body,
                                Comments = (
                                    from c in post.Comments
                                    select new CommentViewModel
                                    {
                                        Name = c.Name,
                                        Email = c.Email,
                                        Body = c.Body
                                    }).ToList()
                            }).ToList()
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
