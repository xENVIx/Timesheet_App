using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using LibSqlLite;

namespace Timesheeter.Data
{
    public class ProjectCodes : DataClass<ProjectCodes.ProjectCode>
    {
        //public IReadOnlyCollection<Customer> All { get { return _list; } }
        //public BindingList<Customer> All { get { return _list; } }
        public ProjectCode? this[long key] 
        { 
            get
            {
                
                return _list.Where(x => x.ID == key).FirstOrDefault();
                
            } 
        }

        private LibSqlLite.SqliteStore _dataStore;


        public class ProjectCode
        {
            [PrimaryKey, Browsable(false)] public long ID { get; set; }
            [Unique(IgnoreCase = true), DisplayName("Project Code")] public string Code { get; set; } = string.Empty;
            [DisplayName("Customer"), GridLookup(typeof(Customers), "Name")] public long CustomerID { get; set; }
            public String Location { get; set; } = String.Empty;

        }


        public ProjectCodes(SqliteStore dataStore)
        {
            _list = new BindingList<ProjectCode>();

            _dataStore = dataStore;
            _dataStore.EnsureTable<ProjectCode>();

            foreach (var proj in _dataStore.All<ProjectCode>())
            {
                _list.Add(proj);
            }
        }

        public bool ProjectCodeExistsByCode(String projCode)
        {
            //if (this[key] != null) return true;
            //return false;

            //if (_list.Exists(cust => String.Compare(cust.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)) return true;
            if (_list.Where(code => String.Compare(code.Code, projCode, StringComparison.InvariantCultureIgnoreCase) == 0).ToList().Count > 0) return true;


            return false;
        }

        public bool Save(ProjectCode projCode)
        {
            try
            {
                _dataStore.Insert(projCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            _list.Add(projCode);
            return true;
        }
        


    }
}
