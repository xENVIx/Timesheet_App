using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Data;

namespace Timesheeter.Elements
{
    public class DataGridViewData<T> : DataGridView
    {


        T? _data;

        public DataGridViewData() : base()
        {
            
        }


        public void PostInit(DataClass<T>? data)
        {
            
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.DataSource = data.All;


        }










    }
}
