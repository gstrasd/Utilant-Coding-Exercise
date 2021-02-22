using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Domain;
using Serilog;
using Typicode = Infrastructure.Domain.Typicode;

namespace Application
{
    internal class AlbumRepository : IAlbumRepository
    {
        private readonly Typicode.IJsonPlaceholderRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public AlbumRepository(Typicode.IJsonPlaceholderRepository repository, IUserRepository userRepository, ILogger logger)
        {
            _repository = repository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Album> GetAlbumAsync(int albumId)
        {
            if (albumId <= 0) throw new ArgumentOutOfRangeException(nameof(albumId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting album {albumId}.");

            Typicode.Album typicodeAlbum = default;
            User user = default;
            List<Typicode.Photo> typicodePhotos = default;

            await Task.WhenAll(
                Task.Run(async () =>
                {
                    typicodeAlbum = await _repository.GetAlbumAsync(albumId);
                    user = typicodeAlbum != default ? await _userRepository.GetUserAsync(typicodeAlbum.UserId) : default;
                }),
                Task.Run(async () => typicodePhotos = (await _repository.GetPhotosAsync(albumId)).ToList()));

            if (typicodeAlbum == default) return default;

            var firstPhoto = typicodePhotos?.FirstOrDefault();
            var album = new Album
            {
                AlbumId = typicodeAlbum.Id,
                Title = typicodeAlbum.Title,
                Thumbnail = firstPhoto != default ? new Uri(firstPhoto.ThumbnailUrl) : default,
                Photos = typicodePhotos?.Select(p => new Thumbnail { Title = p.Title, Url = new Uri(p.ThumbnailUrl) }).ToList(),
                User = user
            };

            return album;
        }
    }
}
