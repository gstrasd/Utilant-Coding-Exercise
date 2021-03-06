﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Domain
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(int userId);
        Task<List<Post>> GetBlogAsync(int userId);
    }
}