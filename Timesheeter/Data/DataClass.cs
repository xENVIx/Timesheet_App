using LibSqlLite;
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

    public abstract class DataClass<T> : IDataClass where T : class
    {

        protected SqliteStore _dataStore;

        public DataClass(SqliteStore dataStore)
        {
            _dataStore = dataStore;
            _list = new BindingList<T>();
        }

        public bool Update(T item)
        {
            try { return _dataStore.Update(item);  } 
            catch (Exception ex) { Console.WriteLine($"Exception: {ex.Message}"); return false; }
            
        }

        public void Reload(T item)
        {
            int index = _list.IndexOf(item);
            var id = typeof(T).GetProperty("ID")!.GetValue(item)!;
            var fresh = _dataStore.Get<T>(id);
            if (index >= 0 && fresh != null) _list[index] = fresh;
        }

        public BindingList<T> All { get { return _list; } }

        IBindingList IDataClass.All { get { return _list; } }

        protected BindingList<T> _list;




    }
}
