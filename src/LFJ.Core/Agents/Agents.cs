using Abp.Domain.Entities;
using LFJ.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LFJ.Agents
{
    public class Agents : Entity<int>
    {
        public User User { get; set; }

        [ForeignKey("User")]
        public long UserId { get; set; }
        public int PMCode { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
    }
}
