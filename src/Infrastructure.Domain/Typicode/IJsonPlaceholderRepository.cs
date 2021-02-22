using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Domain.Typicode
{
    public interface IJsonPlaceholderRepository
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> GetUserAsync(int userId);
        Task<IEnumerable<Post>> GetPostsAsync(int userId);
        Task<IEnumerable<Comment>> GetCommentsAsync(int userId);
        Task<IEnumerable<Album>> GetAlbumsAsync();
        Task<Album> GetAlbumAsync(int albumId);
        Task<IEnumerable<Photo>> GetPhotosAsync(int albumId);
    }
}
