namespace MatterRecord.Contents.EternalWine
{
    public class Eternal : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            base.Update(player, ref buffIndex);  // 先调用基类

            // 为所有伤害分组设置无敌帧
            foreach (int cooldownID in ImmunityHelper.GetAllImmunityCooldownIDs())
            {
                player.AddImmuneTime(cooldownID, 2);  // 2帧无敌
            }

            // 保留通用设置
            player.immune = true;
            player.immuneTime = 2;
        }
    }
}