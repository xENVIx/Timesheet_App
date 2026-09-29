using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheeter.Lib
{
    public interface IFactory
    {

        public T? GetData<T>();
        public Object? GetData(Type dataType);

        /// <summary>
        /// Will list the available data types you can obtain in "GetData"....
        /// </summary>
        /// <returns></returns>
        public String[] AvailableDataTypes();
    }
}
