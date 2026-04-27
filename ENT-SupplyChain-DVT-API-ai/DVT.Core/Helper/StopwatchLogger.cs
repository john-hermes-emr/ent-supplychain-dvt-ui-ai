using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT.Core.Helper
{
    public class StopwatchLogger
    {
        public int RowCount { get; set; }
        public string Name { get; private set; }
        public Stopwatch Stopwatch { get; private set; }
        public StringBuilder Log { get; private set; }

        public StopwatchLogger(string name, int rowCount = 0)
        {
            Name = name;
            RowCount = rowCount;
            Stopwatch = new Stopwatch();
            Log = new StringBuilder();
            Log.AppendLine($"{Name} initialized with RowCount: {RowCount}");
        }

        public void Start()
        {
            Stopwatch.Start();
        }

        public void Stop()
        {
            Stopwatch.Stop();
        }

        public void StopAndLog(string methodName, bool restart)
        {
            Stopwatch.Stop();

            if(Stopwatch.ElapsedMilliseconds == 0)
            {
                Log.AppendLine($"{Name}.{methodName} execution time: {Stopwatch.Elapsed.Microseconds} us");
            }
            else
            {
                Log.AppendLine($"{Name}.{methodName} execution time: {Stopwatch.ElapsedMilliseconds} ms");
            }

            if (restart)
            {
                Stopwatch.Restart();
            }
        }

        public void AppendToLog(string additionalInfo)
        {
            Log.AppendLine(additionalInfo);
        }
    }
}
