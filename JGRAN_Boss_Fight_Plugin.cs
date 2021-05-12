using System;
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
                "/setarena remove - removes arena platforms if it has been set\n"+
                "/setarena help - show help information"
            });
        }

        string getArena()
        {
            return (string)Properties.Settings.Default["Arena"];
        }

        void saveArena(string arena)
        {
            Properties.Settings.Default["Arena"] = arena;
            Properties.Settings.Default.Save();
        }

        void removeArena()
        {
            Properties.Settings.Default["Arena"] = "0";
            Properties.Settings.Default.Save();
        }

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
                case "remove":
                    if (getArena() != "0")
                    {
                        TShockAPI.DB.Region _region = TShock.Regions.GetRegionByName(getArena());
                        _removeArena(_region.Area.Location.X, _region.Area.Location.Y, _region.Area.Width, _region.Area.Height, () => {
                            removeArena();
                            args.Player.SendSuccessMessage("Arena has been removed successfully");
                        });
                    }
                    else
                        args.Player.SendWarningMessage("Arena is not yet set");
                    break;
                case "this":
                    TShockAPI.DB.Region region_ = args.Player.CurrentRegion;
                    if (region_ != null)
                    {
                        if (region_.Owner == args.Player.Name)
                        {
                            if(getArena() != "0")
                            {
                                _removeArena(region_.Area.Location.X, region_.Area.Location.Y, region_.Area.Width, region_.Area.Height, () => {
                                    removeArena();
                                });
                            }
                            _generateArena(region_.Area.Location.X, region_.Area.Location.Y, region_.Area.Width, region_.Area.Height, () => {
                                saveArena(region_.Name);
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
                    if (getArena() == "0")
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
                        if (args.Player.CurrentRegion.Name == getArena())
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
                            if (getArena() != "0")
                            {
                                _removeArena(region.Area.Location.X, region.Area.Location.Y, region.Area.Width, region.Area.Height, () => {
                                    removeArena();
                                });
                            }
                            _generateArena(region.Area.Location.X, region.Area.Location.Y, region.Area.Width, region.Area.Height, ()=> {
                                saveArena(token);
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
            if (args.Npc.boss && getArena() != "0")
            {
                Point point = new Point();
                point.X = (int) args.Npc.position.ToTileCoordinates().X;
                point.Y = (int) args.Npc.position.ToTileCoordinates().Y;

                TShockAPI.DB.Region region = TShock.Regions.GetRegionByName(getArena());

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
                        //Main.tile[i, j].wall = 0;
                        Main.tile[i, j].wall = 1;
                    }
                }
                for(int j=y; j<=y+h; j++)
                {
                    for (int i = x; i <= x + w; i++)
                    {
                        // 19 or 94
                        if (j % 4 == 0) {
                            Main.tile[i, j].active(true);
                            Main.tile[i, j].frameX = -1;
                            Main.tile[i, j].frameY = -1;
                            Main.tile[i, j].lava(false);
                            Main.tile[i, j].liquid = 0;
                            Main.tile[i, j].type = 19;
                            //Main.tile[i, j].frameNumber(94);
                        }
                    }
                }
            });
        }

        void _generateArena(int x, int y, int w, int h, Action callback)
        {
            for (int i = x; i <= x + w; i++)
            {
                for (int j = y; j <= y + h; j++)
                {
                    if(i % 4 == 0)
                    {
                        Main.tile[i, j] = new Tile();
                        //Main.tile[i, j].wall = 0;
                        Main.tile[i, j].wall = 1;
                    }
                }  
            }
            for (int j = y; j <= y + h; j++)
            {
                for (int i = x; i <= x + w; i++)
                {
                    // 19 or 94
                    if (j % 4 == 0)
                    {
                        Main.tile[i, j].active(true);
                        Main.tile[i, j].frameX = -1;
                        Main.tile[i, j].frameY = -1;
                        Main.tile[i, j].lava(false);
                        Main.tile[i, j].liquid = 0;
                        Main.tile[i, j].type = 19;
                        //Main.tile[i, j].frameNumber(94);
                    }
                }
            }
            callback();
        }

        void _removeArena(int x, int y, int w, int h, Action callback)
        {
            for (int i = x; i <= x + w; i++)
            {
                for (int j = y; j <= y + h; j++)
                {
                    Main.tile[i, j] = new Tile();
                }
            }
            callback();
        }
    }
}
