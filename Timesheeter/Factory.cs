using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Data;

namespace Timesheeter
{
    internal class Factory : Lib.IFactory
    {



        private LibSqlLite.SqliteStore _store;
        private Data.Customers _customers;
        private readonly System.Windows.Forms.Timer _timer;


        internal Factory()
        {


            String dbFile = Path.Combine(AppContext.BaseDirectory, "Timesheeter.db");
            _store = new LibSqlLite.SqliteStore(dbFile);
            _customers = new Data.Customers(_store);

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 10000;
            _timer.Tick += _timer_Tick;
            
            _timer.Start();


        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            
        }


        public T? GetData<T>()
        {


            if (typeof(T) == typeof(Customers))
            {
                return (T)(object)_customers;
            }

            return default(T);

        }

        internal void Run()
        {




            Application.Run(new Form1(this));
        }


    }
}
