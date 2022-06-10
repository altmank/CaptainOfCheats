using Mafi;

namespace CaptainOfCheats.Cheats
{
    [MultiDependency]
    public interface ICheatProviderTab
    {
        string Name { get; }
        string IconPath { get; }
    }
}