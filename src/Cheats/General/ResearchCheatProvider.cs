using CaptainOfCheats.Extensions;
using Mafi.Core.Input;
using Mafi.Core.MessageNotifications;
using Mafi.Core.Research;

namespace CaptainOfCheats.Cheats.General
{
    public class ResearchCheatProvider
    {
        private readonly IInputScheduler _inputScheduler;
        private readonly IMessageNotificationsManager _messageNotificationsManager;
        private readonly ResearchManager _researchManager;

        public ResearchCheatProvider(IInputScheduler inputScheduler, ResearchManager researchManager, IMessageNotificationsManager messageNotificationsManager)
        {
            _inputScheduler = inputScheduler;
            _researchManager = researchManager;
            _messageNotificationsManager = messageNotificationsManager;
        }

        public void UnlockCurrentResearch()
        {
            _inputScheduler.ScheduleInputCmd(new ResearchCheatFinishCmd());
        }

        public void UnlockAllResearch()
        {
            _researchManager.Call("Cheat_UnlockAllResearch");
            _messageNotificationsManager.DismissAllNotifications();
        }
    }
}