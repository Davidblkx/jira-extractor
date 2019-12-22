using System;
using System.Threading.Tasks;
using jiraps.ResultsHandler;

namespace jiraps
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 0) {
                if (args.Length != 3) {
                    Console.WriteLine("Arguments should be in format [sprint] [start (YYYY-MM-DD)] [end (YYYY-MM-DD)]");
                    return;
                }

                var sprint = args[0];
                var start = ParseDate(args[1]);
                var end = ParseDate(args[2]);
                JiraHandler.JiraHandlerService.SetCurrentSprint(sprint);
                JiraHandler.JiraHandlerService.SetSprintDate(start, end);
                Console.WriteLine($"Loading worklog for {sprint}, from {start.ToShortDateString()} to {end.ToShortDateString()}");
            }
            await ResultsHandlerService.GenerateCSVReport();
        }

        static DateTime ParseDate(string date)
        {
            if (DateTime.TryParse(date, out var d)) return d;

            Console.WriteLine($"date not valid: {date}");
            Environment.Exit(1);
            throw new Exception("Date not valid");
        }
    }
}
