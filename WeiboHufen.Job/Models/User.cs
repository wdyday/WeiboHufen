using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeiboHufen.Job.Models
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime? SignInTime { get; set; }
        public bool SignInSucceeded { get; set; }
    }
}
