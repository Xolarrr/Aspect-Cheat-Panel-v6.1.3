using BepInEx;
using HarmonyLib;
using System.Net;
using System.Reflection;
using Utilla;

namespace Aspect.Plugin
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla 1.5.0 as a dependency!
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {
        // plugin data
        private const string modGUID = "menulib";
        public const string modVersion = "6.1.3";
        private const string modName = "Aspect - Cheat Panel v6.1.3";

        private static Harmony harmony;
        public static bool Patched { get; private set; }

        // load harmony
        private void OnEnable()
        {
            if (!Patched)
            {
                if (harmony == null)
                {
                    harmony = new Harmony(modGUID);
                }
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Patched = true;
            }
        }

        // unload harmony
        private void OnDisable()
        {
            if (harmony != null && Patched)
            {
                harmony.UnpatchSelf();
                Patched = false;
            }
        }

        // Utilla Stuff
        public static bool inAllowedRoom = false;

        [ModdedGamemodeJoin]
        private void RoomJoined(string gamemode)
        {
            // The room is modded. Enable mod stuff.
            inAllowedRoom = true;
        }

        [ModdedGamemodeLeave]
        private void RoomLeft(string gamemode)
        {
            // The room was left. Disable mod stuff.
            inAllowedRoom = false;
        }
    }
}
