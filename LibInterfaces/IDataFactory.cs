using System;
using System.Collections.Generic;
using System.Text;

namespace LibInterfaces
{
    public interface IDataFactory
    {
        //public List<DataBase> Data { get; set; }
        public T? GetData<T>();
        public Object? GetData(Type dataType);

    }
}
