using LibLogging;
using LibSqlLite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

using LibInterfaces;

namespace Timesheeter.Core.Data
{
    public class ProjectCodes : DataClass<ProjectCodes.ProjectCode>
    {

        public ProjectCode? this[Guid key] 
        { 
            get
            {
                
                return _list.Unfiltered.Where(x => x.ID == key).FirstOrDefault();
                
            } 
        }



        public class ProjectCode : IData, ISoftDeletable, IAudited
        {
            [PrimaryKey, GridHidden] public Guid ID { get; set; }
            [Required, Unique(IgnoreCase = true), DisplayName("Project Code")] public string Code { get; set; } = string.Empty;
            [DisplayName("Short Code")] public string ShortCode { get; set; } = String.Empty;
            [Required, DisplayName("Customer"), GridLookup(typeof(Customers), "Name"), References(typeof(Customers.Customer))] public Guid CustomerID { get; set; }
            [DisplayName("Project Name")] public String ProjectName { get; set; } = string.Empty;
            [DisplayName("Description")] public String Description { get; set; } = string.Empty;
            public String Location { get; set; } = String.Empty;
            [DisplayName("Deleted"), GridHidden] public bool Deleted { get; set; } = false;
            [GridHidden, DisplayName("Created (UTC)")] public DateTime TOC { get; set; }
            [GridHidden, DisplayName("Modified (UTC)")] public DateTime TOM { get; set; }
            [GridHidden, MaxLength(Audit.MaxProcessLength), DisplayName("Modified By")] public string MOP { get; set; } = string.Empty;


            // used to allow compatability with the DataComboBox impl...
            [Ignore, GridHidden] public String Name { get { return Code; } set {  Code = value; } }

        }


        public ProjectCodes(SqliteStore dataStore, ILoggerApp? logger) : base(dataStore, logger)
        {
            /*
            _dataStore.EnsureTable<ProjectCode>();

            foreach (var proj in _dataStore.All<ProjectCode>())
            {
                AddLoaded(proj);
            }
            */
        }

        /// <summary>The project code with this code (deleted or not), other than <paramref name="except"/>.</summary>
        public ProjectCode? GetOtherByCode(String projCode, ProjectCode? except)
        {
            return _list.Unfiltered.FirstOrDefault(code => code != except &&
                String.Compare(code.Code, projCode.Trim(), StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        /// <summary>
        /// <paramref name="renamed"/> was given another project code's code: that project code is kept
        /// (brought back if deleted), time entries (deleted ones too) move over to it, and
        /// <paramref name="renamed"/> is deleted under its old code.
        /// </summary>
        public bool MergeRenamed(ProjectCode renamed, TimeEntries timeEntries)
        {
            var target = GetOtherByCode(renamed.Code, renamed);
            return target != null && MergeInto(renamed, target, timeEntries, nameof(TimeEntries.TimeEntry.ProjectCodeID));
        }

        /// <summary>The project code with this code (ignoring case), deleted or not.</summary>
        public ProjectCode? GetByCode(String projCode)
        {
            return _list.Unfiltered.FirstOrDefault(code => String.Compare(code.Code, projCode.Trim(), StringComparison.InvariantCultureIgnoreCase) == 0);
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
