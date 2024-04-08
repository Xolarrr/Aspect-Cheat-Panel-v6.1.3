using UnityEngine;
using UnityEngine.UI;

namespace Aspect.MenuLib
{
    public static class Board
    {
        public static void SetBoardText(string title, string content)
        {
            // dont set text if the board doesn't exist
            if (GameObject.Find("COC Text") == null) return;

            //set text
            GameObject boardTitle = GameObject.Find("CodeOfConduct");
            boardTitle.GetComponent<Text>().text = "[<color=yellow>" + title + "</color>]";

            GameObject board = GameObject.Find("COC Text");
            board.GetComponent<Text>().text = content;

            RectTransform component = board.GetComponent<RectTransform>();
            component.sizeDelta = new Vector2(68.8744f, 177.5f);
            component.localPosition = new Vector3(-56.2008f, -63f, 0.0002f);
        }

        public static void SetBoardColor(Color color1, Color color2)
        {
            // include all screen in the game asap - done (mostly)
            string[] screens = {
                "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/StaticUnlit/motdscreen",
                "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/StaticUnlit/screen",
                "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/Wall Monitors Screens/wallmonitorcanyon",
                "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/Wall Monitors Screens/wallmonitorcosmetics",
                "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/Wall Monitors Screens/wallmonitorcave",
                "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/Wall Monitors Screens/wallmonitorforest",
                "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/Wall Monitors Screens/wallmonitorskyjungle",
                "Environment Objects/LocalObjects_Prefab/Forest/Terrain/campgroundstructure/scoreboard/REMOVE board"
            };
            for (int i = 0; i < screens.Length; i++)
            {
                // set custom material
                Material material = new Material(Shader.Find("Standard"));
                GameObject.Find(screens[i]).GetComponent<Renderer>().material = material;

                //initialize colorchanger
                Menu.ColorChanger colorChanger = GameObject.Find(screens[i]).AddComponent<Menu.ColorChanger>();
                colorChanger.Color1 = color1;
                colorChanger.Color2 = color2;
            }
        }
    }
}