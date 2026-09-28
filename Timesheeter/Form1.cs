using Timesheeter.Lib;

namespace Timesheeter
{
    public partial class Form1 : Form
    {


        private IFactory _factory;
        public Form1(IFactory factory)
        {
            InitializeComponent();
            _factory = factory;


            this.Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            _ucMain.PostInit(_factory);
        }
    }
}
