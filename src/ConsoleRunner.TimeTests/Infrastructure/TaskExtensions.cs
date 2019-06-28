using Shouldly;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public static class TaskExtensions
    {
        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return await task;
            }
            else
            {
                throw new TimeoutException();
            }
        }
    }
}
