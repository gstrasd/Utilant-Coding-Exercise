using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Application.Domain;
using Geolocation;
using Serilog;
using Typicode = Infrastructure.Domain.Typicode;

namespace Application
{
    internal class UserRepository : IUserRepository
    {
        private readonly Typicode.IJsonPlaceholderRepository _repository;
        private readonly ILogger _logger;

        public UserRepository(Typicode.IJsonPlaceholderRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting user {userId}.");

            var user = await _repository.GetUserAsync(userId);
            if (user == default) return default;

            Uri.TryCreate(user.Website.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? user.Website : String.Concat("http://", user.Website), UriKind.RelativeOrAbsolute, out var website);
            MailAddress.TryCreate(user.Email, out var email);
            var result = new User
            {
                UserId = user.Id,
                Name = user.Name,
                UserName = user.UserName,
                Email = email,
                Address = user.Address != default ? new Address
                {
                    Street = user.Address.Street,
                    Suite = user.Address.Suite,
                    City = user.Address.City,
                    ZipCode = user.Address.ZipCode,
                    GeoLocation = user.Address.Geo != default ? new Coordinate(user.Address.Geo.Lat, user.Address.Geo.Lng) : default
                } : default,
                Phone = user.Phone,
                Website = website,
                Company = new Company
                {
                    Name = user.Company?.Name,
                    CatchPhrase = user.Company?.CatchPhrase,
                    Bs = user.Company?.Bs
                }
            };

            return result;
        }

        public async Task<List<Post>> GetBlogAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting blog for user {userId}.");

            IEnumerable<Typicode.Post> posts = default;
            IEnumerable<Typicode.Comment> comments = default;

            await Task.WhenAll(
                Task.Run(async () => posts = await _repository.GetPostsAsync(userId)),
                Task.Run(async () => comments = await _repository.GetCommentsAsync(userId)));

            var result = (
                from c in comments
                group c by c.PostId
                into g
                join p in posts on g.Key equals p.Id
                select new Post
                {
                    Title = p.Title,
                    Body = p.Body,
                    Comments = g.Select(pc =>
                    {
                        MailAddress.TryCreate(pc.Email, out var email);
                        return new Comment
                        {
                            Name = pc.Name,
                            Email = email,
                            Body = pc.Body
                        };
                    }).ToList()
                }).ToList();

            return result;
        }
    }
}
