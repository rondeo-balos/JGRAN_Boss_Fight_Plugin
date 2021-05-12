﻿using System;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace JGRAN_Boss_Fight_Plugin
{
    [ApiVersion(2, 1)]
    public class JGRAN_Boss_Fight_Plugin : TerrariaPlugin
    {
        public override string Author => "Rondeo Balos";
        public override string Description => "A Boss Fight plugin";
        public override string Name => "JGRAN Boss Fight Plugin";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public JGRAN_Boss_Fight_Plugin(Main game) : base(game) { }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, onInit);
            ServerApi.Hooks.NpcAIUpdate.Register(this, onNPCAIUpdate);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, onInit);
                ServerApi.Hooks.NpcAIUpdate.Deregister(this, onNPCAIUpdate);
            }
            base.Dispose(disposing);
        }

        void onInit(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("bossfight.setarena", setArena, "setarena")
            {
                HelpText = "Before using this command, make sure to use the House Region plugin and you must have a valid Region.\n\n"+
                "/setarena regionname - setting the arena region for bossfight\n" +
                "/setarena check - checks if the region has been set\n"+
                "/setarena help - show help information"
            });
        }

        string arena = null;

        void setArena(CommandArgs args)
        {

            if(args.Parameters.Count <= 0)
            {
                args.Player.SendWarningMessage("type '/setarena help' - for more info");
                return;
            }
            var token = args.Parameters[0];
            switch (token)
            {
                case "this":
                    TShockAPI.DB.Region region_ = args.Player.CurrentRegion;
                    if (region_ != null)
                    {
                        if (region_.Owner == args.Player.Name)
                        {
                            generateArena(region_.Area.Location.X, region_.Area.Location.Y, region_.Area.Width, region_.Area.Height).ContinueWith((d) => {
                                TShock.Utils.SaveWorld();
                                arena = region_.Name;
                                args.Player.SendSuccessMessage("Arena has been set successfully");
                            });
                        }
                        else
                            args.Player.SendWarningMessage("This is not your region man!");
                    }
                    else
                        args.Player.SendErrorMessage("You are not currently standing in a region.");
                    break;
                case "check":
                    if (arena == null)
                    {
                        args.Player.SendWarningMessage("arena not set");
                        if (args.Player.CurrentRegion != null)
                        {
                            if(args.Player.CurrentRegion.Owner == args.Player.Name)
                                args.Player.SendInfoMessage($"but you are currently standing in [{args.Player.CurrentRegion.Name}] region. If you want to set this region just type /setarena this");
                        }
                        break;
                    }
                    
                    if (args.Player.CurrentRegion != null)
                    {
                        args.Player.SendInfoMessage($"Region: {args.Player.CurrentRegion.Name}");
                        if (args.Player.CurrentRegion.Name == arena)
                            args.Player.SendInfoMessage("You're inside the arena");
                        else
                            args.Player.SendWarningMessage("You're outside the arena");
                        
                    }else
                        args.Player.SendWarningMessage("Your not in a Region");
                    break;
                case "help":
                    args.Player.SendInfoMessage("Before using this command, make sure to use the House Region plugin and you must have a valid Region.\n\n" + 
                        "/setarena <regionname> - setting the arena region for bossfight\n" +
                        "/setarena check - checks if the region has been set\n" +
                        "/setarena help - show help information");
                    break;
                default:
                    TShockAPI.DB.Region region = TShock.Regions.GetRegionByName(token);
                    if (region != null)
                    {
                        if (region.Owner == args.Player.Name)
                        {
                            generateArena(region.Area.Location.X, region.Area.Location.Y, region.Area.Width, region.Area.Height).ContinueWith((d) => {
                                TShock.Utils.SaveWorld();
                                arena = token;
                                args.Player.SendSuccessMessage("Arena has been set successfully");
                            });
                        }
                        else
                            args.Player.SendWarningMessage($"Region [{token}] is not yours");
                    }
                    else
                        args.Player.SendErrorMessage($"Region [{token}] not found, please create a region using /house def");
                    break;
            }
        }

        void onNPCAIUpdate(NpcAiUpdateEventArgs args)
        {
            if (args.Npc.boss && arena != null)
            {
                Point point = new Point();
                point.X = (int) args.Npc.position.ToTileCoordinates().X;
                point.Y = (int) args.Npc.position.ToTileCoordinates().Y;

                TShockAPI.DB.Region region = TShock.Regions.GetRegionByName(arena);

                if (!region.Area.Contains(point))
                {
                    TSPlayer.All.SendErrorMessage("Boss out of range");
                    args.Npc.Teleport(region.Area.Center.ToWorldCoordinates());
                    //args.Npc.DirectionTo(region.Area.Center.ToWorldCoordinates());
                }
            }
        }

        Task generateArena(int x, int y, int w, int h)
        {
            return Task.Run(()=> {
                for (int i=x; i<=x+w; i++)
                {
                    for (int j=y; j<=y+h; j++)
                    {
                        Main.tile[i, j] = new Tile();
                        Main.tile[i, j].wall = 0;
                        Main.tile[i, j].wall = 1;
                    }
                }
                for(int j=y; j<=y+h; j++)
                {
                    for (int i = x; i <= x + w; i++)
                    {
                        if (j % 2 == 0)
                            Main.tile[i, j].type = 94;
                            //Main.tile[i, j] = 94;
                    }
                }
            });
        }
    }
}
