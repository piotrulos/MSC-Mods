#if !Mini
using MSCLoader;
using UnityEngine;

namespace ModsShop
{
    public class DebugCmd : ConsoleCommand
    {
        // What the player has to type into the console to execute your commnad
        public override string Name => "shopDebug";

        // The help that's displayed for your command when typing help
        public override string Help => "Command Description";
        public override bool ShowInHelp => false;


        public override void Run(string[] args)
        {
            if(ModLoader.CurrentScene == CurrentScene.Game)
            {
                switch (args[0])
                {
                    case "tp":
                        GameObject.Find("PLAYER").transform.position = new Vector3(-1511.606f, 4f, 1234.105f);
                        GameObject.Find("PLAYER").transform.eulerAngles = new Vector3(0f, 332f, 0f);
                        break;
                    case "list":

                        break;
                    default:
                        ModConsole.Error("Invalid syntax");
                        break;
                }
            }
        }

    }
}
#endif