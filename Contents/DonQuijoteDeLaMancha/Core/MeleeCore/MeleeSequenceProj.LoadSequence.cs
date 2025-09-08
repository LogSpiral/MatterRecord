using System.Collections.Generic;
using Terraria.DataStructures;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core;


public partial class MeleeSequenceProj
{
    //初始化-加载序列数据
    /// <summary>
    /// 是否是本地序列的弹幕
    /// </summary>
    private bool IsLocalProj => Player.whoAmI == Main.myPlayer;

    /// <summary>
    /// 标记为完工，设置为true后将读取与文件同目录下同类名的xml文件(参考Texture默认读取
    /// </summary>
    public virtual bool LabeledAsCompleted => false;

    public static Dictionary<int, Sequence> LocalMeleeSequence { get; } = [];
    protected Sequence meleeSequence = null;
    public SequenceModel SequenceModel { get; protected set; }

    protected abstract void SetupSequence(Sequence sequence);

    private void InitializeSequence()
    {
        meleeSequence = new Sequence();

        SetupSequence(meleeSequence);

        SequenceModel = new(meleeSequence);
        SequenceModel.OnInitializeElement += element =>
        {
            if (element is not MeleeAction action) return;
            action.StandardInfo = StandardInfo;
            action.Owner = Player;
            action.Projectile = Projectile;
            Projectile.netUpdate = true;
        };
    }

    public override void OnSpawn(IEntitySource source)
    {
        InitializeSequence();
        base.OnSpawn(source);
    }
}