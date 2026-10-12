using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheeter.Lib;

namespace Timesheeter.Elements
{
    public class BaseButton : Button
    {

        public event EventHandler<BaseButtonClickedEventArgs>? BaseClicked;

        public UCSubPage? SelectedPage { get; set; } = null;

        public BaseButton() : base()
        {
            this.Click += BaseButton_Click;
        }

        private void BaseButton_Click(object? sender, EventArgs e)
        {
            BaseClicked?.Invoke(sender, new BaseButtonClickedEventArgs(this, SelectedPage));
        }
    }


    public class BaseButtonClickedEventArgs : EventArgs
    {
        public String Name { get { return Page?.Name ?? ""; } }
        public UCSubPage? Page { get; private set; }
        public BaseButton Button { get; private set; }
        public BaseButtonClickedEventArgs(BaseButton button, UCSubPage? page) : base()
        {
            Button = button;
            Page = page;
        }
    }
}
