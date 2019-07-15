using System;

namespace NachoCron.TimeTests.Helpers
{
    public static class CronExpression
    {
        public static string Never => $"0 0 0 1 1 ? {DateTime.Now.Year + 2}";

        public static string EveryOneSecond => "0/1 * * * * ? *";

        public static string EveryTwoSeconds => "0/2 * * * * ? *";
    }
}
