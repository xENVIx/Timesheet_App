using LibLogging;
using LibSqlLite;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Timesheeter.Core.Data;

using LibInterfaces;

namespace Timesheeter.Server.Classes
{
    public class APIKeys : DataClass<APIKeys.APIKey>
    {

        public APIKeys(IDataStore store, ILoggerApp? logger) : base(store, logger)
        {

        }


        public class APIKey : IData, ISoftDeletable, IAudited
        {
            [PrimaryKey, GridHidden] public Guid ID { get; set; }
            [Required, Unique] public byte[] Key { get; set; } = Array.Empty<byte>();
            [Required, Unique(IgnoreCase = true)] public string Secret { get; set; } = string.Empty;

            [DisplayName("Deleted")] public bool Deleted { get; set; } = false;
            [DisplayName("Created (UTC)")] public DateTime TOC { get; set; }
            [DisplayName("Modified (UTC)")] public DateTime TOM { get; set; }
            [MaxLength(Audit.MaxProcessLength), DisplayName("Modified By")] public string MOP { get; set; } = string.Empty;

        }




    }
}
