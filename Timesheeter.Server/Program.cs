
namespace Timesheeter.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Factory factory = new Factory();
            factory.Initialize(args);
            factory.Run();
        }
    }
}
