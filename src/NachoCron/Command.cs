using System.Threading.Tasks;

namespace NachoCron
{
    public class Command
    {
        public Task<CommandResult> Task { get; }
        public int ProcessId { get; }

        public Command(Task<CommandResult> task, int processId)
        {
            Task = task;
            ProcessId = processId;
        }
    }
}
