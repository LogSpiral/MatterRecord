using Terraria;

namespace MatterRecord.Contents.Recorder.Dialogue
{
    public abstract class DialogueModule
    {
        public abstract bool IsActive { get; }
        public abstract void OnEnterWorld(Player player, LocalTriggerData data);
        public abstract void PostUpdate(Player player, LocalTriggerData data);
        public abstract void OnHurt(Player player, LocalTriggerData data, Player.HurtInfo info);
    }
}