using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BankApp_MVC.Models;

namespace BankApp_MVC.Controllers
{
    public class TransactionsController : Controller
    {
        private BankingContext db = new BankingContext();

        // GET: Transactions
        public ActionResult Index()
        {
			//Check previous page
			// --if user was on accounts page show accounts transaction)
			// --if user was on loans page show loans transaction)
			if (System.Web.HttpContext.Current.Request.UrlReferrer.LocalPath == "/Accounts")
			{
				int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;

				//Get account ids for current customer
				List<int> account_ids = db.Accounts.Include(a => a.Customer).Where(a => a.CustomerID == c_id).Select(a => a.AccountID).ToList();
				//Get transactions for all account ids
				var transactions = db.Transactions.Include(t => t.Account).Where(t => account_ids.Contains((int)t.AccountID));
				
				return View(transactions.ToList());
			}
			else if (System.Web.HttpContext.Current.Request.UrlReferrer.LocalPath == "/Loans")
			{
				int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
				//Get loan ids for current customer
				List<int> loans_ids = db.Loans.Include(a => a.Customer).Where(a => a.CustomerID == c_id).Select(a => a.LoanID).ToList();
				//Get transactions for all loan ids
				var transactions = db.Transactions.Include(t => t.Account).Where(t => loans_ids.Contains((int)t.LoanID));
				return View(transactions.ToList());
			}
			int id = (int)Session["AccountLoan_id"];
			return View();
        }

		public ActionResult IndexById(int aol_id)
		{
			if (System.Web.HttpContext.Current.Request.UrlReferrer.LocalPath == "/Accounts")
			{
				Session["type"] = "Accounts";
				int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
				//Get transactions for all account ids
				var transactions = db.Transactions.Include(t => t.Account).Where(t => t.AccountID == aol_id);
				ViewBag.AccountBalance = db.Accounts.Single(a => a.AccountID == aol_id && a.CustomerID == c_id).Amount;
				ViewBag.AccountType = db.Accounts.Single(a => a.AccountID == aol_id && a.CustomerID == c_id).Discriminator;
				double b = ViewBag.AccountBalance;
				if (b < 0)
				{
					ViewBag.IsNegative = true;
					ViewBag.AccountBalance = -b;
				}
				return View("Index", transactions.ToList());
			}
			else if (System.Web.HttpContext.Current.Request.UrlReferrer.LocalPath == "/Loans")
			{
				Session["type"] = "Loans";
				int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
				//Get transactions for all loan ids
				var transactions = db.Transactions.Include(t => t.Loan).Where(t => t.LoanID == aol_id);
				ViewBag.LoanDebt = db.Loans.Single(a => a.LoanID == aol_id && a.CustomerID == c_id).Debt;
				return View("Index", transactions.ToList());
			}
			else if (Session["AccountLoan_id"] != null)
			{
				int al_id = (int)Session["AccountLoan_id"];
				if ((string)Session["type"] == "Accounts")
				{
					Debug.WriteLine(al_id);
					int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
					var transactions = db.Transactions.Include(t => t.Account).Where(t => t.AccountID == al_id);
					ViewBag.actid = db.Transactions.OrderByDescending(t => t.TransactionID).First().AccountID;
					ViewBag.AccountBalance = db.Accounts.Single(a => a.AccountID == aol_id && a.CustomerID == c_id).Amount;
					ViewBag.AccountType = db.Accounts.Single(a => a.AccountID == aol_id && a.CustomerID == c_id).Discriminator;
					double b = ViewBag.AccountBalance;
					if (b < 0)
					{
						ViewBag.IsNegative = true;
						ViewBag.AccountBalance = -b;
					}
					return View("Index", transactions.ToList());
				}
				if ((string)Session["type"] == "Loans")
				{
					Debug.WriteLine(al_id);
					int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
					var transactions = db.Transactions.Include(t => t.Loan).Where(t => t.LoanID == al_id);
					ViewBag.loanid = db.Transactions.OrderByDescending(t => t.TransactionID).First().LoanID;
					ViewBag.LoanDebt = db.Loans.Single(a => a.LoanID == aol_id && a.CustomerID == c_id).Debt;
					return View("Index", transactions.ToList());
				}				
			}
			return View("Index");
		}

		public ActionResult Withdraw()
		{
			if (Session["AccountLoan_id"] != null)
			{
				Session["AccountLoan_id"] = Int32.Parse(HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["aol_id"]);
				Debug.WriteLine("From GET Withdraw: " + Session["AccountLoan_id"]);
			}
			return View();
		}

		[HttpPost]
		public ActionResult Withdraw([Bind(Include = "Amount")] Account act)
		{
			if (ModelState.IsValid)
			{
				//Load Current Customer's account data into object
				int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
				int acc_id = (int)Session["AccountLoan_id"];
				Account account = db.Accounts.Single(c => c.CustomerID == c_id && c.AccountID == acc_id);
				//Perform Action
				account.Withdraw((double)act.Amount);
				Debug.WriteLine("From POST Withdraw: " + Session["AccountLoan_id"]);
				//Redirect back to Transaction page
				return RedirectToAction("IndexById", new {aol_id = acc_id });
			}
			return View();
		}

		public ActionResult Deposit()
		{
			if (Session["AccountLoan_id"] != null)
			{
				Session["AccountLoan_id"] = Int32.Parse(HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["aol_id"]);
				Debug.WriteLine("From GET Deposit: " + Session["AccountLoan_id"]);
			}
			return View();
		}

		[HttpPost]
		public ActionResult Deposit([Bind(Include = "Amount")] Account act)
		{
			if (ModelState.IsValid)
			{
				//Load Current Customer's account data into object
				int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
				int acc_id = (int)Session["AccountLoan_id"];
				Account account = db.Accounts.Single(c => c.CustomerID == c_id && c.AccountID == acc_id);
				//Perform Action
				account.Deposit((double)act.Amount);
				Debug.WriteLine("From POST Deposit: " + Session["AccountLoan_id"]);
				//Redirect back to Transaction page
				return RedirectToAction("IndexById", new { aol_id = acc_id });
			}

			return View();

		}

		public ActionResult TransferTo()
		{
			if (Session["AccountLoan_id"] != null)
			{
				Session["AccountLoan_id"] = Int32.Parse(HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["aol_id"]);
				Debug.WriteLine("From GET TransferTo: " + Session["AccountLoan_id"]);
			}
			//Pass account object to view 
			int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
			List<int> acts = db.Accounts.Where(a => a.CustomerID == c_id).Select(a => a.AccountID).ToList();
			Debug.WriteLine("From GET Transfer To: " + Session["AccountLoan_id"]);
			//ViewBag.AccountID = acts;
			return View();
		}
		
		[HttpPost]
		public ActionResult TransferTo([Bind(Include = "Amount, AccountID")] Account act)
		//public ActionResult TransferTo(double? Amount,int? AccountID)
		{
			if (ModelState.IsValid)
			{
				//Load Current Customer's account data into object
				int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
				int acc_id = (int)Session["AccountLoan_id"];

				Account from = db.Accounts.Single(c => c.CustomerID == c_id && c.AccountID == acc_id);
				Account to = db.Accounts.Single(c => c.CustomerID == c_id && c.AccountID == act.AccountID);
				//Perform Action
				from.TransferTo((double)act.Amount, to);
				Debug.WriteLine("From POST Transfer To: " + Session["AccountLoan_id"]);
				//Redirect back to Transaction page
				return RedirectToAction("IndexById", new { aol_id = acc_id });
			}
			return View();
		}

		public ActionResult PayOverdraft()
		{
			if (Session["AccountLoan_id"] != null)
			{
				Session["AccountLoan_id"] = Int32.Parse(HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["aol_id"]);
			}
			return View();
		}

		[HttpPost]
		public ActionResult PayOverdraft([Bind(Include = "Amount")] Account act)
		{
			if (ModelState.IsValid)
			{
				//Load Current Customer's account data into object
				int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
				int acc_id = (int)Session["AccountLoan_id"];
				Account account = db.Accounts.Single(c => c.CustomerID == c_id && c.AccountID == acc_id);
				//Perform Action
				account.PayOverdraft((double)act.Amount);
				ViewBag.AccountBalance = db.Accounts.Single(a => a.AccountID == acc_id && a.CustomerID == c_id).Amount;
				ViewBag.AccountType = db.Accounts.Single(a => a.AccountID == acc_id && a.CustomerID == c_id).Discriminator;
				//Redirect back to Transaction page
				return RedirectToAction("IndexById", new { aol_id = acc_id });
			}
			return View();
		}

		public ActionResult PayInstallment()
		{
			if (Session["AccountLoan_id"] != null)
			{
				Session["AccountLoan_id"] = Int32.Parse(HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["aol_id"]);
			}
			return View();
		}

		[HttpPost]
		public ActionResult PayInstallment([Bind(Include = "Debt")] Loan l)
		{
			if (ModelState.IsValid)
			{
				//Load Current Customer's account data into object
				int c_id = db.Customers.Single(c => c.UserName == User.Identity.Name).CustomerID;
				int l_id = (int)Session["AccountLoan_id"];
				Loan loan = db.Loans.Single(c => c.CustomerID == c_id && c.LoanID == l_id);
				//Perform Action
				loan.PayInstallment((double)l.Debt);
				ViewBag.loanid = db.Transactions.OrderByDescending(t => t.TransactionID).First().LoanID;
				ViewBag.LoanDebt = db.Loans.Single(a => a.LoanID == l_id && a.CustomerID == c_id).Debt;
				//Redirect back to Transaction page
				return RedirectToAction("IndexById", new { aol_id = l_id });
			}

			return View();

		}

		// GET: Transactions/Details/5
		public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            ViewBag.AccountID = new SelectList(db.Accounts, "AccountID", "Discriminator");
            ViewBag.LoanID = new SelectList(db.Loans, "LoanID", "LoanID");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TransactionID,TransactionDate,SignedCredit,AccountID,LoanID,TransactionDesc")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AccountID = new SelectList(db.Accounts, "AccountID", "Discriminator", transaction.AccountID);
            ViewBag.LoanID = new SelectList(db.Loans, "LoanID", "LoanID", transaction.LoanID);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.AccountID = new SelectList(db.Accounts, "AccountID", "Discriminator", transaction.AccountID);
            ViewBag.LoanID = new SelectList(db.Loans, "LoanID", "LoanID", transaction.LoanID);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TransactionID,TransactionDate,SignedCredit,AccountID,LoanID,TransactionDesc")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AccountID = new SelectList(db.Accounts, "AccountID", "Discriminator", transaction.AccountID);
            ViewBag.LoanID = new SelectList(db.Loans, "LoanID", "LoanID", transaction.LoanID);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
