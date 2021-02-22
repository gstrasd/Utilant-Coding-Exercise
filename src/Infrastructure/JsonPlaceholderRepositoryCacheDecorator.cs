using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Domain.Typicode;
using Serilog;

namespace Infrastructure
{
    internal class JsonPlaceholderRepositoryCacheDecorator : IJsonPlaceholderRepository
    {
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private static bool _cached;
        private static List<User> _cachedUsers;
        private static List<Album> _cachedAlbums;
        private readonly IJsonPlaceholderRepository _repository;
        private readonly ILogger _logger;

        public JsonPlaceholderRepositoryCacheDecorator(IJsonPlaceholderRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger;

            if (!_cached) PrimeCache(_repository, _logger);
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            _logger.Debug("Getting Typicode users from cache.");
            return _cachedUsers;
        }

        public Task<User> GetUserAsync(int userId)
        {
            return _repository.GetUserAsync(userId);
        }

        public Task<IEnumerable<Post>> GetPostsAsync(int userId)
        {
            return _repository.GetPostsAsync(userId);
        }

        public Task<IEnumerable<Comment>> GetCommentsAsync(int userId)
        {
            return _repository.GetCommentsAsync(userId);
        }

        public async Task<IEnumerable<Album>> GetAlbumsAsync()
        {
            _logger.Debug("Getting Typicode albums from cache.");

            return _cachedAlbums;
        }

        public Task<Album> GetAlbumAsync(int albumId)
        {
            return _repository.GetAlbumAsync(albumId);
        }

        public Task<IEnumerable<Photo>> GetPhotosAsync(int albumId)
        {
            return _repository.GetPhotosAsync(albumId);
        }

        private static void PrimeCache(IJsonPlaceholderRepository repository, ILogger logger)
        {
            _lock.Wait();

            try
            {
                if (!_cached)
                {
                    Parallel.Invoke(
                        () =>
                        {
                            logger.Verbose("Priming Typicode users cache.");
                            _cachedUsers = repository.GetUsersAsync().GetAwaiter().GetResult().ToList();
                        },
                        () =>
                        {
                            logger.Verbose("Priming Typicode albums cache.");
                            _cachedAlbums = repository.GetAlbumsAsync().GetAwaiter().GetResult().ToList();
                        });
                    _cached = true;
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}