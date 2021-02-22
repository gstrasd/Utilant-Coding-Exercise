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
    internal class UserRepositoryCacheDecorator : IUserRepository
    {
        private static readonly ConcurrentDictionary<int, User> _users = new ConcurrentDictionary<int, User>();
        private static readonly ConcurrentDictionary<int, List<Post>> _blog = new ConcurrentDictionary<int, List<Post>>();
        private readonly IUserRepository _repository;
        private readonly ILogger _logger;

        public UserRepositoryCacheDecorator(IUserRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting user {userId} from cache.");

            if (_users.TryGetValue(userId, out var user)) return user;

            _logger.Verbose($"User {userId} not found in cache.");
            user = await _repository.GetUserAsync(userId);

            if (user == default) return default;

            _logger.Verbose($"Caching user {userId}.");
            _users.TryAdd(userId, user);

            return user;
        }

        public async Task<List<Post>> GetBlogAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "Argument must be a positive, non-zero value.");

            _logger.Debug($"Getting blog for user {userId} from cache.");

            if (_blog.TryGetValue(userId, out var blog)) return blog;

            _logger.Verbose($"Blog for user {userId} not found in cache.");
            blog = await _repository.GetBlogAsync(userId);

            if (blog == default) return default;

            _logger.Verbose($"Caching blog for user {userId}.");
            _blog.TryAdd(userId, blog);

            return blog;
        }
    }
}