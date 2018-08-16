using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankApp_MVC.Models
{
	public partial class Customer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public Customer()
		{
			this.Accounts = new HashSet<Account>();
			this.Loans = new HashSet<Loan>();
		}

		public int CustomerID { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }
		public string UserPassword { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public virtual ICollection<Account> Accounts { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public virtual ICollection<Loan> Loans { get; set; }
	}
	public partial class Customer
	{

		public void CloseAccount(Account a)
		{
			try
			{
				//Delete Account from CustomerAccountList by Account Number
				this.Accounts.Remove(a);
				Console.WriteLine("ACCOUNT NUMBER " + a.AccountID + " HAS BEEN REMOVED...");
				//Delete Information from Database

			}
			catch (Exception ex)
			{
				Console.WriteLine("ACCOUNT NUMBER " + a.AccountID + " WAS NOT SUCCESSFULLY REMOVED...");
				Console.WriteLine(ex.Message);
			}
		}
		public void OpenAccount(double amt, int customerid, string TYPE)
		{
			Account newacc = new Account() { Discriminator = TYPE,};
		
			if (amt > 0)
			{
				//Connect external customer object to this account's customer object
				this.CustomerID = customerid;
				newacc.Amount = amt;
				//Add Account to Customer Account
				this.Accounts.Add(newacc);
				//Add Transaction to database

			}
			else
			{
				Console.WriteLine("INVALID INPUT...");
			}
		}
		public void Register(string username, string password)
		{
			if (username != null && password != null)
			{
				//Store Credentials in Customers Table (CustomerID,FirstName,LastName,Usernam,Password)

				Console.WriteLine("Registered!");
			}
			else
			{
				Console.WriteLine("PROVIDE VALID VALUES FOR USERNAME AND/OR PASSWORD...");
			}

		}
		public void Login(string username, string password)
		{
			//Check if User exists in Customers Table
			Console.WriteLine("Logged In!");
		}
		public void LogOut()
		{
			Console.WriteLine("Logged Out!");
		}
	}
}