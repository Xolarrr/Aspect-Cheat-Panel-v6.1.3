using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using GorillaLocomotion;
using System.IO;
using HarmonyLib;
using Aspect.Utilities;
using Photon.Pun;
using GorillaNetworking;

namespace Aspect.MenuLib
{
    // Update via GorillaLocomotion.Player patch
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    internal static class Update
    {
        // sets board text depending on the time of the day
        public static string GetBoardText(string text = "")
        {
            string[] time = DateTime.Now.ToString("hh.tt").Split('.');
            if (time[1] == "PM")
            {
                if (int.Parse(time[0]) >= 6)
                {
                    return $"Good evening! Thanks for choosing aspects. {text}";
                }
                else
                {
                    return $"Good afternoon! Thanks for choosing aspects. {text}";
                }
            }
            else
            {
                if (int.Parse(time[0]) >= 6)
                {
                    return $"Good morning! Thanks for choosing aspects. {text}";
                }
                else
                {
                    return $"Good night! Thanks for choosing aspects. {text}";
                }
            }
        }

        // font(s)
        public static Font menuTitleFont { get; private set; }
        public static Font menuButtonFont { get; private set; }

        // menu variables
        public static Menu.MenuTemplate menu;
        static bool isSetup = false;

        static void Prefix(Player __instance)
        {
            // setup
            if (!isSetup)
            {
                // initialize fonts
                menuTitleFont = Font.CreateDynamicFontFromOSFont("Agency FB", 24);
                menuButtonFont = Font.CreateDynamicFontFromOSFont("Agency FB", 20);

                // create menu
                menu = Menu.MenuTemplate.CreateMenu(
                    $"{Plugin.Plugin.modVersion}",
                    Color.white,
                    new Vector3(0.1f, 1f, 1f),
                    __instance.leftControllerTransform.gameObject,
                    true
                );

                // make menubuttons

                /* Add this to create an empty button
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "" });
                */
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Turn Off NotifiLib", OnUpdate = () => NotifiLib.ClearAllNotifications(), Description = "Disables incoming notifications." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "ESP", OnUpdate = () => GorillaMods.ESP(), OnDisable = () => GorillaMods.ESP(true), Description = "Makes you see players through walls, and if you press left grip it adds tracers." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Tag Gun", OnUpdate = () => GorillaMods.TagGun(), OnDisable = () => GorillaExtensions.GunTemplate(true), Description = "Use grip to aim and trigger to shoot. It's way better when you have master!" });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Tag All", OnUpdate = () => GorillaMods.TagAll(), Extensions = "MASTER", Toggle = false, Description = "Instantly tags everyone in an infectionlobby when you have master." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Slow RGB", OnUpdate = () => GorillaMods.RGB(), Extensions = "STUMP", Description = "Changes your color slowly." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Fast RGB", OnUpdate = () => GorillaMods.RGB(0.05f), Extensions = "STUMP", Description = "Changes your color quickly." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Platforms", OnUpdate = () => GorillaMods.Platforms(), OnDisable = () => GorillaMods.Platforms(true), Description = "Use grips to activate platforms." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Sticky Platforms", OnUpdate = () => GorillaMods.Platforms(false, true), OnDisable = () => GorillaMods.Platforms(true), Description = "Use grips to activate platforms." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Climb Anywhere", OnUpdate = () => GorillaMods.ClimbAnywhere(), OnDisable = () => GorillaMods.ClimbAnywhere(true), Description = "Use grips while touching a surface to activate climbing." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Iron Monkey", OnUpdate = () => GorillaMods.IronMonkey(10f), Description = "Use Primary-buttons to fly like you have a jetpack." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Super Monkey", OnUpdate = () => GorillaMods.SuperMonkey(), OnDisable = () => GorillaMods.SuperMonkey(false), Description = "Seconday to fly, Primary to activate low gravity." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "No-Clip", OnUpdate = () => GorillaMods.NoClip(), OnDisable = () => GorillaMods.NoClip(false), Description = "Press trigger to turn off colliders." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Low Gravity", OnUpdate = () => GorillaMods.ChangeGravity(false), OnDisable = () => GorillaMods.ChangeGravity(true), Description = "Sets low gravity." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "High Gravity", OnUpdate = () => GorillaMods.ChangeGravity(false, 15f), OnDisable = () => GorillaMods.ChangeGravity(true), Description = "Sets high gravity." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Ghost Monkey", OnUpdate = () => GorillaMods.GhostMonkey(), OnDisable = () => GorillaMods.GhostMonkey(false), Description = "Secondary to go out of your rig." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Invisibility", OnUpdate = () => GorillaMods.Invisibility(), OnDisable = () => GorillaMods.Invisibility(false), Description = "Secondary to go invisible." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Teleport Gun", OnUpdate = () => GorillaMods.TeleportGun(), OnDisable = () => GorillaExtensions.GunTemplate(true), Description = "Grip to aim, trigger to shoot." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Disable Quitbox", OnUpdate = () => GorillaMods.DisableQuitbox(true), Toggle = false, Description = "Disables quitbox." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Projectile Teleport", OnUpdate = () => GorillaMods.ProjectileTeleport(false), Description = "Throw/shoot a projectile to teleport." });
                menu.Buttons.Add(new Menu.ButtonTemplate { Text = "Ride Projectile", OnUpdate = () => GorillaMods.ProjectileTeleport(true), Description = "Throw/shoot a projectile to ride it." });
                menu.OriginalButtons = menu.Buttons;

                // setup custom board
                Board.SetBoardText($"Aspect Cheat Panel {Plugin.Plugin.modVersion}", GetBoardText($"There is a total of {menu.OriginalButtons.Count} mods in this menu. Holding left-grip while pressing a button favorites and pins it to start."));
                Board.SetBoardColor(new Color32(85, 15, 150, 1), new Color32(125, 15, 200, 1));

                // close menu setup
                isSetup = true;
            }

            // update
            Menu.CallUpdate(Input.instance.CheckButton(Input.ButtonType.secondary, menu.LeftHand), menu);
            Menu.UpdateToggledMods(menu);
        }
    }

    // Base menu library
    public class Menu
    {
        public static void CallUpdate(bool StateDepender, MenuTemplate Menu)
        {
            if (!StateDepender && Menu.MenuRoot != null)
            {
                // destroy menu reference
                GameObject.Destroy(Menu.Reference);

                // Create menuroot rigidbody
                bool loadOnce = false;
                try
                {   
                    if (!loadOnce)
                    {
                        Rigidbody menuRB = Menu.MenuRoot.AddComponent<Rigidbody>();
                        foreach (Collider collider in Menu.MenuRoot.GetComponentsInChildren<Collider>())
                        {
                            GameObject.Destroy(collider);
                        }
                        if (Menu.LeftHand)
                        {
                            menuRB.velocity = Player.Instance.leftHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                            menuRB.angularVelocity = GameObject.Find("TurnParent/LeftHand Controller").GetComponent<GorillaVelocityEstimator>().angularVelocity;
                        }
                        else
                        {
                            menuRB.velocity = Player.Instance.rightHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                            menuRB.angularVelocity = GameObject.Find("TurnParent/RightHand Controller").GetComponent<GorillaVelocityEstimator>().angularVelocity;
                        }
                        loadOnce = true;
                    }
                }
                catch { }

                // Destroy Menu
                GameObject.Destroy(Menu.MenuRoot, 1);
                Menu.MenuRoot = null;

                return;
            }

            if (Menu.MenuRoot == null && StateDepender)
            {
                Draw(Menu);

                Menu.Reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                GameObject.Destroy(Menu.Reference.GetComponent<Renderer>());
                Menu.Reference.transform.parent = Menu.ReferenceParent;
                Menu.Reference.transform.localPosition = Vector3.zero;

                Menu.Reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                Menu.ReferenceName = Util.GenRandomString(100);
                Menu.Reference.GetComponent<SphereCollider>().gameObject.name = Menu.ReferenceName;
            }
            
            else if (Menu.MenuRoot != null)
            {
                Menu.MenuRoot.transform.position = Menu.Pivot.transform.position;
                Menu.MenuRoot.transform.rotation = Menu.Pivot.transform.rotation;

                if (!Menu.LeftHand) Menu.MenuRoot.transform.RotateAround(Menu.MenuRoot.transform.position, Menu.MenuRoot.transform.forward, 180f);
            }
        }

        public static void UpdateToggledMods(MenuTemplate Menu)
        {
            foreach (ButtonTemplate btn in Menu.Buttons.ToArray())
            {
                if (btn.ButtonState && btn.OnUpdate != null)
                {
                    try
                    {
                        btn.OnUpdate.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        private static void Draw(MenuTemplate Menu)
        {
            // menu root
            Menu.MenuRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(Menu.MenuRoot.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(Menu.MenuRoot.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(Menu.MenuRoot.GetComponent<Renderer>());
            Menu.MenuRoot.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);

            // menu background
            GameObject bgObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(bgObject.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(bgObject.GetComponent<BoxCollider>());
            bgObject.transform.SetParent(Menu.MenuRoot.transform, false);
            bgObject.transform.localScale = Menu.Scale;
            bgObject.transform.position = new Vector3(0.05f, 0f, 0f);
            // fix this please - it works now:)
            ColorChanger colorChanger = bgObject.AddComponent<ColorChanger>();
            colorChanger.Color1 = new Color32(85, 15, 150, 1);
            colorChanger.Color2 = new Color32(125, 15, 200, 1);

            // canvas
            Menu.Canvas = new GameObject();
            Menu.Canvas.transform.parent = Menu.MenuRoot.transform;
            Canvas canvas = Menu.Canvas.AddComponent<Canvas>();
            CanvasScaler canvasScaler = Menu.Canvas.AddComponent<CanvasScaler>();
            Menu.Canvas.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 3000f;

            // text
            GameObject textObj = new GameObject();
            textObj.transform.parent = Menu.Canvas.transform;
            Text text = textObj.AddComponent<Text>();
            text.font = Update.menuTitleFont;
            text.text = Menu.Title + " [" + Menu.currentPage.ToString() + "]";
            text.color = Menu.TitleColor;
            text.fontSize = 1;
            text.fontStyle = FontStyle.BoldAndItalic;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;

            // text rect transform
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.28f, 0.05f);
            text.GetComponent<RectTransform>().position = new Vector3(0.06f, 0f, 0.175f);
            text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            AddPageButtons(Menu);
            ButtonTemplate[] DisconnectButton = { new ButtonTemplate { Text = "Disconnect", OnUpdate = () => PhotonNetwork.Disconnect(), Toggle = false }, new ButtonTemplate { Empty = true }, new ButtonTemplate { Empty = true } };
            ButtonTemplate[] array = DisconnectButton.ToList().Concat(Menu.Buttons.Concat(Menu.FavoritedMods).ToArray().Skip(Menu.currentPage * Menu.ButtonsPerPage).Take(Menu.ButtonsPerPage).ToList()).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                AddButton(Menu.ButtonSpace * i, array[i], Menu);
            }
        }

        private static void AddButton(float Offset, ButtonTemplate Button, MenuTemplate Menu)
        {
            if (Button.Empty == true) return;

            // creates the button object
            GameObject buttonGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(buttonGO.GetComponent<Rigidbody>());
            buttonGO.GetComponent<BoxCollider>().isTrigger = true;
            buttonGO.transform.SetParent(Menu.MenuRoot.transform, false);
            buttonGO.transform.localScale = new Vector3(0.09f, Menu.Scale.y - 0.1f, 0.08f);
            buttonGO.transform.localPosition = new Vector3(0.56f, 0f, 0.67f - Offset);
            buttonGO.AddComponent<ButtonCollider>().button = Button;
            buttonGO.GetComponent<ButtonCollider>().menu = Menu;

            // manages the button colors
            Color targetColor;
            if (!Button.IsFavorited)
            {
                targetColor = Button.IsLabel ? Menu.LabelColor : Button.ButtonState ? Menu.OnColor : Menu.OffColor;
            }
            else
            {
                targetColor = Button.ButtonState ? Menu.FavOnColor : Menu.FavOffColor;
            }
            buttonGO.GetComponent<Renderer>().material.SetColor("_Color", targetColor);

            // creates the text objects
            GameObject textObj = new GameObject();
            textObj.transform.parent = Menu.Canvas.transform;
            Text text = textObj.AddComponent<Text>();
            text.font = Update.menuButtonFont;
            string[] ButtonExtensions = Button.Extensions.Split('.');
            string Extentions = "";
            if (ButtonExtensions.Contains("MASTER"))
            {
                string color = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient ? "green" : "red";
                Extentions += $" [<color={color}>MASTER</color>]";
            }
            if (ButtonExtensions.Contains("STUMP"))
            {
                string color = PhotonNetwork.InRoom && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(RigManager.VRRigToPhotonView(GorillaTagger.Instance.offlineVRRig).Owner.UserId) ? "green" : "red";
                Extentions += $" [<color={color}>STUMP</color>]";
            }
            if (ButtonExtensions.Contains("MODDED"))
            {
                string color = Plugin.Plugin.inAllowedRoom ? "green" : "red";
                Extentions += $" [<color={color}>MODDED</color>]";
            }
            text.text = Button.Text + Extentions;
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Italic;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;

            // initialize the text rect transform
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.2f, 0.03f);
            text.GetComponent<RectTransform>().localPosition = new Vector3(0.064f, 0f, 0.269f - Offset / 2.522522522522523f); // 2.55f is wrong - changed to 2.522522522522523f
            text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        private static void AddPageButtons(MenuTemplate Menu)
        {
            // button variables
            float space = -Menu.ButtonSpace;
            float calculatedSpace = Menu.ButtonSpace * Menu.ButtonsPerPage;
            string ButtonText = "<<<";

            for (int i = 0; i < 2; i++)
            {
                space += Menu.ButtonSpace;

                // creates the button object
                GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(button.GetComponent<Rigidbody>());
                button.GetComponent<BoxCollider>().isTrigger = true;
                button.transform.SetParent(Menu.MenuRoot.transform, false);
                button.transform.localScale = new Vector3(0.09f, Menu.Scale.y - 0.1f, 0.08f);
                button.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - calculatedSpace);
                button.GetComponent<Renderer>().material.SetColor("_Color", Menu.PagebuttonColor);
                button.AddComponent<ButtonCollider>().button = new ButtonTemplate { Text = ButtonText, Toggle = false };
                button.GetComponent<ButtonCollider>().menu = Menu;

                // creates the text objects
                GameObject textObj = new GameObject();
                textObj.transform.parent = Menu.Canvas.transform;
                Text text = textObj.AddComponent<Text>();
                text.font = Update.menuButtonFont;
                text.text = ButtonText;
                text.fontSize = 1;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontStyle = FontStyle.Italic;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;

                // initialize the text rect transform
                text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.2f, 0.03f);
                text.GetComponent<RectTransform>().localPosition = new Vector3(0.064f, 0f, 0.111f - calculatedSpace / 2.522522522522523f);
                text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                ButtonText = ">>>";
                calculatedSpace = Menu.ButtonSpace * (Menu.ButtonsPerPage + 1);
            }
        }

        private static void RefreshMenu(MenuTemplate Menu)
        {
            UnityEngine.Object.Destroy(Menu.MenuRoot);
            Menu.MenuRoot = null;
            UnityEngine.Object.Destroy(Menu.Reference);
            Menu.Reference = null;
        }

        public class ButtonTemplate
        {
            // Text that shows on the button
            public string Text { get; set; } = "";

            // Put mod Description here
            public string Description { get; set; } = "";

            // If this is turned on the menu skips the button so it becomes an empty slot
            public bool Empty { get; set; } = false;

            /*
                OnEnable runs one time when the button gets enabled.
                OnUpdate runs each frame when the button is enabled.
                OnDisable runs one time when you turn off the button.
            */
            public Action OnEnable { get; set; } = null;
            public Action OnUpdate { get; set; } = null;
            public Action OnDisable { get; set; } = null;


            // Turn this off if you want your button to run once and not toggle.
            public bool Toggle { get; set; } = true;

            // Make the button into a label
            public bool IsLabel { get; set; } = false;

            // If this is on, the button is favorited and pinned to the start
            public bool IsFavorited { get; set; } = false;

            // Handles if the buttonArray is enabled or not, may not be customized.
            internal bool ButtonState { get; set; } = false;

            // Curretn extentions { MASTER, MODDED, STUMP }
            public string Extensions { get; set; } = "";
        }

        public class MenuTemplate
        {
            // buttons
            internal int ButtonsPerPage = 4;
            public List<ButtonTemplate> Buttons = new List<ButtonTemplate>();
            public List<ButtonTemplate> FavoritedMods = new List<ButtonTemplate>();
            public List<ButtonTemplate> OriginalButtons = new List<ButtonTemplate>();

            // public values
            public string Title;
            public Color TitleColor;
            public Vector3 Scale;
            public GameObject Pivot;
            public bool LeftHand;
            public int TapFavButtonSound = 84;
            public float TapSoundStrength = 0.25f;
            public string CurrentButtonDescription;

            // vars for the menu core
            internal int currentPage = 0;
            internal GameObject MenuRoot = null;
            internal List<GameObject> OldMenuRoots = new List<GameObject>();
            internal GameObject Canvas = null;
            internal GameObject Reference = null;
            internal string ReferenceName;
            internal Transform ReferenceParent;
            internal float ButtonSpace = 0.13f;

            // page colors
            public Color OffColor = new Color32(115, 0, 160, 1);
            public Color OnColor = new Color32(70, 0, 100, 1);
            public Color FavOffColor = new Color32(240, 170, 50, 1);
            public Color FavOnColor = new Color32(175, 130, 30, 1);
            public Color LabelColor = Color.grey;
            public Color PagebuttonColor = new Color32(115, 0, 160, 1);

            // returns a new menu instance
            public static MenuTemplate CreateMenu(string title, Color titleColor, Vector3 scale, GameObject pivot, bool leftHand)
            {
                MenuTemplate template = new MenuTemplate();

                template.Title = title;
                template.TitleColor = titleColor;
                template.Scale = scale;
                template.Pivot = pivot;
                template.LeftHand = leftHand;

                if (leftHand) template.ReferenceParent = GorillaTagger.Instance.rightHandTriggerCollider.transform;

                else template.ReferenceParent = GorillaTagger.Instance.leftHandTriggerCollider.transform;

                return template;
            }

            // switch hands
            public void SwitchHands()
            {
                if (LeftHand)
                {
                    Pivot = Player.Instance.rightControllerTransform.gameObject;
                    this.ReferenceParent = GorillaTagger.Instance.leftHandTriggerCollider.transform;
                    LeftHand = false;
                    RefreshMenu(this);
                } else
                {
                    Pivot = Player.Instance.leftControllerTransform.gameObject;
                    this.ReferenceParent = GorillaTagger.Instance.rightHandTriggerCollider.transform;
                    LeftHand = true;
                    RefreshMenu(this);
                }
            }

            /* 
                'Config(false);' saves the button data/activations to a file
                'Config(true);' loads the button data/turns on all the saved buttons
            */
            public void Config(bool load, List<ButtonTemplate> buttons = null)
            {
                if (load)
                {
                    try
                    {
                        // set favorite Buttonnames
                        if (!Directory.Exists("Aspect Cheat Panel v6.1.1")) Directory.CreateDirectory("Aspect Cheat Panel v6.1.1");
                        string[] ButtonNamesArray = File.ReadAllText("Aspect Cheat Panel v6.1.1\\buttonactivesontemplate.log").Split('.');

                        foreach (ButtonTemplate button in buttons)
                        {
                            if (ButtonNamesArray.Contains(button.Text))
                            {
                                button.IsFavorited = true;
                                this.FavoritedMods.Add(button);
                                this.Buttons.Remove(button);
                            }
                        }

                        // set buttonstates
                        string[] ButtonActivesArray = File.ReadAllText("Aspect Cheat Panel v6.1.1\\buttonactivesontemplate.log").Split('.');

                        for (int i = 0; i < ButtonActivesArray.Length; i++)
                        {
                            if (ButtonActivesArray[i].Contains("True"))
                            {
                                this.FavoritedMods.Concat(this.Buttons).ToArray()[i].ButtonState = true;
                            }
                            else
                            {
                                this.FavoritedMods.Concat(this.Buttons).ToArray()[i].ButtonState = false;
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        Debug.LogException(e);
                    }
                } else
                {
                    try
                    {
                        if (!Directory.Exists("Aspect Cheat Panel v6.1.1")) Directory.CreateDirectory("Aspect Cheat Panel v6.1.1");
                        // save fav buttonnames
                        if (File.Exists("Aspect Cheat Panel v6.1.1\\buttonnames.log"))
                        {
                            File.Delete("Aspect Cheat Panel v6.1.1\\buttonnames.log");
                        }

                        string ButtonNames = "";
                        foreach (ButtonTemplate btn in this.FavoritedMods.ToArray())
                        {
                            ButtonNames += btn.Text.ToString() + '.';
                        }

                        File.Create("Aspect Cheat Panel v6.1.1\\buttonnames.log");
                        File.WriteAllText("Aspect Cheat Panel v6.1.1\\buttonnames.log", ButtonNames);


                        // save buttonstates
                        if (File.Exists("Aspect Cheat Panel v6.1.1\\buttonactives.log"))
                        {
                            File.Delete("Aspect Cheat Panel v6.1.1\\buttonactives.log");
                        }

                        string ButtonActives = "";
                        foreach (ButtonTemplate btn in this.FavoritedMods.Concat(this.Buttons))
                        {
                            ButtonActives += btn.ButtonState.ToString() + '.';
                        }

                        File.Create("Aspect Cheat Panel v6.1.1\\buttonnames.log");
                        File.WriteAllText("Aspect Cheat Panel v6.1.1\\buttonactives.log", ButtonActives);
                    }
                    catch (IOException e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public class ColorChanger : MonoBehaviour
        {
            public Color Color1 = Color.black;
            public Color Color2 = Color.black;
            public Shader shader = null;

            Gradient gradient = new Gradient();
            Color32 color;

            public void Start()
            {
                // set color and alphakeys 
                var colors = new GradientColorKey[3];
                colors[0] = new GradientColorKey(Color1, 0.0f);
                colors[1] = new GradientColorKey(Color2, 0.5f);
                colors[2] = new GradientColorKey(Color1, 1.0f);
                
                var alphas = new GradientAlphaKey[3];
                alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
                alphas[1] = new GradientAlphaKey(0.5f, 0.5f);
                alphas[2] = new GradientAlphaKey(0.0f, 1.0f);

                gradient.SetKeys(colors, alphas);
            }

            public void Update()
            {
                color = gradient.Evaluate((Time.time / 4) % 1);
                if (base.GetComponent<Renderer>().material.color != color)
                {
                    Material material = new Material(Shader.Find("GorillaTag/UberShader"));
                    base.GetComponent<Renderer>().material = material;
                    base.GetComponent<Renderer>().material.SetColor("_Color", color);
                }
                else
                {
                    base.GetComponent<Renderer>().material.SetColor("_Color", color);
                }
            }
        }

        internal class ButtonCollider : MonoBehaviour
        {
            public ButtonTemplate button;
            public MenuTemplate menu;
            static float PressCooldown = 0;

            public void OnTriggerEnter(Collider collider)
            {
                if (collider.gameObject.name == menu.ReferenceName && !button.IsLabel && Time.frameCount >= PressCooldown + 30f)
                {
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, false, 0.5f);
                    if (button.Text.Contains(">"))
                    {
                        if (menu.currentPage < (menu.Buttons.ToArray().Length + menu.ButtonsPerPage - 1) / menu.ButtonsPerPage - 1)
                        {
                            menu.currentPage++;
                        } else
                        {
                            menu.currentPage = 0;
                        }
                        RefreshMenu(menu);
                        PressCooldown = Time.frameCount;
                    } else if (button.Text.Contains("<"))
                    {
                        if (menu.currentPage > 0)
                        {
                            menu.currentPage--;
                        } else
                        {
                            menu.currentPage = (menu.Buttons.ToArray().Length + menu.ButtonsPerPage - 1) / menu.ButtonsPerPage - 1;
                        }
                        RefreshMenu(menu);
                        PressCooldown = Time.frameCount;
                    }

                    // something is wrong pls fix - fixed
                    if (Input.instance.CheckButton(Input.ButtonType.grip, true) && !button.Text.Contains(">") && !button.Text.Contains("<") && button.Text != "Disconnect")
                    {
                        button.IsFavorited = !button.IsFavorited;
                        switch (button.IsFavorited)
                        {
                            case false:
                                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(menu.TapFavButtonSound, menu.LeftHand, menu.TapSoundStrength);
                                menu.FavoritedMods.Remove(button);
                                // this adds extra buttons to the menu - no longer does
                                List<ButtonTemplate> fixedButtons1 = new List<ButtonTemplate>();
                                foreach (ButtonTemplate btn in menu.OriginalButtons)
                                {
                                    if (!menu.FavoritedMods.Contains(btn))
                                    {
                                        fixedButtons1.Add(btn);
                                    }
                                }
                                menu.Buttons = menu.FavoritedMods.Concat(fixedButtons1).ToList();
                                menu.Config(false);
                                Menu.RefreshMenu(menu);
                                PressCooldown = Time.frameCount;
                                return;

                            case true:
                                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(menu.TapFavButtonSound, menu.LeftHand, menu.TapSoundStrength);
                                menu.FavoritedMods.Add(button);
                                // same here
                                List<ButtonTemplate> fixedButtons2 = new List<ButtonTemplate>();
                                foreach (ButtonTemplate btn in menu.OriginalButtons)
                                {
                                    if (!menu.FavoritedMods.Contains(btn))
                                    {
                                        fixedButtons2.Add(btn);
                                    }
                                }
                                menu.Buttons = menu.FavoritedMods.Concat(fixedButtons2).ToList();
                                Menu.RefreshMenu(menu);
                                PressCooldown = Time.frameCount;
                                return;

                            default:
                                break;
                        }
                    }

                    // Put mod description on board and send notification
                    if (!button.Text.Contains(">") && !button.Text.Contains("<") && button.Text != "Disconnect")
                    {
                        string color = !button.ButtonState ? "green" : "red";
                        if (!button.Toggle) color = "green";
                        string text = $"[<color={color}>{button.Text}</color>] {button.Description}";
                        Board.SetBoardText($"Aspect Cheat Panel {Plugin.Plugin.modVersion}", Update.GetBoardText($"There is a total of {menu.OriginalButtons.Count} mods in this menu! Holding left-grip while pressing a button favorites and pins it to start.\n\nCurrent Mod:\n{text}"));
                        NotifiLib.SendNotification(text);
                    }

                    if (button.ButtonState && button.OnDisable != null && button.Toggle)
                    {
                        button.ButtonState = !button.ButtonState;

                        button.OnDisable.Invoke();

                        Menu.RefreshMenu(menu);
                        PressCooldown = Time.frameCount;
                        return;
                    }

                    if (button.Toggle)
                    {
                        button.ButtonState = !button.ButtonState;

                        if (button.ButtonState && button.OnEnable != null)
                        {
                            button.OnEnable.Invoke();
                            if (button.OnUpdate != null)
                            {
                                button.OnUpdate.Invoke();
                            }
                        }

                        Menu.RefreshMenu(menu);
                        PressCooldown = Time.frameCount;
                        return;
                    }

                    if (!button.Toggle && button.OnUpdate != null)
                    {
                        button.OnUpdate.Invoke();

                        Menu.RefreshMenu(menu);
                        PressCooldown = Time.frameCount;
                    }
                }
            }
        }
    }
}
