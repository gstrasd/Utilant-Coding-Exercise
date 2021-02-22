using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Domain
{
    public class Album
    {
        public int AlbumId { get; set; }
        public string Title { get; set; }
        public Uri Thumbnail { get; set; }
        public List<Thumbnail> Photos { get; set; }
        public User User { get; set; }
    }

    public class Thumbnail
    {
        public string Title { get; set; }
        public Uri Url { get; set; }
    }
}
