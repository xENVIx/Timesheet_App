using LibSqlLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheeter.Core.Lib;
using static Timesheeter.Core.Data.ProjectCodes;

namespace Timesheeter.Core.Data
{
    public interface IDataClass
    {
        IBindingList All { get; }
        IBindingList Active { get; }
    }

    public abstract class DataClass<T> : IDataClass where T : class
    {

        protected SqliteStore _dataStore;

        public DataClass(SqliteStore dataStore)
        {
            _dataStore = dataStore;
            _list = new BindingListView<T>();
            _active = new BindingListView<T>();

            // The active list holds the same items; the filter hides the deleted ones.
            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T))) _active.Filter = "Deleted = false";
        }

        /// <summary>
        /// Whether deleted records are loaded into <see cref="All"/> (still shown wherever other records
        /// refer to them) or left out of the app completely. Either way they stay in the database.
        /// </summary>
        protected virtual bool KeepsDeleted => true;

        /// <summary>Adds an item read from the database to the lists (subclass constructors).</summary>
        protected void AddLoaded(T item)
        {
            if (item is ISoftDeletable { Deleted: true } && !KeepsDeleted) return;

            _list.Add(item);
            _active.Add(item);
        }

        /// <summary>Sorts both <see cref="All"/> and <see cref="Active"/>.</summary>
        protected void SetSort(string sort)
        {
            _list.Sort = sort;
            _active.Sort = sort;
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
            if (fresh == null) return;

            if (index >= 0) _list[index] = fresh;
            int activeIndex = _active.IndexOf(item);
            if (activeIndex >= 0) _active[activeIndex] = fresh;
        }

        /// <summary>
        /// Call after an item's values changed: re-sorts it in both lists and tells everything bound
        /// to them (see <see cref="BindingListView{T}.Reposition"/>).
        /// </summary>
        public void Reposition(T item)
        {
            _list.Reposition(item);
            _active.Reposition(item);
        }

        /// <summary>
        /// Marks the item deleted in the database (the row is kept). It leaves <see cref="Active"/>,
        /// so its own screen and pickers no longer show it. With <see cref="KeepsDeleted"/> it stays in
        /// <see cref="All"/>, so records referring to it still show it; otherwise it leaves the app.
        /// </summary>
        public bool Delete(T item)
        {
            if (item is not ISoftDeletable deletable) throw new NotSupportedException($"{typeof(T).Name} can't be deleted.");

            deletable.Deleted = true;

            // Not validated like Update: a record with an invalid value can still be deleted.
            bool saved;
            try { saved = _dataStore.Update(item); }
            catch (Exception ex) { Console.WriteLine($"Exception: {ex.Message}"); saved = false; }

            if (!saved)
            {
                deletable.Deleted = false;
                return false;
            }

            if (KeepsDeleted)
            {
                Reposition(item);
            }
            else
            {
                _list.Remove(item);
                _active.Remove(item);
            }
            return true;
        }

        /// <summary>
        /// Un-deletes the item (e.g. a new record was given a deleted one's name), saving any other
        /// changes made to it too.
        /// </summary>
        public bool Restore(T item)
        {
            if (item is not ISoftDeletable deletable) throw new NotSupportedException($"{typeof(T).Name} can't be deleted.");

            bool wasDeleted = deletable.Deleted;
            deletable.Deleted = false;
            if (!Update(item))
            {
                deletable.Deleted = wasDeleted;
                return false;
            }

            Reposition(item);
            return true;
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
            _active.Add(item);
            return true;
        }

        

        /// <summary>
        /// The items, sortable and filterable (e.g. through a BindingSource). A filter hides items
        /// from everything bound to this list; use All.Unfiltered to look through every item.
        /// Includes deleted items when <see cref="KeepsDeleted"/>: use it to look records up by ID.
        /// </summary>
        public BindingListView<T> All { get { return _list; } }

        /// <summary>
        /// The same items without the deleted ones: for the records' own screen, and for picking a
        /// record for something new.
        /// </summary>
        public BindingListView<T> Active { get { return _active; } }

        IBindingList IDataClass.All { get { return _list; } }
        IBindingList IDataClass.Active { get { return _active; } }

        protected BindingListView<T> _list;
        protected BindingListView<T> _active;




    }
}
