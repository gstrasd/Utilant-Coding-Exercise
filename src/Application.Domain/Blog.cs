using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Domain
{
    public class Blog
    {
        public int UserId { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public List<Comment> Comments { get; set; }
    }

    public class Comment
    {
        public string Name { get; set; }
        public MailAddress Email { get; set; }
        public string Content { get; set; }
    }
}