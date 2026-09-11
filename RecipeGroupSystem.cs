using Terraria.Localization;

namespace MatterRecord;

/// <summary>
/// 本模组的合成组（RecipeGroup）注册器。
/// <para>用于把「二选一」的替代材料并入同一条配方，避免同一物品出现两条配方。</para>
/// <para>合成组显示名由自定义本地化 key <c>Mods.MatterRecord.RecipeGroups.*</c> 提供，
/// 文本维护在 <c>Localization\Misc\</c> 目录下的 hjson 文件中（zh-Hans / en-US 各一份）。</para>
/// </summary>
public class RecipeGroupSystem : ModSystem
{
    /// <summary>「银表 / 钨表」合成组名称（《爱丽丝漫游仙境》的额外合成材料）。</summary>
    public const string SilverWatchGroupName = "MatterRecord:SilverWatch";
    public RecipeGroup SilverWatchGroup { get; private set; }

    /// <summary>「火枪 / 送葬者」合成组名称（《蝇王》的额外合成材料）。</summary>
    public const string MusketGroupName = "MatterRecord:Musket";
    public RecipeGroup MusketGroup { get; private set; }

    public static RecipeGroupSystem Instance { get; private set; }

    /// <summary>
    /// 注册本模组所需的合成组。tModLoader 保证本方法先于各物品的 <c>AddRecipes</c> 执行，
    /// 因此可以在配方中直接按名称引用这些组。
    /// </summary>
    public override void AddRecipeGroups()
    {
        // 怀表组：银表或钨表均可
        SilverWatchGroup = 
            RecipeGroup.Register(
                SilverWatchGroupName, 
                "Mods.MatterRecord.RecipeGroups.SilverWatch", 
                ItemID.SilverWatch, ItemID.TungstenWatch);

        // 火枪组：火枪（腐化）或送葬者（猩红）均可
        MusketGroup =
            RecipeGroup.Register(
                MusketGroupName,
                "Mods.MatterRecord.RecipeGroups.Musket",
                ItemID.Musket, ItemID.TheUndertaker);
    }

    public override void Load()
    {
        Instance = this;
    }
    public override void Unload()
    {
        Instance = null;
    }
}
