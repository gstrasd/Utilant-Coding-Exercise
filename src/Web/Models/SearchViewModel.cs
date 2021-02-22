using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Geolocation;

namespace Web.Models
{
    public class SearchViewModel
    {
        public string SearchValue { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public List<AlbumViewModel> Albums { get; set; }
    }

    public class AlbumViewModel
    {
        public int AlbumId { get; set; }
        public bool Selected { get; set; }
        public string Title { get; set; }
        public Uri Thumbnail { get; set; }
        public List<PhotoViewModel> Photos { get; set; }
        public string UserName { get; set; }
        public MailAddress Email { get; set; }
        public string Phone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public Coordinate GeoLocation { get; set; }
    }

    public class PhotoViewModel
    {
        public string Title { get; set; }
        public Uri Thumbnail { get; set; }
    }
}
