using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Aspect.Utilities
{
    public static class Util
    {
        public static string GenRandomString(int length)
        {
            // there is probably a better way - but idrc
            string finalString = "";
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";

            for (int i = 0; i < length; i++)
            {
                char caracterAtRandomIndex = letters[Random.Range(1, letters.Length)];
                finalString += caracterAtRandomIndex;
            }

            return finalString;
        }
    }

    public static class RigManager
    {
        // material names
        public static string it = "It";
        public static string infected = "infected";
        public static string casual = "gorilla_body(Clone)";
        public static string hunted = "ice";
        public static string bluealive = "bluealive";
        public static string bluestunned = "bluestunned";
        public static string bluehit = "bluehit";
        public static string paintsplatterblue = "paintsplattersmallblue";
        public static string orangealive = "orangealive";
        public static string orangestunned = "orangestunned";
        public static string orangehit = "orangehit";
        public static string paintsplatterorange = "paintsplattersmallorange";
        public static string sodainfected = "SodaInfected";

        public static PhotonView VRRigToPhotonView(VRRig rig)
        {
            return (PhotonView)Traverse.Create(rig).Field("photonView").GetValue();
        }

        public static bool IsTagged(VRRig rig)
        {
            return rig.mainSkin.material.name.Contains(it) || rig.mainSkin.material.name.Contains(infected);
        }
    }
}
