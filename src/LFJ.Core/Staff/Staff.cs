using Abp.Domain.Entities;
using LFJ.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LFJ.Staff
{
    public class Staff : Entity<int>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Status { get; set; }
    }
}
