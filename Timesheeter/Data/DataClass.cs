using LibSqlLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static Timesheeter.Data.ProjectCodes;

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

        /// <summary>
        /// Checks the item's DataAnnotations attributes, e.g. [Required] rejects null, "" and whitespace.
        /// The database can't do this for text: a UNIQUE index allows any number of NULLs.
        /// </summary>
        protected bool IsValid(T item)
        {
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(item, new ValidationContext(item), results, validateAllProperties: true))
                return true;

            foreach (var result in results) Console.WriteLine($"Invalid: {result.ErrorMessage}");
            return false;
        }

        public bool Update(T item)
        {
            if (!IsValid(item)) return false;

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

        public bool Save(T item)
        {
            if (!IsValid(item)) return false;

            try
            {
                _dataStore.Insert(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            _list.Add(item);
            return true;
        }

        

        public BindingList<T> All { get { return _list; } }

        IBindingList IDataClass.All { get { return _list; } }

        protected BindingList<T> _list;




    }
}
