using System.IO;
using System.Linq;
using Terraria.Localization;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.MeleeCore;

public struct ActionModifyData(float size, float timeScaler, float knockBack, float damage, int critAdder, float critMultiplyer)
{
    public ActionModifyData() : this(1, 1, 1, 1, 0, 1)
    {

    }
    public float Size { get; set; } = size;

    public float TimeScaler { get; set; } = timeScaler;

    public float KnockBack { get; set; } = knockBack;

    public float Damage { get; set; } = damage;

    public int CritAdder { get; set; } = critAdder;

    public float CritMultiplyer { get; set; } = critMultiplyer;

    /// <summary>
    /// 将除了速度以外的值赋给目标
    /// </summary>
    /// <param name="target"></param>
    public readonly void SetActionValue(ref ActionModifyData target)
    {
        float speed = target.TimeScaler;
        target = this with { TimeScaler = speed };
    }

    public readonly void SetActionSpeed(ref ActionModifyData target) => target.TimeScaler = TimeScaler;

    public override readonly string ToString()
    {
        //return (actionOffsetSize, actionOffsetTimeScaler, actionOffsetKnockBack, actionOffsetDamage, actionOffsetCritAdder, actionOffsetCritMultiplyer).ToString();
        var cultureInfo = GameCulture.KnownCultures.First().CultureInfo;
        var result = $"({Size.ToString("0.00", cultureInfo)}|{TimeScaler.ToString("0.00", cultureInfo)}|{KnockBack.ToString("0.00", cultureInfo)}|{Damage.ToString("0.00", cultureInfo)}|{CritAdder.ToString(cultureInfo)}|{CritMultiplyer.ToString("0.00", cultureInfo)})";
        return result;
    }

    public readonly void WriteBinary(BinaryWriter writer)
    {
        writer.Write(Size);
        writer.Write(TimeScaler);
        writer.Write(KnockBack);
        writer.Write(Damage);
        writer.Write((byte)CritAdder);
        writer.Write(CritMultiplyer);
    }

    public void ReadBinary(BinaryReader reader)
    {
        Size = reader.ReadSingle();
        TimeScaler = reader.ReadSingle();
        KnockBack = reader.ReadSingle();
        Damage = reader.ReadSingle();
        CritAdder = reader.ReadByte();
        CritMultiplyer = reader.ReadSingle();
    }
}