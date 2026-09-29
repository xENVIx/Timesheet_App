using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using LibSqlLite;

namespace Timesheeter.Data
{
    public class Customers : DataClass<Customers.Customer>
    {
        //public IReadOnlyCollection<Customer> All { get { return _list; } }
        //public BindingList<Customer> All { get { return _list; } }
        public Customer? this[long key] 
        { 
            get
            {
                
                return _list.Where(x => x.ID == key).FirstOrDefault();
                
            } 
        }



        //private List<Customer> _list = new List<Customer>();
        //private BindingList<Customer> _list = new BindingList<Customer>();


        public class Customer
        {
            [PrimaryKey, Browsable(false)] public long ID { get; set; }
            [Unique(IgnoreCase = true)] public string Name { get; set; } = string.Empty;

        }


        public Customers(SqliteStore dataStore) : base(dataStore) 
        {
            _list = new BindingList<Customer>();

            _dataStore.EnsureTable<Customer>();

            foreach (var customer in _dataStore.All<Customer>())
            {
                _list.Add(customer);
            }
        }

        public Customer? GetCustomerByName(String name)
        {

            return _list.Where(cust => string.Compare(cust.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();

        }


        public bool CustomerExistsByName(String name)
        {
            //if (this[key] != null) return true;
            //return false;

            //if (_list.Exists(cust => String.Compare(cust.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)) return true;
            if (_list.Where(cust => String.Compare(cust.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0).Count() > 0) return true;


            return false;
        }

        public bool Save(Customer customer)
        {
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
        


    }
}
