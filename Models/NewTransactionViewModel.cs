using System.Collections.Generic;

namespace BankAccounts.Models
{
    public class NewTransactionViewModel
    {
        public Transaction NewTransaction {get;set;}
        public User AccountUser {get;set;}
    }
}