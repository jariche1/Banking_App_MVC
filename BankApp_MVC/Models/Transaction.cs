using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BankApp_MVC.Models
{
	public partial class Transaction
	{
		public int TransactionID { get; set; }
		[Display(Name = "Date of Transaction")]
		public System.DateTime TransactionDate { get; set; }
		[Display(Name = "Credit/Debit")]
		public Nullable<double> SignedCredit { get; set; }
		public Nullable<int> AccountID { get; set; }
		public Nullable<int> LoanID { get; set; }
		[Display(Name = "Description")]
		public string TransactionDesc { get; set; }

		public virtual Account Account { get; set; }
		public virtual Loan Loan { get; set; }
	}
}