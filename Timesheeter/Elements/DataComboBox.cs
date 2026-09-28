using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Data;
using Timesheeter.Lib;

namespace Timesheeter.Elements
{
    public class DataComboBox<T> : ComboBox
    {


        public DataComboBox() : base()
        {

            this.ValueMember = "ID";
            this.DisplayMember = "Name";

        }

        internal void PostInit(DataClass<T>? data)//IFactory factory)
        {

            if (data == null) throw new ArgumentNullException(nameof(data));

            this.DataSource = data.All;

            this.Text = "";

        }






    }
}
