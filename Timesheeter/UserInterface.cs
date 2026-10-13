using Timesheeter.Lib;
using Timesheeter.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Timesheeter
{
    public partial class UserInterface : Form
    {


        private IDataFactory _factory;
        private ILogger? _logger;
        public UserInterface(IDataFactory factory, ILogger? logger)
        {
            InitializeComponent();
            _factory = factory;
            _logger = logger;

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
