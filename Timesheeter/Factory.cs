using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheeter
{
    internal class Factory : Lib.IFactory
    {


        public Object? GetData(Type dataType)
        {

            if (dataType.GetType() == typeof(Data.Customers))
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
                Console.WriteLine(customer.CustomerName);
            }

            _customers.Save(new Data.Customers.Customer()
            {
                CustomerName = "New Customer"
            });

            Console.WriteLine($"Customers");
            foreach (var customer in _customers.All)
            {
                Console.WriteLine(customer.CustomerName);
            }

            Application.Run(new Form1());
        }


    }
}
