
namespace Timesheeter.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {

            

            Factory factory = new Factory();
            if (!factory.Initialize(args))
            {

                // Only when someone can press a key: ReadKey throws when input is redirected (scripts, scheduled tasks).
                if (!Console.IsInputRedirected)
                {
                    Console.WriteLine($"Press Any Key To Exit");
                    Console.ReadKey();
                }

                return;
            }
            factory.Run();
        }
    }
}
