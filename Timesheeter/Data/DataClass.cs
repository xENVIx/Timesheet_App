using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Timesheeter.Data
{
    public abstract class DataClass<T>
    {
        public BindingList<T> All { get { return _list; } }

        protected BindingList<T> _list;




    }
}
