using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
                private MyContext dbContext;
     
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Create")]
        public IActionResult Create(User user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Add(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("userId", user.UserId);
                HttpContext.Session.SetString("userName", user.FirstName);
                return RedirectToAction("Account", new {userId = user.UserId});
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.LoginEmail);
                if(userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.LoginPassword);
                if(result == 0)
                {
                    ModelState.AddModelError("LoginPassword", "Username/password combination incorrect");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("userId", userInDb.UserId);
                HttpContext.Session.SetString("userName", userInDb.FirstName);
                return RedirectToAction("Account", new {userId = userInDb.UserId});
            }
            else
            {
                return View("Index");
            }
        }
        [HttpGet("/Account/{UserId}")]
        public IActionResult Account(int userId)
        {
            if(HttpContext.Session.GetString("userName") == null) 
            {
                return RedirectToAction("Index");
            }
            else
            {
                int? UserInSession = HttpContext.Session.GetInt32("userId");
                User LoggedIn = dbContext.Users.FirstOrDefault(u => u.UserId == (int)UserInSession);
                if(userId != LoggedIn.UserId)
                {
                    return RedirectToAction("Failure");
                }
                NewTransactionViewModel viewMod = new NewTransactionViewModel();
                viewMod.AccountUser = dbContext.Users.Include(user => user.Transactions).FirstOrDefault(u => u.UserId == LoggedIn.UserId);
                float SumOfTransactions = 0;
                foreach(Transaction t in viewMod.AccountUser.Transactions)
                {
                    SumOfTransactions += t.Amount;
                }
                ViewBag.Balance = SumOfTransactions;
                return View(viewMod);
            }
        }

        [HttpPost("Transaction")]
        public IActionResult Transaction(NewTransactionViewModel Deposit)
        {
            if(ModelState.IsValid)
            {
                User person = dbContext.Users.Include(t => t.Transactions).FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("userId"));
                float SumOfTransactions = 0;
                foreach(Transaction t in person.Transactions)
                {
                    SumOfTransactions += t.Amount;
                }
                if(SumOfTransactions + Deposit.NewTransaction.Amount < 0)
                {
                    ModelState.AddModelError("NewTransaction.Amount", "Insufficient funds");
                    int? UserInSession = HttpContext.Session.GetInt32("userId");
                    User LoggedIn = dbContext.Users.FirstOrDefault(u => u.UserId == (int)UserInSession);
                    NewTransactionViewModel viewMod = new NewTransactionViewModel();
                    viewMod.AccountUser = dbContext.Users.Include(user => user.Transactions).FirstOrDefault(u => u.UserId == LoggedIn.UserId);
                    return View("Account", viewMod);
                }
                int? InSession = HttpContext.Session.GetInt32("userId");
                User ThisUser = dbContext.Users.FirstOrDefault(u => u.UserId == (int)InSession);
                Deposit.NewTransaction.UserId = ThisUser.UserId;
                dbContext.Transactions.Add(Deposit.NewTransaction);
                dbContext.SaveChanges();
                return RedirectToAction("Account", new{userId = (int)ThisUser.UserId});
            }
            else
            {
                int? UserInSession = HttpContext.Session.GetInt32("userId");
                User LoggedIn = dbContext.Users.FirstOrDefault(u => u.UserId == (int)UserInSession);
                NewTransactionViewModel viewMod = new NewTransactionViewModel();
                viewMod.AccountUser = dbContext.Users.Include(user => user.Transactions).FirstOrDefault(u => u.UserId == LoggedIn.UserId);
                return View("Account", viewMod);
            }
        }
        [HttpGet("Failure")]
        public IActionResult Failure()
        {
            if(HttpContext.Session.GetString("userName") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        [HttpGet("LogOut")]
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
