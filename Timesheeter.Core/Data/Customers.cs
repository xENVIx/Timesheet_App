using LibLogging;
using LibSqlLite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheeter.Core.Interfaces;

namespace Timesheeter.Core.Data
{
    public class Customers : DataClass<Customers.Customer>
    {

        public const String TableName = nameof(Customer);
        

        //public IReadOnlyCollection<Customer> All { get { return _list; } }
        //public BindingList<Customer> All { get { return _list; } }
        public Customer? this[Guid key] 
        { 
            get
            {
                
                return _list.Unfiltered.Where(x => x.ID == key).FirstOrDefault();
                
            } 
        }



        //private List<Customer> _list = new List<Customer>();
        //private BindingList<Customer> _list = new BindingList<Customer>();


        public class Customer : IData, ISoftDeletable, IAudited
        {
            [PrimaryKey, GridHidden] public Guid ID { get; set; }
            [Required, Unique(IgnoreCase = true)] public string Name { get; set; } = string.Empty;
            [DisplayName("Deleted"), GridHidden] public bool Deleted { get; set; } = false;
            [GridHidden, DisplayName("Created (UTC)")] public DateTime TOC { get; set; }
            [GridHidden, DisplayName("Modified (UTC)")] public DateTime TOM { get; set; }
            [GridHidden, MaxLength(Audit.MaxProcessLength), DisplayName("Modified By")] public string MOP { get; set; } = string.Empty;

        }


        public Customers(SqliteStore dataStore, ILoggerApp? logger) : base(dataStore, logger) 
        {
            /*
            _dataStore.EnsureTable<Customer>();

            foreach (var customer in _dataStore.All<Customer>())
            {
                AddLoaded(customer);
            }
            */
        }

        /// <summary>The customer named <paramref name="name"/> (deleted or not), other than <paramref name="except"/>.</summary>
        public Customer? GetOtherByName(String name, Customer? except)
        {
            return _list.Unfiltered.FirstOrDefault(cust => cust != except &&
                string.Compare(cust.Name, name.Trim(), StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        /// <summary>
        /// <paramref name="renamed"/> was renamed to another customer's name: that customer is kept
        /// (brought back if deleted), project codes move over to it, and <paramref name="renamed"/> is
        /// deleted under its old name.
        /// </summary>
        public bool MergeRenamed(Customer renamed, ProjectCodes projectCodes)
        {
            var target = GetOtherByName(renamed.Name, renamed);
            return target != null && MergeInto(renamed, target, projectCodes, nameof(ProjectCodes.ProjectCode.CustomerID));
        }

        public Customer? GetCustomerByName(String name)
        {

            return _list.Unfiltered.Where(cust => string.Compare(cust.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();

        }


        public bool CustomerExistsByName(String name)
        {
            //if (this[key] != null) return true;
            //return false;

            //if (_list.Exists(cust => String.Compare(cust.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)) return true;
            if (_list.Unfiltered.Where(cust => String.Compare(cust.Name, name.Trim(), StringComparison.InvariantCultureIgnoreCase) == 0).Count() > 0) return true;


            return false;
        }

        /*
        public bool Save(Customer customer)
        {
            if (!IsValid(customer)) return false;

            try
            {
                _dataStore.Insert(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            _list.Add(customer);
            return true;
        }
        */
        


    }
}
