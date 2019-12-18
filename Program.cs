using System;
using System.Threading.Tasks;
using jiraps.ResultsHandler;

namespace jiraps
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting..");
            await ResultsHandlerService.GenerateCSVReport();
        }
    }
}
