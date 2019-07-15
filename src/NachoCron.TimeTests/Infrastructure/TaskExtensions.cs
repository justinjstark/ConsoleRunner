using System;
using System.Threading.Tasks;

namespace NachoCron.TimeTests.Infrastructure
{
    public static class TaskExtensions
    {
        public static async Task WithTimeout(this Task task, TimeSpan timeout, bool throwOnTimeout = true)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return;
            }
            else
            {
                if (throwOnTimeout)
                {
                    throw new TimeoutException();
                }
            }
        }
    }
}
