using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Application.Domain;
using Infrastructure.Domain.Typicode;
using Serilog;
using Album = Application.Domain.Album;
using Typicode = Infrastructure.Domain.Typicode;

namespace Application
{
    internal class AlbumSearchService : IAlbumSearchService
    {
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private static bool _cached;
        private static List<(int AlbumId, string Title, int UserId, string Name)> _searchContext;
        private readonly IJsonPlaceholderRepository _repository;
        private readonly IAlbumRepository _albumRepository;
        private readonly ILogger _logger;

        public AlbumSearchService(IJsonPlaceholderRepository repository, IAlbumRepository albumRepository, ILogger logger)
        {
            _repository = repository;
            _albumRepository = albumRepository;
            _logger = logger;

            if (!_cached) CacheSearchContext(_repository, _logger);
        }

        public async Task<(List<Album> Albums, int PageCount)> SearchAsync(string value, int page, int pageSize)
        {
            if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Argument must be a positive, non-zero value.");
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Searching for \"{value}\".");

            var search = (
                    from c in _searchContext
                    where 
                        String.IsNullOrWhiteSpace(value) ||
                        value.Length <= 2 && (c.Title.StartsWith(value, StringComparison.OrdinalIgnoreCase) || c.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase)) ||
                        value.Length > 2 && (c.Title.Contains(value, StringComparison.OrdinalIgnoreCase) || c.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
                    orderby c.Title
                    select c.AlbumId)
                .ToList();

            var results = await search
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToAsyncEnumerable()
                .SelectAwait(async albumId => await _albumRepository.GetAlbumAsync(albumId))
                .ToListAsync();

            var pageCount = (int) Math.Ceiling(search.Count / (double) pageSize);

            return (results, pageCount);
        }

        private static void CacheSearchContext(IJsonPlaceholderRepository repository, ILogger logger)
        {
            _lock.Wait();

            try
            {
                if (!_cached)
                {
                    logger.Verbose("Caching search context.");

                    IEnumerable<Typicode.Album> albums = default;
                    IEnumerable<Typicode.User> users = default;

                    Parallel.Invoke(
                        () => albums = repository.GetAlbumsAsync().GetAwaiter().GetResult(),
                        () => users = repository.GetUsersAsync().GetAwaiter().GetResult());

                    // Obtain a list of all albums and their associated album title and user's name to support searching.
                    _searchContext = (
                            from a in albums
                            join u in users on a.UserId equals u.Id
                            select (a.Id, a.Title, u.Id, u.Name))
                        .ToList();
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
