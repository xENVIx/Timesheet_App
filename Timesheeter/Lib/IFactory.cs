using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheeter.Lib
{
    public interface IFactory
    {

        public Object? GetData(Type dataType);
    }
}
