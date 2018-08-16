using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BankApp_MVC.Models
{
	public partial class Account
	{
		private BankingContext db = new BankingContext();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public Account()
		{
			this.Transactions = new HashSet<Transaction>();
		}

		[Display(Name = "Account ID")]
		public int AccountID { get; set; }
		[Display(Name = "Balance")]
		public Nullable<double> Amount { get; set; }
		public int CustomerID { get; set; }
		[Display(Name="Account Type")]
		public string Discriminator { get; set; }

		public virtual Customer Customer { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public virtual ICollection<Transaction> Transactions { get; set; }
	}
	
	public partial class Account
	{
		public void Withdraw(double amt)
		{
			if (amt > 0)
			{
				//Decrease account by this amount
				this.Amount -= amt;
				//Log this transaction
				LogTransaction(-amt, "Withdraw: -$" + amt);
				//Update database with Withdraw information
				var accnt = db.Accounts.Find(this.AccountID);
				accnt.Amount -= amt;
				db.SaveChanges();

			}
			else
			{
				Console.WriteLine("INVALID INPUT...");
			}
		}
		public void Deposit(double amt)
		{
			if (amt > 0)
			{
				//Increase Amount
				this.Amount += amt;
				//Log this Transaction
				LogTransaction(amt, "Deposit: $" + amt);
				//Update Database with Deposit Information
				var accnt = db.Accounts.Find(this.AccountID);
				accnt.Amount += amt;
				db.SaveChanges();

			}
			else
			{
				Console.WriteLine("INVALID INPUT...");
			}
		}
		public bool IsBroke() => this.Amount <= 0 ? true : false;
		private bool IsNegative() => this.Amount < 0 ? true : false;
		public void TransferTo(double amt, Account to)
		{
			if (amt > 0)
			{
				if (!this.IsBroke() && this.Amount > amt)
				{
					//Take amount from this account and add to desired account
					this.Amount -= amt;
					LogTransaction(-amt, this, "Transfer from Account No. " + this.AccountID + " to Account No. " + to.AccountID);
					to.Amount += amt;
					//Log this transaction
					LogTransaction(amt, to, "Transfer from Account No. " + this.AccountID + " to Account No. " + to.AccountID);
					//Add Transaction to database
					var accnt1 = db.Accounts.Find(this.AccountID);
					accnt1.Amount -= amt;
					var accnt2 = db.Accounts.Find(to.AccountID);
					accnt2.Amount += amt;
					db.SaveChanges();

				}
				else
				{
					Console.WriteLine("INSUFFICIENT FUNDS...");
				}
			}
			else
			{
				Console.WriteLine("INVALID AMOUNT ENTERED...");
			}
		}
		public void DisplayAccountInfo()
		{
			Console.WriteLine("---------------------------------------------");
			Console.WriteLine($"{Customer.LastName}, {Customer.FirstName}");
			Console.WriteLine("---------------------------------------------");
			//Customer Account Info
			foreach (Account item in Customer.Accounts)
			{
				Console.WriteLine($"Account Number: {item.AccountID} ({item.Discriminator})");
				Console.WriteLine($"Current Balance: ${item.Amount}");
				int ix = 1;
				foreach (Transaction t in item.Transactions)
				{
					//Console.WriteLine("I am in here");
					Console.WriteLine($"    Transaction No. " + ix);
					Console.WriteLine($"    Transaction Date: {Convert.ToString(t.TransactionDate)}");
					if (t.SignedCredit > 0) Console.WriteLine($"    Credit/Debit:  ${t.SignedCredit}");
					if (t.SignedCredit < 0) Console.WriteLine($"    Credit/Debit: -${-t.SignedCredit}");
					Console.WriteLine($"    Transaction Desc: " + t.TransactionDesc);
					Console.WriteLine();
					ix++;
				}
				Console.WriteLine();
			}

			if (Customer.Loans.Count > 0)
			{
				//Loan Information
				foreach (Loan item in Customer.Loans)
				{
					Console.WriteLine($"Loan Identification Number: {item.LoanID}");
					if (item.Debt > 0) Console.WriteLine($"Current Debt: -${item.Debt}");
					if (item.Debt == 0) Console.WriteLine($"Current Debt: ${item.Debt}");
					int ix = 1;
					foreach (Transaction t in item.Transactions)
					{
						Console.WriteLine($"    Transaction No. " + ix);
						Console.WriteLine($"    Transaction Date: {Convert.ToString(t.TransactionDate)}");
						if (t.SignedCredit > 0) Console.WriteLine($"    Credit/Debit: ${t.SignedCredit}");
						if (t.SignedCredit < 0) Console.WriteLine($"    Credit/Debit: -${-t.SignedCredit}");
						Console.WriteLine();
						ix++;
					}
					Console.WriteLine();
				}
			}
		}
		public void PayOverdraft(double amt)
		{
			const double INTEREST_RATE = 0.01;

			if (this.IsNegative())
			{
				if (amt > 0)
				{
					double fee = (double)-this.Amount * INTEREST_RATE;
					double payment = amt > (double)-this.Amount + fee ? (double)-this.Amount : amt - fee;
					this.Amount += payment;
					LogTransaction(-payment, this, "Installment Paid: -$" + payment);
					//Update Database with Payment Information
					var accnt = db.Accounts.Find(this.AccountID);
					accnt.Amount += amt;
					db.SaveChanges();
				}
				else
				{
					Console.WriteLine("INVALID INPUT...");
				}
			}
			else
			{
				Console.WriteLine("OVERDRAFT FEE DOES NOT APPLY...");
			}
		}
		protected void LogTransaction(double val, string desc)
		{
			//Create Transaction
			Transaction t = new Transaction()
			{
				AccountID = this.AccountID,
				TransactionDate = DateTime.Now,
				SignedCredit = val,
				TransactionDesc = desc,
			};
			//Log transaction
			this.Transactions.Add(t);
			//Add Transaction to database
			db.Transactions.Add(t);
			db.SaveChanges();


		}
		protected void LogTransaction(double val, Account acnt, string desc)
		{
			//Create Transaction
			Transaction t = new Transaction()
			{
				AccountID = acnt.AccountID,
				TransactionDate = DateTime.Now,
				SignedCredit = val,
				TransactionDesc = desc,
			};
			//Log transaction
			this.Transactions.Add(t);
			//Add Transaction to database
			db.Transactions.Add(t);
			db.SaveChanges();
		}
	}

}