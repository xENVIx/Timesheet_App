using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheeter.Elements
{
    public class AccordionPanel : Panel
    {

        private Button _btnAccord;
        private Panel _pnlControls;
        private Panel _pnlButton;

        public AccordionPanel() : base()
        {

            _panelAdding = true;

            _pnlControls = new Panel();
            _pnlButton = new Panel();
            _btnAccord = new Button();
            //_btnAccord.Location = new Point(this.Location.X, this.Location.Y - 20);
            ///_btnAccord.Size = new Size(20, 20);
            _btnAccord.Text = ">";
            _btnAccord.Click += _btnAccord_Click;

            this.Controls.Add(_pnlButton);
            this.Controls.Add(_pnlControls);
            _pnlButton.Dock = DockStyle.Top;
            _pnlControls.Dock = DockStyle.Fill;
            _pnlButton.Controls.Add(_btnAccord);

            _pnlButton.Size = new Size(_pnlButton.Width, 40);
            _btnAccord.Location = new Point(10, 10);
            _btnAccord.Size = new Size(10, 10);

            _panelAdding = false;
            
        }

        private volatile bool _panelAdding = false;

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            if (!_panelAdding)
            {
                if (e.Control != null)
                {
                    _pnlControls.Controls.Remove(e.Control);
                }
            }
            else base.OnControlAdded(e);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (!_panelAdding)
            {
                if (e.Control != null)
                {
                    _pnlControls.Controls.Add(e.Control);
                }
            }
            else base.OnControlAdded(e);

            //base.OnControlAdded(e);
        }

        private void _btnAccord_Click(object? sender, EventArgs e)
        {
            


        }
    }
}
