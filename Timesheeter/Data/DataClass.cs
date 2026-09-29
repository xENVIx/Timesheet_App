using LibSqlLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheeter.Lib;
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
            _list = new BindingListView<T>();
        }

        /// <summary>
        /// Checks the item's DataAnnotations attributes, e.g. [Required] rejects null, "" and whitespace.
        /// The database can't do this for text: a UNIQUE index allows any number of NULLs.
        /// </summary>
        protected bool IsValid(T item)
        {
            var errors = GetValidationErrors(item);
            foreach (var error in errors) Console.WriteLine($"Invalid: {error}");
            return errors.Count == 0;
        }

        /// <summary>The item's validation error messages; empty when it can be saved.</summary>
        public IReadOnlyList<string> GetValidationErrors(T item)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(item, new ValidationContext(item), results, validateAllProperties: true);
            return results.Select(r => r.ErrorMessage ?? "Invalid value.").ToList();
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

        

        /// <summary>
        /// The items, sortable and filterable (e.g. through a BindingSource). A filter hides items
        /// from everything bound to this list; use All.Unfiltered to look through every item.
        /// </summary>
        public BindingListView<T> All { get { return _list; } }

        IBindingList IDataClass.All { get { return _list; } }

        protected BindingListView<T> _list;




    }
}
