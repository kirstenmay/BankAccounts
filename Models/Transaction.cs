using System;
using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId {get;set;}
        public float Amount {get;set;}
        public DateTime Created_at {get;set;}
        public DateTime Updated_at {get;set;}
        public int UserId {get;set;}
        public User Creator {get;set;}
    }
}
