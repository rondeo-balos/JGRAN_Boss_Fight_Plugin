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
            Commands.ChatCommands.Add(new Command("tshock.admin.bossfight", bossFightCommand, "bossfight")
            {
                HelpText = "JGRAN Reward Plugin"
            });
        }

        float[] bosspos = null;
        int radius = 40000;

        const int X = 0;
        const int Y = 1;

        void bossFightCommand(CommandArgs args)
        {
            var token = args.Parameters[0];
            switch (token)
            {
                case "position":
                    // bossfight position
                    bosspos = new float[] { args.Player.LastNetPosition.X, args.Player.LastNetPosition.Y };
                    args.Player.SendSuccessMessage($"Boss Fight position has been set to [{bosspos[0]},{bosspos[1]}]!");
                    break;
                case "radius":
                    //bossfight radius 50000
                    if (Int32.TryParse(args.Parameters[1], out radius))
                    {
                        args.Player.SendSuccessMessage($"Boss Fight radius has been set to {radius}!");
                    }
                    break;
                case "check":
                    if (bosspos != null)
                    {
                        if ((1 - radius) * (1 - radius) <= (args.Player.LastNetPosition.X - bosspos[X]) * (args.Player.LastNetPosition.X - bosspos[X]))
                            args.Player.SendSuccessMessage("Your safe in this place!");
                        else
                            args.Player.SendErrorMessage("Fight for your death!");
                    }
                    else
                        args.Player.SendErrorMessage("Bossfight Region has not been set!");
                        break;
            }
        }

        void onNPCAIUpdate(NpcAiUpdateEventArgs args)
        {
            if (args.Npc.boss && bosspos != null)
            {
                //Console.WriteLine($"boss position: [{args.Npc.position.X},{args.Npc.position.Y}] region position: [{bosspos[X]},{bosspos[Y]}]");
                if((1-radius)* (1 - radius) <= (args.Npc.position.X - bosspos[X])*(args.Npc.position.X - bosspos[X]))
                {
                    Console.WriteLine("Boss out of range");
                    args.Npc.Teleport(new Microsoft.Xna.Framework.Vector2(bosspos[X], bosspos[Y]));
                    //args.Npc.DirectionTo(new Microsoft.Xna.Framework.Vector2(bosspos[X],bosspos[Y]));
                    //args.Npc.AIDirect();
                }
            }
        }
    }
}
