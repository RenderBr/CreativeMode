using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace CreativeMode
{
    [ApiVersion(2, 1)]
    public class CreativeMode : TerrariaPlugin
    {
        public CreativeMode(Main game)
            : base(game)
        {
            Order = 5;
        }

        public override string Name
        {
            get { return "CreativeMode Rewritten"; }
        }

        public override string Author
        {
            get { return "Average"; }
        }

        public override string Description
        {
            get { return "Implements a useful building mode"; }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0); }
        }



        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, GetData);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.ServerJoin.Register(this, ServerJoin);
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);

            Commands.ChatCommands.Add(new Command(new List<string>() { "creativemode.*", "creativemode.paint", "creativemode.tiles" }, CreativeModeCmd, "creativemode"));

            if (!Config.ReadConfig())
            {
                TShock.Log.ConsoleError("Failed to read CreativeModeConfig.json. Consider generating a new config file.");
            }

            if (Config.contents.EnableWhitelist)
            {
                WhiteList = Config.contents.WhitelistItems;
            }
            if (Config.contents.EnableBlacklist)
            {
                BlackList = Config.contents.BlacklistItems;
            }
            if (Config.contents.EnableWhitelist && Config.contents.EnableBlacklist)
            {
                TShock.Log.ConsoleError("CreativeMode Whitelist & Blacklist are both enabled! Defaulted to Whitelist.");
            }
        }


        private DateTime LastCheck = DateTime.UtcNow;

        public List<int> WhiteList = new();
        public List<int> BlackList = new();
        private void ServerJoin(JoinEventArgs args)
        {
            try
            {
                TSPlayer plr = TShock.Players[args.Who];
                CreativeModeTracker.GetProperties(plr.Name);
            }
            catch (Exception e)
            {
                TShock.Log.Error(e.ToString());
                TShock.Log.Error(args.Who.ToString());
            }
        }

        public void OnLeave(LeaveEventArgs args)
        {
            try
            {
                TSPlayer plr = TShock.Players[args.Who];
                CreativeModeTracker.RemoveProperties(plr.Name);
            }
            catch (Exception e)
            {
                TShock.Log.Error(e.ToString());
                TShock.Log.Error(args.Who.ToString());
            }
        }

        public void OnUpdate(EventArgs args)
        {
            if ((DateTime.UtcNow - LastCheck).TotalSeconds > 5)
            {
                LastCheck = DateTime.UtcNow;

                CreativeModeTracker.ApplyBuffs();
            }
        }

        public void CreativeModeCmd(CommandArgs args)
        {
            if (args.Player == null)
                return;

            // /creativemode <on/off> (property)

            TSPlayer plr = args.Player;
            var p = args.Parameters;

            CreativeModeProperties props = CreativeModeTracker.GetProperties(plr.Name);
            if (p.Count <= 0) // toggle creativemode on/off
            {
                var toggle = props.Toggle();
                plr.SendInfoMessage("CreativeMode is now {0}.", toggle ? "enabled" : "disabled");
                return;
            }
            if (p.Count >= 1)
            {
                bool toggle = p[0].ToLower() == "on" ? true : false;
                var prop = p[1]?.ToLower();
                switch (prop)
                {
                    case "s":
                    case "shine":
                    case "glow":
                        {
                            props.Shine = toggle;
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "no":
                    case "nightowl":
                        {
                            props.NightOwl = toggle;
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "infinite":
                    case "inf":
                    case "endless":
                        {
                            props.Endless = toggle;
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "b":
                    case "builder":
                        {
                            props.Builder = toggle;
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "mine":
                    case "m":
                    case "mining":
                        {
                            props.Mining = toggle;
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "p":
                    case "panic":
                        {
                            props.Panic = toggle;
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "ww":
                    case "waterwalk":
                        {
                            props.WaterWalk = toggle;
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "g":
                    case "gills":
                        {
                            props.Gills = toggle;
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "os":
                    case "obsidianskin":
                        {
                            props.ObsidianSkin = toggle;
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "effects":
                    case "buffs":
                        {
                            if (toggle)
                                props.AllOn(true);
                            else
                                props.AllOff(true);
                            plr.SendToggleMessage(prop, toggle);
                            return;
                        }
                    case "on":
                        {
                            props.AllOn(false);
                            plr.SendToggleMessage("CreativeMode", true);
                            return;
                        }
                    case "off":
                        {
                            props.AllOff(false);
                            plr.SendToggleMessage("CreativeMode", false);
                            return;
                        }
                    default:
                        plr.SendErrorMessage("Invalid property name: {0}.", prop);
                        plr.SendErrorMessage("Available properties: shine, nightowl, builder, mining, panic, waterwalk, gills, obsidianskin, buffs, endless, on, off");
                        return;


                }
            }

        }

        public void GetData(GetDataEventArgs e)
        {
            var plr = TShock.Players[e.Msg.whoAmI];
            if (plr.GetProperties().Endless == false)
                return;

            if (plr.Group.HasPermission("creativemode.*") || plr.Group.HasPermission("creativemode.tiles"))
            {
                if (e.MsgID == PacketTypes.Tile)
                {
                    #region Modify Tile (0x11) [17]
                    Int32 Length = e.Msg.readBuffer.Length;

                    Byte type; //Action
                    Int16 x, y;  //Tile X & Y
                    UInt16 tileType;
                    Byte style; //Var2
                    using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                    {
                        using (var reader = new BinaryReader(data))
                        {
                            try
                            {
                                type = reader.ReadByte();
                                x = reader.ReadInt16();
                                y = reader.ReadInt16();
                                if (x >= 0 && y >= 0 && x < Main.maxTilesX && y < Main.maxTilesY)
                                {
                                    int count = 0;
                                    Item giveItem = null;
                                    switch (type)
                                    {
                                        case 1:
                                            #region PlaceTile
                                            {
                                                bool wand = false;
                                                int itemWand = -1;
                                                tileType = reader.ReadUInt16();//createTile //Special cases for 191, 192 (Wood), 194 (Bone if item.type is not 766), 225 (Hive) //Check .tileWand
                                                style = reader.ReadByte();//placeStyle
                                                foreach (Item item in plr.TPlayer.inventory)
                                                {
                                                    if (item.type != 0 && item.createTile == tileType && item.placeStyle == style)
                                                    {
                                                        if (item.tileWand != -1)
                                                        {
                                                            wand = true;
                                                            itemWand = item.tileWand;
                                                            break;
                                                        }
                                                        count += item.stack;
                                                        giveItem = item;
                                                    }
                                                }
                                                if (wand)
                                                {
                                                    count = 0;
                                                    foreach (Item item in plr.TPlayer.inventory)
                                                    {
                                                        if (item.type == itemWand)
                                                        {
                                                            count += item.stack;
                                                            giveItem = item;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 3:
                                            #region PlaceWall
                                            {
                                                tileType = reader.ReadUInt16();//createWall
                                                foreach (Item item in plr.TPlayer.inventory)
                                                {
                                                    if (item.type != 0 && item.createWall == tileType)
                                                    {
                                                        count += item.stack;
                                                        giveItem = item;
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 8:
                                            #region PlaceActuator
                                            {
                                                foreach (Item item in plr.TPlayer.inventory)
                                                {
                                                    if (item.type == 849)
                                                    {
                                                        count += item.stack;
                                                        giveItem = item;
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 5:
                                        case 10:
                                        case 12:
                                            #region PlaceWire*
                                            {
                                                foreach (Item item in plr.TPlayer.inventory)
                                                {
                                                    if (item.type == 530)
                                                    {
                                                        count += item.stack;
                                                        giveItem = item;
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                    }
                                    if (count < 10 && giveItem != null)
                                    {
                                        if (Config.contents.EnableWhitelist && !WhiteList.Contains(giveItem.netID))
                                        {
                                            return;
                                        }
                                        else if (Config.contents.EnableBlacklist && BlackList.Contains(giveItem.netID))
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            plr.GiveItemCheck(giveItem.type, giveItem.Name, giveItem.maxStack - 10);
                                            return;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                TShock.Log.ConsoleError("Failed to read ({0}/16) Packet details of {1}: {2}", Length, ex.ToString(), ex.StackTrace);
                                return;
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                    #endregion
                    return;
                }
            }

            if (plr.Group.HasPermission("creativemode.*") || plr.Group.HasPermission("creativemode.paint"))
            {
                if (e.MsgID == PacketTypes.PaintTile || e.MsgID == PacketTypes.PaintWall)
                {
                    #region Paint Tile (0x3F) [63] & Paint Wall (0x40) [64]
                    Int32 Length = e.Msg.readBuffer.Length;
                    Int16 x, y;
                    Byte color; //type
                    #region Read data
                    using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                    {
                        using (var reader = new BinaryReader(data))
                        {
                            try
                            {
                                x = reader.ReadInt16();
                                y = reader.ReadInt16();
                                color = reader.ReadByte();
                            }
                            catch (Exception ex)
                            {
                                TShock.Log.ConsoleError("Failed to read Packet details of {0}: {1}", ex.ToString(), ex.StackTrace);
                                return;
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                    #endregion
                    if (x >= 0 && y >= 0 && x < Main.maxTilesX && y < Main.maxTilesY)
                    {
                        int count = 0;
                        Item giveItem = null;
                        foreach (Item item in TShock.Players[e.Msg.whoAmI].TPlayer.inventory)
                        {
                            if (item.type != 0 && item.paint == color)
                            {
                                count += item.stack;
                                giveItem = item;
                            }
                        }
                        if (count < 10 && giveItem != null)
                        {
                            if (Config.contents.EnableWhitelist && !WhiteList.Contains(giveItem.netID))
                            {
                                return;
                            }
                            else if (Config.contents.EnableBlacklist && BlackList.Contains(giveItem.netID))
                            {
                                return;
                            }
                            else
                            {
                                //TShock.Players[e.Msg.whoAmI].GiveItem(giveItem.type, giveItem.Name, giveItem.width, giveItem.height, giveItem.maxStack - 10);
                                TShock.Players[e.Msg.whoAmI].GiveItemCheck(giveItem.type, giveItem.Name, giveItem.maxStack - 10);
                                return;
                            }
                        }
                    }
                    #endregion
                    return;
                }
            }

            //Reference to DarkUnderdog's InfiniteAmmo code - pastebin.com/7wFyUD5X
            //*if (plr.Group.HasPermission("creativemode.*") || plr.Group.HasPermission("creativemode.ammo"))

            {
                if (e.MsgID == PacketTypes.ProjectileNew)
                {
                    foreach (Item item in plr.TPlayer.inventory)
                    {
                        switch (item.ammo)
                        {
                            case 1:
                            case 14:
                            case 15:
                            case 23:
                            case 71:
                            case 246:
                            case 311:
                            case 323:
                            case 514:
                            case 949:
                                Item giveItem = item;
                                if (item.stack < 10 && giveItem != null)
                                {
                                    plr.GiveItem(giveItem.type, giveItem.maxStack - 10);
                                }
                                break;
                        }
                    }
                }
                return;
            }

        }
    }
}
