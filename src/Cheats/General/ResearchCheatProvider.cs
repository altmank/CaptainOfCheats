using CaptainOfCheats.Config;
using CaptainOfCheats.Logging;
using Mafi.Collections;
using Mafi.Core.Input;
using Mafi.Core.MessageNotifications;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;

namespace CaptainOfCheats.Cheats.General
{
    public class ResearchCheatProvider
    {
        private readonly IInputScheduler _inputScheduler;
        private readonly IMessageNotificationsManager _messageNotificationsManager;
        private readonly ResearchManager _researchManager;
        private readonly UnlockedProtosDb _unlockedProtosDb;

        public ResearchCheatProvider(IInputScheduler inputScheduler, ResearchManager researchManager, UnlockedProtosDb unlockedProtosDb, IMessageNotificationsManager messageNotificationsManager)
        {
            _inputScheduler = inputScheduler;
            _researchManager = researchManager;
            _unlockedProtosDb = unlockedProtosDb;
            _messageNotificationsManager = messageNotificationsManager;
        }



        public void UnlockCurrentResearch()
        {
            _inputScheduler.ScheduleInputCmd(new ResearchCheatFinishCmd());
        }

        public void UnlockAllResearch()
        {
            var researchUnlockProtos = _researchManager.AllNodes.SelectMany(x => x.Proto.RequiredUnlockedProtos.ToLyst());

            Logger.Log.Info("Unlocking TechnologyProtos that are required by research...");
            foreach (var tech in researchUnlockProtos)
            {
                Logger.Log.Info($"Unlocking TechnologyProto {tech}");
                _unlockedProtosDb.Unlock(tech);
            }

            do
            {
                var count = _researchManager.AllNodes.Count(x => x.State == ResearchNodeState.Available);
                Logger.Log.Info($"Researchable Node Count: {count}");

                foreach (var researchNodeProto in _researchManager.AllNodes.Filter(x => x.State == ResearchNodeState.Available))
                {
                    var success = _researchManager.TryStartResearch(researchNodeProto.Proto, out var errorMessage);
                    Logger.Log.Info($"Starting {researchNodeProto.Proto.Id.Value} research, success: {success} {errorMessage}");
                    Logger.Log.Info($"Cheating {researchNodeProto.Proto.Id.Value} research finish");
                    _researchManager.Cheat_FinishCurrent();
                }
            } while (_researchManager.AllNodes.Any(x => x.State == ResearchNodeState.Available));

            _messageNotificationsManager.DismissAllNotifications();
        }
    }
}