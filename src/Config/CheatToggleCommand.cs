using System;

namespace CaptainOfCheats.Config
{
    public class CheatToggleCommand : ICheatCommandBase
    {
        public CheatToggleCommand(string title, Action<bool> action, Func<bool> isToggleEnabled)
        {
            Title = title;
            Action = action;
            IsToggleEnabled = isToggleEnabled;
        }
        
        public Action<bool> Action { get; }
        public Func<bool> IsToggleEnabled { get; }
        public string Title { get; }
        public string Tooltip { get; set; }
    }
}