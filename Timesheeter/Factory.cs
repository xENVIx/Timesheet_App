using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheeter
{
    internal class Factory : Lib.IFactory
    {


        public Object? GetData(Type dataType)
        {

            if (dataType == typeof(Data.Customers))
            {
                return _customers;
            }


            return null;


        }

        private LibSqlLite.SqliteStore _store;
        private Data.Customers _customers;


        internal Factory()
        {


            String dbFile = Path.Combine(AppContext.BaseDirectory, "Timesheeter.db");
            _store = new LibSqlLite.SqliteStore(dbFile);
            _customers = new Data.Customers(_store);

        }

        internal void Run()
        {

            Console.WriteLine($"Customers");
            foreach (var customer in _customers.All)
            {
                Console.WriteLine(customer.Name);
            }

            if (!_customers.CustomerExistsByName("New Customer"))
            {
                _customers.Save(new Data.Customers.Customer()
                {
                    Name = "New Customer"
                });
            }

            Console.WriteLine($"Customers");
            foreach (var customer in _customers.All)
            {
                Console.WriteLine(customer.Name);
            }

            Application.Run(new Form1());
        }


    }
}
