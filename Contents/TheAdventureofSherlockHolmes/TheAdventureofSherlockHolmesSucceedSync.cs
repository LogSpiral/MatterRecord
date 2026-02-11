using NetSimplified;
namespace MatterRecord.Contents.TheAdventureofSherlockHolmes;

internal class TheAdventureofSherlockHolmesSucceedSync : NetModule
{
    public static TheAdventureofSherlockHolmesSucceedSync Get() => NetModuleLoader.Get<TheAdventureofSherlockHolmesSucceedSync>();
    public override void Receive()
    {
        TheAdventureofSherlockHolmesSystem.readyToShow = true;
    }
}
