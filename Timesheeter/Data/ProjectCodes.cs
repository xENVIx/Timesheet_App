using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

using LibSqlLite;

namespace Timesheeter.Data
{
    public class ProjectCodes : DataClass<ProjectCodes.ProjectCode>
    {

        public ProjectCode? this[long key] 
        { 
            get
            {
                
                return _list.Unfiltered.Where(x => x.ID == key).FirstOrDefault();
                
            } 
        }



        public class ProjectCode
        {
            [PrimaryKey, GridHidden] public long ID { get; set; }
            [Required, Unique(IgnoreCase = true), DisplayName("Project Code")] public string Code { get; set; } = string.Empty;
            [Required, DisplayName("Customer"), GridLookup(typeof(Customers), "Name")] public long CustomerID { get; set; }
            [DisplayName("Description")] public String Description { get; set; } = string.Empty;
            public String Location { get; set; } = String.Empty;


            // used to allow compatability with the DataComboBox impl...
            [Ignore, GridHidden] public String Name { get { return Code; } set {  Code = value; } }

        }


        public ProjectCodes(SqliteStore dataStore) : base(dataStore)
        {
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
            if (_list.Unfiltered.Where(code => String.Compare(code.Code, projCode, StringComparison.InvariantCultureIgnoreCase) == 0).ToList().Count > 0) return true;


            return false;
        }

        /*
        public bool Save(ProjectCode projCode)
        {
            if (!IsValid(projCode)) return false;

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
        */


    }
}
