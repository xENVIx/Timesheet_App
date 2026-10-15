
namespace Timesheeter.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {

            

            Factory factory = new Factory();
            if (!factory.Initialize(args))
            {

                Console.WriteLine($"Press Any Key To Exit");
                Console.ReadKey();
                //Console.Read();

                return;
            }
            factory.Run();
        }
    }
}
