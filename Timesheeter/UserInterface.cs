using Timesheeter.Lib;

namespace Timesheeter
{
    public partial class UserInterface : Form
    {


        private IFactory _factory;
        public UserInterface(IFactory factory)
        {
            InitializeComponent();
            _factory = factory;


            this.Load += UserInterface_Load;
        }

        private void UserInterface_Load(object? sender, EventArgs e)
        {
            _ucMain.PostInit(_factory);

            // Styles every control now, and any added later.
            Theme.Apply(this);
        }
    }
}
