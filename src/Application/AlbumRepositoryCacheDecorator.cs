using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Domain;
using Serilog;

namespace Application
{
    internal class AlbumRepositoryCacheDecorator : IAlbumRepository
    {
        private static readonly ConcurrentDictionary<int, Album> _albums = new ConcurrentDictionary<int, Album>();
        private readonly IAlbumRepository _repository;
        private readonly ILogger _logger;

        public AlbumRepositoryCacheDecorator(IAlbumRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Album> GetAlbumAsync(int albumId)
        {
            if (albumId <= 0) throw new ArgumentOutOfRangeException(nameof(albumId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting album {albumId} from cache.");

            if (_albums.TryGetValue(albumId, out var album)) return album;

            _logger.Verbose($"Album {albumId} not found in cache.");
            album = await _repository.GetAlbumAsync(albumId);

            if (album == default) return default;

            _logger.Verbose($"Caching album {albumId}.");
            _albums.TryAdd(albumId, album);

            return album;
        }
    }
}