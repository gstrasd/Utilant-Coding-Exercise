using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Domain
{
    public interface IAlbumSearchService
    {
        Task<(List<Album> Albums, int PageCount)> SearchAsync(string value, int page, int pageSize);
    }
}