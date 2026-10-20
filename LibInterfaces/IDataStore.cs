
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibInterfaces;

public interface IDataStore
{

    public void EnsureTable<T>() where T : class;

    public IReadOnlyList<T> All<T>() where T : class;

    public bool Update<T>(T item) where T : class;

    public T? Get<T>(object id) where T : class;

    public void InTransaction<T>(Action<T> work) where T : class;

    public void Insert<T>(T item) where T : class;


}
