using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankApp_MVC.Models
{
	public partial class Loan
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public Loan()
		{
			this.Transactions = new HashSet<Transaction>();
		}

		public int LoanID { get; set; }
		public Nullable<double> Debt { get; set; }
		public Nullable<double> InterestRate { get; set; }
		public System.DateTime DateCreated { get; set; }
		public int CustomerID { get; set; }

		public virtual Customer Customer { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public virtual ICollection<Transaction> Transactions { get; set; }
	}

	public partial class Loan
	{
		private BankingContext db = new BankingContext();
		public void PayInstallment(double amt)
		{
			if (amt > 0)
			{
				this.Debt -= amt;
				if (this.Debt < amt) this.Debt = 0;
				//Log transaction
				Transaction t = new Transaction()
				{
					TransactionDate = this.DateCreated,
					SignedCredit = amt,
					LoanID = this.LoanID,
					TransactionDesc = "Installment Payed on Loan",
				};
				this.Transactions.Add(t);
				//Add Transaction to database
				db.Transactions.Add(t);
				//Update database with Withdraw information
				var loan = db.Loans.Find(this.LoanID);
				loan.Debt -= amt;
				db.SaveChanges();
			}
			else
			{
				Console.WriteLine("INVALID AMOUNT...");
			}
		}
	}
}