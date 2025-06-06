global using Terraria;
global using Terraria.ID;
global using Terraria.IO;
global using Terraria.ModLoader;
using MatterRecord.Contents.CantSeword;
using MatterRecord.Contents.DonQuijoteDeLaMancha;
using MatterRecord.Contents.EternalWine;
using MatterRecord.Contents.Faust;
using MatterRecord.Contents.TheAdventureofSherlockHolmes;
using MatterRecord.Contents.TheOldManAndTheSea;
using MatterRecord.Contents.TheoryOfFreedom;
using MatterRecord.Contents.TheoryofJustice;
using MatterRecord.Contents.TortoiseShell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Config;

namespace MatterRecord
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class MatterRecord : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PacketType packetType = (PacketType)reader.ReadByte();
            switch (packetType)
            {
                case PacketType.EternalWinePlayerSync:
                    {
                        byte whoami = reader.ReadByte();
                        float debt = reader.ReadSingle();
                        float max = reader.ReadSingle();
                        var plr = Main.player[whoami];
                        var mplr = plr.GetModPlayer<EternalWinePlayer>();
                        mplr.LifeDebt = debt;
                        mplr.LifeDebtMax = max;
                        if (Main.netMode == NetmodeID.Server)
                        {
                            var packet = GetPacket();
                            packet.Write((byte)packetType);
                            packet.Write(whoami);
                            packet.Write(debt);
                            packet.Write(max);
                            packet.Send(-1, whoAmI);
                        }
                        break;
                    }
                case PacketType.PlayerVelocitySync:
                    {
                        byte whoami = reader.ReadByte();
                        Vector2 velocity = reader.ReadVector2();
                        var plr = Main.player[whoami];
                        plr.velocity = velocity;
                        var mplr = plr.GetModPlayer<TortoiseShellPlayer>();
                        mplr.syncVelocity = velocity;
                        mplr.counter = 1;
                        if (Main.netMode == NetmodeID.Server)
                        {
                            var packet = GetPacket();
                            packet.Write((byte)packetType);
                            packet.Write(whoami);
                            packet.WriteVector2(velocity);
                            packet.Send(-1, whoAmI);
                        }
                        break;
                    }
                case PacketType.NPCVelocitySync:
                    {
                        byte whoami = reader.ReadByte();
                        Vector2 velocity = reader.ReadVector2();
                        Main.npc[whoami].velocity = velocity;
                        if (Main.netMode == NetmodeID.Server)
                        {
                            var packet = GetPacket();
                            packet.Write((byte)packetType);
                            packet.Write(whoami);
                            packet.WriteVector2(velocity);
                            packet.Send(-1, whoAmI);
                        }
                        break;
                    }
                case PacketType.HookPointSync: 
                    {
                        byte whoami = reader.ReadByte();
                        byte count = reader.ReadByte();
                        List<Point> coords = new List<Point>();
                        for (int n = 0; n < count; n++) 
                        {
                            coords.Add(new Point(reader.ReadInt32(), reader.ReadInt32()));
                        }
                        var plr = Main.player[whoami];
                        var mplr = plr.GetModPlayer<FreedomPlayer>();
                        mplr.targetTileCoords.Clear();
                        mplr.targetTileCoords.AddRange(coords);
                        if (Main.netMode == NetmodeID.Server)
                        {
                            var packet = GetPacket();
                            packet.Write((byte)packetType);
                            packet.Write(whoami);
                            packet.Write(count);
                            for (int n = 0; n < count; n++)
                            {
                                packet.Write(coords[n].X);
                                packet.Write(coords[n].Y);
                            }
                            packet.Send(-1, whoAmI);
                        }
                        break;
                    }
                case PacketType.TASHTileSync: 
                    {
                        if (Main.netMode != NetmodeID.Server) break;
                        var point = new Point(reader.ReadInt32(), reader.ReadInt32());
                        NetMessage.SendSection(whoAmI, point.X / 200, point.Y / 150);
                        var packet = GetPacket();
                        packet.Write((byte)PacketType.TASHSyncSuccessed);
                        packet.Send(whoAmI);
                        break;
                    }
                case PacketType.TASHSyncSuccessed: 
                    {
                        TASHSystem.readyToShow = true;
                        break;
                    }
                case PacketType.FaustSync: 
                    {
                        byte playerNumber = reader.ReadByte();
                        FaustPlayer examplePlayer = Main.player[playerNumber].GetModPlayer<FaustPlayer>();
                        examplePlayer.ReceivePlayerSync(reader);
                        if (Main.netMode == NetmodeID.Server)
                            examplePlayer.SyncPlayer(-1, whoAmI, false);

                        break;
                    }
            }
            base.HandlePacket(reader, whoAmI);
        }
    }
    public enum PacketType : byte
    {
        EternalWinePlayerSync,
        PlayerVelocitySync,
        NPCVelocitySync,
        HookPointSync,
        TASHTileSync,
        TASHSyncSuccessed,
        FaustSync
    }


    public class ContentsLoots : GlobalNPC 
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            int itemtype = npc.type switch
            {
                NPCID.KingSlime => ModContent.ItemType<TheAdventureofSherlockHolmes>(),
                NPCID.DukeFishron => ModContent.ItemType<TheOldManAndTheSea>(),
                NPCID.Plantera => ModContent.ItemType<TheoryOfFreedom>(),
                NPCID.EyeofCthulhu=> ModContent.ItemType<DonQuijoteDeLaMancha>(),
                NPCID.DD2DarkMageT1 or NPCID.DD2DarkMageT3 => ModContent.ItemType<TheoryofJustice>(),
                NPCID.BrainofCthulhu => ModContent.ItemType<Faust>(),
                _ => -1
            };
            if (itemtype != -1)
                npcLoot.Add(ItemDropRule.Common(itemtype));

            if (npc.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail)
                npcLoot.Add(ItemDropRule.ByCondition(new Conditions.LegacyHack_IsABoss(), ModContent.ItemType<Faust>()));
            base.ModifyNPCLoot(npc, npcLoot);
        }
    }


    public class MatterRecordConfig : ModConfig 
    {
        public static MatterRecordConfig Instance => ModContent.GetInstance<MatterRecordConfig>();
        public override ConfigScope Mode => ConfigScope.ServerSide;
        public bool DonQuijoteSlashActive = false;
    }
}
