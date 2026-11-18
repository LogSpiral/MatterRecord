//using NetSimplified;

//namespace MatterRecord.Contents.LordOfTheFlies;

//public class TeleportEffectSync:NetModule
//{
//    public static TeleportEffectSync Get() => NetModuleLoader.Get<TeleportEffectSync>();
//    public override void Receive()
//    {
//        Main.player[Sender].GetModPlayer<LordOfTheFliesTeleportEffectPlayer>().StartTeleportEffect();
//        if (Main.dedServ)
//            Send(-1, Sender);
//    }
//}
