using CaptainOfCheats.Config;
using Mafi;
using Mafi.Collections;

namespace CaptainOfCheats.Cheats
{
    [MultiDependency]
    public interface ICheatProvider
    {
        Lyst<ICheatCommandBase> Cheats { get; }
    }
}