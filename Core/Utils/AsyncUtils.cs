using System.Threading.Tasks;

namespace Core.Utils
{
    public static class AsyncUtils
    {
        public static bool IsTaskAlive(Task task)
        {
            switch (task?.Status)
            {
                case TaskStatus.WaitingForActivation:
                case TaskStatus.WaitingToRun:
                case TaskStatus.Running:
                    return true;
                default:
                    return false;
            }
        }
    }
}
