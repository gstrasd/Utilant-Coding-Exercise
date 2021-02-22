using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Infrastructure.Domain.Typicode;
using Serilog;

namespace Infrastructure
{
    internal class JsonPlaceholderRepository : IJsonPlaceholderRepository
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
        // NOTE: HttpClient is disposable, but it cannot be disposed here because it is being used as a singleton.
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public JsonPlaceholderRepository(HttpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            _logger.Debug("Getting all Typicode users.");

            var json = await _client.GetStringAsync("/users");
            var users = JsonSerializer.Deserialize<List<User>>(json, _options);
            users ??= new List<User>();

            _logger.Verbose($"Found {users.Count} Typicode users.");

            return users;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting Typicode user {userId}.");

            var json = await _client.GetStringAsync($"/users/{userId}");
            var user = JsonSerializer.Deserialize<User>(json, _options);

            _logger.Verbose($"Typicode user {userId}{(user == default ? " not" : String.Empty)} found.");

            return user;
        }

        public async Task<IEnumerable<Post>> GetPostsAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting all Typicode posts for user {userId}.");

            var json = await _client.GetStringAsync($"/users/{userId}/posts");
            var posts = JsonSerializer.Deserialize<List<Post>>(json, _options);
            posts ??= new List<Post>();

            _logger.Verbose($"Found {posts.Count} Typicode posts.");

            return posts;
        }

        public async Task<IEnumerable<Comment>> GetCommentsAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting all Typicode post comments for user {userId}.");

            var json = await _client.GetStringAsync($"/users/{userId}/comments");
            var comments = JsonSerializer.Deserialize<List<Comment>>(json, _options);
            comments ??= new List<Comment>();

            _logger.Verbose($"Found {comments.Count} Typicode post comments.");

            return comments;
        }

        public async Task<IEnumerable<Album>> GetAlbumsAsync()
        {
            _logger.Debug("Getting all Typicode albums.");

            var json = await _client.GetStringAsync("/albums");
            var albums = JsonSerializer.Deserialize<List<Album>>(json, _options);
            albums ??= new List<Album>();

            _logger.Verbose($"Found {albums.Count} Typicode albums.");

            return albums;
        }

        public async Task<Album> GetAlbumAsync(int albumId)
        {
            if (albumId <= 0) throw new ArgumentOutOfRangeException(nameof(albumId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting Typicode album {albumId}.");

            var json = await _client.GetStringAsync($"/albums/{albumId}");
            var album = JsonSerializer.Deserialize<Album>(json, _options);

            _logger.Verbose($"Typicode album {albumId}{(album == default ? " not" : String.Empty)} found.");

            return album;
        }

        public async Task<IEnumerable<Photo>> GetPhotosAsync(int albumId)
        {
            if (albumId <= 0) throw new ArgumentOutOfRangeException(nameof(albumId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting all Typicode photos for album {albumId}.");

            var json = await _client.GetStringAsync($"/album/{albumId}/photos");
            var photos = JsonSerializer.Deserialize<List<Photo>>(json, _options);
            photos ??= new List<Photo>();

            _logger.Verbose($"Found {photos.Count} Typicode album photos.");

            return photos;
        }
    }
}
