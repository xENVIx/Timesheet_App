using System;
using System.Collections.Generic;
using System.Text;

using LibSqlLite;

namespace Timesheeter.Data
{
    public class Customers
    {
        public IReadOnlyCollection<Customer> All { get { return _list; } }
        public Customer? this[long key] 
        { 
            get
            {
                
                return _list.Where(x => x.ID == key).FirstOrDefault();
                
            } 
        }


        public bool CustomerExistsByName(String name)
        {
            //if (this[key] != null) return true;
            //return false;

            if (_list.Exists(cust => String.Compare(cust.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)) return true;

            return false;
        }

        private LibSqlLite.SqliteStore _dataStore;
        private List<Customer> _list = new List<Customer>();


        public class Customer
        {
            [PrimaryKey] public long ID { get; set; }
            [Unique(IgnoreCase = true)] public string Name { get; set; } = string.Empty;

        }


        public Customers(SqliteStore dataStore)
        {
            _dataStore = dataStore;
            _dataStore.EnsureTable<Customer>();

            foreach (var customer in _dataStore.All<Customer>())
            {
                _list.Add(customer);
            }
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
