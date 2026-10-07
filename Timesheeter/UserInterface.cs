using Timesheeter.Lib;

namespace Timesheeter
{
    public partial class UserInterface : Form
    {


        private IDataFactory _factory;
        public UserInterface(IDataFactory factory)
        {
            InitializeComponent();
            _factory = factory;

            using (var iconStream = typeof(UserInterface).Assembly.GetManifestResourceStream("Timesheeter.ico"))
            {
                if (iconStream != null) Icon = new Icon(iconStream);
            }


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
