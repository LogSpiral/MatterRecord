using Microsoft.Xna.Framework;
using NetSimplified;
using System.IO;

namespace MatterRecord.Contents.TheHitchhikersGuideToTerraria;

/// <summary>
/// 《泰拉瑞亚漫游指南》专用的网络模块（沿用项目既有的 NetSimplified 方案）。
/// <para>承担两件事：</para>
/// <para>1. 补发漫游指南：客户端进入世界发现需要补发 → 发送请求 → 服务端校验后发放并回执。
/// 之所以不在客户端本地发放，是因为多人模式下客户端修改自己的背包不会同步给服务端，
/// 之后任何一次服务端数据下发都会把它覆盖掉。</para>
/// <para>2. 切换「事象记录」获取配置：<see cref="MatterRecordConfig.AllowingRecordRecipe"/> 属于
/// <c>ConfigScope.ServerSide</c> 配置，多人下客户端本地改值不会被服务端承认，
/// 因此客户端只发请求，由服务端权威切换后再把结果广播给所有端镜像。</para>
/// </summary>
internal class TheHitchhikersGuideSync : NetModule
{
    /// <summary>操作码：客户端请求补发指南。</summary>
    private const byte OpRequestGuide = 0;

    /// <summary>操作码：服务端补发完毕的回执。</summary>
    private const byte OpAcknowledgeGuide = 1;

    /// <summary>操作码：客户端请求服务端切换「事象记录」获取配置。</summary>
    private const byte OpToggleConfig = 2;

    /// <summary>操作码：服务端广播配置的权威值。</summary>
    private const byte OpBroadcastConfig = 3;

    /// <summary>本次报文的操作码。</summary>
    private byte _op;

    /// <summary>请求者（或回执接收者、配置切换发起者）的玩家索引。</summary>
    private byte _whoAmI;

    /// <summary>操作码为配置广播时携带的权威配置值。</summary>
    private bool _configValue;

    /// <summary>
    /// 构造一个「客户端 → 服务端」的补发请求包。
    /// </summary>
    /// <param name="whoAmI">发起请求的玩家索引。</param>
    /// <returns>可直接 Send 的网络包实例。</returns>
    public static TheHitchhikersGuideSync Get(int whoAmI) => Create(OpRequestGuide, whoAmI, false);

    /// <summary>
    /// 构造一个「服务端 → 客户端」的补发回执包。
    /// </summary>
    /// <param name="whoAmI">目标玩家索引。</param>
    /// <returns>可直接 Send 的网络包实例。</returns>
    public static TheHitchhikersGuideSync GetAcknowledged(int whoAmI) => Create(OpAcknowledgeGuide, whoAmI, false);

    /// <summary>
    /// 构造一个「客户端 → 服务端」的配置切换请求包。
    /// </summary>
    /// <returns>可直接 Send 的网络包实例。</returns>
    public static TheHitchhikersGuideSync GetToggleConfig() => Create(OpToggleConfig, 0, false);

    /// <summary>
    /// 构造一个「服务端 → 所有客户端」的配置值广播包。
    /// </summary>
    /// <param name="value">服务端权威的配置值。</param>
    /// <param name="whoAmI">发起切换的玩家索引（用于只让发起者本地看到提示）。</param>
    /// <returns>可直接 Send 的网络包实例。</returns>
    public static TheHitchhikersGuideSync GetBroadcastConfig(bool value, int whoAmI) => Create(OpBroadcastConfig, whoAmI, value);

    /// <summary>
    /// 初始化网络包字段的公共实现，保证字段赋值顺序与各 <c>Get</c> 工厂的参数顺序、
    /// <see cref="Read"/> 的读取顺序三者一致。
    /// </summary>
    /// <param name="op">操作码。</param>
    /// <param name="whoAmI">玩家索引。</param>
    /// <param name="configValue">配置值（仅配置广播使用）。</param>
    /// <returns>已填好字段的网络包实例。</returns>
    private static TheHitchhikersGuideSync Create(byte op, int whoAmI, bool configValue)
    {
        var packet = NetModuleLoader.Get<TheHitchhikersGuideSync>();
        packet._op = op;
        packet._whoAmI = (byte)whoAmI;
        packet._configValue = configValue;
        return packet;
    }

    /// <summary>
    /// 客户端请求服务端补发漫游指南。仅在多人客户端调用。
    /// </summary>
    /// <param name="whoAmI">本地玩家索引。</param>
    public static void Request(int whoAmI) => Get(whoAmI).Send();

    /// <summary>
    /// 切换「事象记录」的获取配置（<see cref="MatterRecordConfig.AllowingRecordRecipe"/>）。
    /// <para>单人模式本地直接切换；多人客户端只发请求，由服务端权威切换后广播回来（发起者会收到聊天栏提示）。</para>
    /// </summary>
    public static void ToggleConfig()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            GetToggleConfig().Send();
            return;
        }

        // 单人模式：没有服务端需要通知，直接切换并在本地提示
        ApplyConfig(!MatterRecordConfig.Instance.AllowingRecordRecipe, true);
    }

    /// <summary>
    /// 把配置值写入本地配置实例。
    /// <para>配方条件 <c>.AddCondition(..., () =&gt; MatterRecordConfig.Instance.AllowingRecordRecipe)</c> 是延迟求值的，
    /// 因此这里改完立即生效，无需重载模组。</para>
    /// </summary>
    /// <param name="value">要写入的配置值。</param>
    /// <param name="notify">是否在本地聊天栏提示当前状态（只有发起切换的玩家需要看到）。</param>
    private static void ApplyConfig(bool value, bool notify)
    {
        var config = MatterRecordConfig.Instance;
        config.AllowingRecordRecipe = value;
        config.OnChanged();

        if (notify)
            Main.NewText(
                TheHitchhikersGuideToTerraria.GetText(value ? "ConfigOn" : "ConfigOff"),
                value ? new Color(120, 255, 120) : new Color(180, 200, 255));
    }

    /// <summary>
    /// 写入网络包内容。
    /// </summary>
    /// <param name="p">待写入的网络包。</param>
    public override void Send(ModPacket p)
    {
        p.Write(_op);
        p.Write(_whoAmI);
        p.Write(_configValue);
    }

    /// <summary>
    /// 读取网络包内容。
    /// </summary>
    /// <param name="r">网络包读取器。</param>
    public override void Read(BinaryReader r)
    {
        _op = r.ReadByte();
        _whoAmI = r.ReadByte();
        _configValue = r.ReadBoolean();
    }

    /// <summary>
    /// 收到网络包后的处理：按操作码分流。
    /// </summary>
    public override void Receive()
    {
        switch (_op)
        {
            case OpRequestGuide:
            case OpAcknowledgeGuide:
                ReceiveGuide();
                break;
            case OpToggleConfig:
                ReceiveToggleConfig();
                break;
            case OpBroadcastConfig:
                ReceiveBroadcastConfig();
                break;
        }
    }

    /// <summary>
    /// 处理补发相关的报文。
    /// <para>客户端分支：落定本地领取标记（物品已由服务端的玩家数据同步下发）。</para>
    /// <para>服务端分支：校验并发放，然后回执请求方。</para>
    /// </summary>
    private void ReceiveGuide()
    {
        if (!Main.dedServ)
        {
            // 客户端：服务端已经处理完毕，把本地标记置位后就不会再反复请求
            if (_op == OpAcknowledgeGuide && _whoAmI < Main.maxPlayers)
                Main.player[_whoAmI].GetModPlayer<TheHitchhikersGuideToTerrariaPlayer>().receivedGuide = true;

            return;
        }

        // 以下为服务端逻辑
        if (_op != OpRequestGuide || _whoAmI >= Main.maxPlayers)
            return;

        var player = Main.player[_whoAmI];
        if (player is null || !player.active)
            return;

        // GrantGuide 内部已做幂等校验（已领取 / 背包已有都不再发放），因此这里无需额外判断，
        // 也就不会出现「重复发包刷物品」的问题。
        TheHitchhikersGuideToTerrariaPlayer.GrantGuide(player);

        // 回执给请求的客户端，让它把本地领取标记也置位
        GetAcknowledged(_whoAmI).Send(_whoAmI, -1);
    }

    /// <summary>
    /// 处理配置切换请求。
    /// <para>该类报文（客户端 → 服务端）只在服务端侧触发本方法，因此这里直接按权威端处理：
    /// 翻转配置值，再把新值广播给所有客户端（含发起者）。</para>
    /// </summary>
    private void ReceiveToggleConfig()
    {
        bool newValue = !MatterRecordConfig.Instance.AllowingRecordRecipe;
        ApplyConfig(newValue, false);

        // 显式指定「发给所有客户端、不忽略任何端」，与项目内既有的 Send(_, _) 用法保持一致
        GetBroadcastConfig(newValue, _whoAmI).Send(-1, -1);
    }

    /// <summary>
    /// 处理配置值广播：各端把服务端权威值镜像到本地配置实例。
    /// <para>只有发起切换的玩家会在本地聊天栏看到状态提示，避免刷屏。</para>
    /// </summary>
    private void ReceiveBroadcastConfig()
    {
        if (Main.dedServ)
            return;

        ApplyConfig(_configValue, _whoAmI == Main.myPlayer);
    }
}
