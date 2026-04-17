using Terraria.UI;                // 包含 UIState 等基础类，建议保留

namespace MatterRecord.Contents.SystemControlSword
{
    public class SystemControlSword : ModItem
    {
        public override void SetDefaults()
        {
            
            Item.damage = 6;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5;
            Item.value = 100;
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
        public override bool? UseItem(Player player)
        {
            // 仅在客户端且当前玩家为本地玩家时打开界面
            if (Main.netMode != NetmodeID.Server && player.whoAmI == Main.myPlayer)
            {
                IngameFancyUI.OpenKeybinds();
            }
            return true;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            var modPlayer = player.GetModPlayer<SystemControlSwordPlayer>();
            // 当计时器达到120时，造成额外30点伤害，并重置计时器
            if (modPlayer.timer == 120)
            {
                // 仅考虑本地（单人模式），直接造成伤害
                target.StrikeNPC(30, 0f, 0, false, false, false);
                modPlayer.timer = 0;
            }
        }
    }

    public class SystemControlSwordPlayer : ModPlayer
    {
        public int timer = 0;

        public override void PostUpdate()
        {
            // 计时器始终增长（0~120），无论界面状态
            if (timer < 120)
                timer++;
        }
    }


}

