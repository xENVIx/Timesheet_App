using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheeter.Lib
{
    public interface IFactory
    {

        public T? GetData<T>();
        public Object? GetData(Type dataType);
    }
}
