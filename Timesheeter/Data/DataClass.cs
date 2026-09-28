using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Timesheeter.Data
{
    public interface IDataClass
    {
        IBindingList All { get; }
    }

    public abstract class DataClass<T> : IDataClass
    {
        public BindingList<T> All { get { return _list; } }

        IBindingList IDataClass.All { get { return _list; } }

        protected BindingList<T> _list;




    }
}
