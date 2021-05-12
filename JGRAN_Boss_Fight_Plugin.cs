using System;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

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

        int[] arena = null;
        const int X = 0, Y = 1, W = 2, H = 3;

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
                case "check":
                    if (arena == null)
                    {
                        args.Player.SendWarningMessage("arena not set");
                        break;
                    }
                    args.Player.SendSuccessMessage($"arena: {arena}");
                    if (!checkRectanglePoint(args.Player.LastNetPosition.X, args.Player.LastNetPosition.Y, arena[X], arena[Y], arena[W], arena[H]))
                        args.Player.SendWarningMessage("Out of range");
                    else
                        args.Player.SendSuccessMessage("Good");
                    break;
                case "help":
                    args.Player.SendInfoMessage("/setarena <regionname> - setting the arena region for bossfight\n" +
                        "/setarena check - checks if the region has been set\n" +
                        "/setarena help - show help information");
                    break;
                default:
                    try
                    {
                        arena = getPlayerRegion(token, args.Player.Name);
                        args.Player.SendSuccessMessage("Arena has been set successfully");
                    }catch(Exception err)
                    {
                        args.Player.SendErrorMessage("Please make sure you have a region before using this command");
                        args.Player.SendWarningMessage(err.Message);
                    }
                    break;
            }
        }

        void onNPCAIUpdate(NpcAiUpdateEventArgs args)
        {
            if (args.Npc.boss && arena != null)
            {
                if (!checkRectanglePoint(args.Npc.position.X, args.Npc.position.Y, arena[X], arena[Y], arena[W], arena[H]))
                {
                    Console.WriteLine("boss out of range");
                    /*int centerX = arena[X] + (arena[W] / 2);
                    int centerY = arena[Y] + (arena[H] / 2);
                    args.Npc.Teleport(new Microsoft.Xna.Framework.Vector2(centerX,centerY));*/
                }
            }
        }

        int[] getPlayerRegion(string region, string owner)
        {
            TShock.DB.Open();
            System.Data.IDbCommand cmd = TShock.DB.CreateCommand();
            string cmdText = "SELECT X1, Y1, width, height FROM Regions WHERE RegionName = '" + region + "' AND Owner = '" + owner + "' AND WorldID = '" + Main.worldID + "'";
            cmd.CommandText = cmdText;
            System.Data.IDataReader reader = cmd.ExecuteReader();
            reader.Read();
            int[] retval = { reader.GetInt32(X), reader.GetInt32(Y), reader.GetInt32(W), reader.GetInt32(H) };
            reader.Dispose();
            cmd.Dispose();
            TShock.DB.Close();
            return retval;
        }

        bool checkRectanglePoint(float px, float py, int rx, int ry, int rw, int rh)
        {
            return px >= rx && // left boundary
                 px <= rx + rw && // right boundary
                 py >= ry && // upper boundary
                 py <= ry + rh; // lower boundary
        }
    }
}
