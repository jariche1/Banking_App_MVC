using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BankApp_MVC.Models
{
	public class BankingContext : DbContext
	{
		public BankingContext() : base("name=localdatabase")
		{
		}
		
		public virtual DbSet<Account> Accounts { get; set; }
		public virtual DbSet<Customer> Customers { get; set; }
		public virtual DbSet<Transaction> Transactions { get; set; }
		public virtual DbSet<Loan> Loans { get; set; }
	}
}