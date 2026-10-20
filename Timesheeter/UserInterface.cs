using Timesheeter.Lib;

using LibInterfaces;
using Microsoft.Extensions.Logging;
using LibLogging;

namespace Timesheeter
{
    public partial class UserInterface : Form
    {

        private IDataFactory _datFactory;
        private ILoggerApp? _logger;
        public UserInterface(IDataFactory datFactory, ILoggerApp? logger)
        {
            InitializeComponent();
            _datFactory = datFactory;
            _logger = logger;

            using (var iconStream = typeof(UserInterface).Assembly.GetManifestResourceStream("Timesheeter.ico"))
            {
                if (iconStream != null) Icon = new Icon(iconStream);
            }


            this.Load += UserInterface_Load;
        }

        private void UserInterface_Load(object? sender, EventArgs e)
        {
            try
            {
                _ucMain.PostInit(_datFactory, _logger);
            }
            catch (Exception ex)
            {
                // A page that failed to load would hit its missing data later, so close instead of carrying on.
                MessageBox.Show($"Exception While Loading the Interface\n{ex.Message}");
                Close();
                return;
            }
            // Styles every control now, and any added later.
            Theme.Apply(this);
        }
    }
}
