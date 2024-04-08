using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Aspect.Utilities;
using System.Collections.Generic;
using GorillaNetworking;

namespace Aspect.MenuLib
{
    public static class GorillaMods
    {
        // Flush RPCs
        public static void FlushRPCs()
        {
            GorillaNot.instance.rpcCallLimit = int.MaxValue;
            GorillaNot.instance.rpcCallLimit = int.MaxValue;
            PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
            PhotonNetwork.RemoveBufferedRPCs(GorillaTagger.Instance.myVRRig.ViewID);
        }

        // ESP
        static Dictionary<string, Color> OriginalRigColors = new Dictionary<string, Color>();
        static Dictionary<string, GameObject> Tracers = new Dictionary<string, GameObject>();
        static Color taggedColor = new Color(0.33f, 0.06f, 0.6f, .4f);
        static Color casualColor = new Color(0.49f, 0.06f, 0.8f, .2f);
        public static void ESP(bool Reset = false)
        {
            // make ESP using GorillaExtensions.ESP() - deleted that shit (IT WAS ASS)
            if (Reset)
            {
                foreach (VRRig rig in GorillaParent.instance.vrrigs)
                {
                    if (rig == GorillaTagger.Instance.offlineVRRig) continue;

                    rig.mainSkin.material.shader = Shader.Find("GorillaTag/UberShader");
                    if (OriginalRigColors.ContainsKey(RigManager.VRRigToPhotonView(rig).Owner.UserId))
                    {
                        if (RigManager.IsTagged(rig))
                        {
                            rig.mainSkin.material.color = Color.white;
                        }
                        else
                        {
                            rig.mainSkin.material.color = OriginalRigColors[RigManager.VRRigToPhotonView(rig).Owner.UserId];
                        }
                    }

                    // without this if statement the ESP doesnt turn off
                    if (Tracers.ContainsKey(RigManager.VRRigToPhotonView(rig).Owner.UserId))
                    {
                        GameObject.Destroy(Tracers[RigManager.VRRigToPhotonView(rig).Owner.UserId]);
                        Tracers.Remove(RigManager.VRRigToPhotonView(rig).Owner.UserId);
                    }
                    // idek why Original rig colors was in here
                }
                OriginalRigColors.Clear();
                return;
            }

            for (int i = 0; i < GorillaParent.instance.vrrigs.Count; i++)
            {
                VRRig rig = GorillaParent.instance.vrrigs[i];
                if (rig == GorillaTagger.Instance.offlineVRRig) continue;

                if (!OriginalRigColors.ContainsKey(RigManager.VRRigToPhotonView(rig).Owner.UserId))
                {
                    if (RigManager.IsTagged(rig))
                    {
                        OriginalRigColors.Add(RigManager.VRRigToPhotonView(rig).Owner.UserId, Random.ColorHSV());
                    }
                    else
                    {
                        OriginalRigColors.Add(RigManager.VRRigToPhotonView(rig).Owner.UserId, rig.mainSkin.material.color);
                    }
                }

                rig.mainSkin.material.color = RigManager.IsTagged(rig) ? taggedColor : casualColor;
                rig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");

                if (Input.instance.CheckButton(Input.ButtonType.grip, true))
                {
                    // tracer
                    if (!Tracers.ContainsKey(RigManager.VRRigToPhotonView(rig).Owner.UserId))
                    {
                        GameObject line = new GameObject(RigManager.VRRigToPhotonView(rig).Owner.UserId);
                        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                        lineRenderer.material.shader = Shader.Find("GUI/Text Shader");
                        lineRenderer.startWidth = 0.025f;
                        lineRenderer.endWidth = 0.025f;
                        lineRenderer.startColor = RigManager.IsTagged(rig) ? taggedColor : casualColor;
                        lineRenderer.endColor = RigManager.IsTagged(rig) ? taggedColor : casualColor;
                        lineRenderer.SetPosition(0, GorillaLocomotion.Player.Instance.leftControllerTransform.position);
                        lineRenderer.SetPosition(1, rig.transform.position);
                        Tracers.Add(RigManager.VRRigToPhotonView(rig).Owner.UserId, line);
                    }
                    else
                    {
                        LineRenderer tracer = Tracers[RigManager.VRRigToPhotonView(rig).Owner.UserId].GetComponent<LineRenderer>();
                        tracer.startColor = rig.mainSkin.material.color;
                        tracer.endColor = rig.mainSkin.material.color;
                        tracer.SetPosition(0, GorillaLocomotion.Player.Instance.leftControllerTransform.position);
                        tracer.SetPosition(1, rig.transform.position);
                    }
                } else
                {
                    if (Tracers.Values.Count != 0)
                    {
                        GameObject.Destroy(Tracers[RigManager.VRRigToPhotonView(rig).Owner.UserId]);
                        Tracers.Remove(RigManager.VRRigToPhotonView(rig).Owner.UserId);
                    }
                }
            }

            // probably a better way to do this
            if (Tracers.Values.Count != 0)
            {
                List<string> ids = new List<string>();
                foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
                {
                    ids.Add(player.UserId);
                }
                foreach (string id in Tracers.Keys)
                {
                    if (!ids.Contains(id))
                    {
                        GameObject.Destroy(Tracers[id]);
                        Tracers.Remove(id);
                    }
                }
            }
        }

        // Tag Gun
        public static void TagGun()
        {
            // make tag gun using GorillaExtensions.GunTemplate()
            RaycastHit hit = GorillaExtensions.GunTemplate();
            if (Input.instance.CheckButton(Input.ButtonType.trigger, false) && hit.collider.GetComponentInParent<VRRig>() != null && !RigManager.IsTagged(hit.collider.GetComponentInParent<VRRig>()))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (PhotonNetwork.PlayerList.Length < 4)
                    {
                        GorillaGameManager.instance.GetComponent<GorillaTagManager>().ChangeCurrentIt(RigManager.VRRigToPhotonView(hit.collider.GetComponentInParent<VRRig>()).Owner);
                        return;
                    }
                    GorillaGameManager.instance.GetComponent<GorillaTagManager>().AddInfectedPlayer(RigManager.VRRigToPhotonView(hit.collider.GetComponentInParent<VRRig>()).Owner);
                    return;
                } else if (RigManager.IsTagged(GorillaTagger.Instance.offlineVRRig)) 
                {
                    if (PhotonNetwork.PlayerList.Length < 4)
                    {
                        GorillaGameManager.instance.GetComponent<GorillaTagManager>().ChangeCurrentIt(RigManager.VRRigToPhotonView(hit.collider.GetComponentInParent<VRRig>()).Owner);
                        return;
                    }
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = hit.point + new Vector3(0, 2, 0);
                    GorillaLocomotion.Player.Instance.leftControllerTransform.gameObject.transform.position = hit.point;
                    return;
                }
            }
            GorillaTagger.Instance.offlineVRRig.enabled = true;
        }

        // Tag All
        public static void TagAll()
        {
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.PlayerList.Length < 4 && GorillaGameManager.instance.GameModeName() == "INFECTION")
            {
                foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
                {
                    if (player.UserId == PhotonNetwork.LocalPlayer.UserId) continue;

                    if (PhotonNetwork.PlayerList.Length < 4)
                    {
                        GorillaGameManager.instance.GetComponent<GorillaTagManager>().ChangeCurrentIt(player);
                        break;
                    }
                    GorillaGameManager.instance.GetComponent<GorillaTagManager>().AddInfectedPlayer(player);
                }
            }
        }

        // RGB (only works in stump for now)
        static float RGBcooldown = 0f;
        public static void RGB(float Cooldown = 1f)
        {
            // Create RGB action
            Color color = Color.HSVToRGB((Time.frameCount / 180f) % 1f, 1f, 1f);
            if (RGBcooldown < Time.time)
            {
                if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(RigManager.VRRigToPhotonView(GorillaTagger.Instance.offlineVRRig).Owner.UserId))
                {
                    RigManager.VRRigToPhotonView(GorillaTagger.Instance.offlineVRRig).RPC("InitializeNoobMaterial", RpcTarget.All, new object[] { color.r, color.g, color.b });
                    FlushRPCs();
                }
                RGBcooldown = Time.time + Cooldown;
            }
        }

        // Platforms
        static GameObject left;
        static GameObject right;
        public static void Platforms(bool Reset = false, bool sticky = false, int oIndex = 0)
        {
            if (Reset)
            {
                if (left != null) UnityEngine.Object.Destroy(left);
                if (right != null) UnityEngine.Object.Destroy(right);
                return;
            }

            PrimitiveType primitiveType = sticky ? PrimitiveType.Sphere : PrimitiveType.Cube;
            Vector3 size = oIndex != 61 ? new Vector3(0.001f, 0.2f, 0.2f) : new Vector3(0.001f, 0.4f, 0.4f);

            if (Input.instance.CheckButton(Input.ButtonType.grip, false) && right == null)
            {
                right = GameObject.CreatePrimitive(primitiveType);
                Menu.ColorChanger colorChanger = right.AddComponent<Menu.ColorChanger>();
                colorChanger.Color1 = new Color32(85, 15, 150, 1);
                colorChanger.Color2 = new Color32(125, 15, 200, 1);
                right.AddComponent<GorillaSurfaceOverride>().overrideIndex = oIndex;
                right.transform.localScale = size;
                right.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.transform.position + -GorillaLocomotion.Player.Instance.rightControllerTransform.transform.right / 20;
                right.transform.rotation = GorillaLocomotion.Player.Instance.rightControllerTransform.transform.rotation;
            }
            else if (!Input.instance.CheckButton(Input.ButtonType.grip, false) && right != null)
            {
                UnityEngine.Object.Destroy(right);
                right = null;
            }
            if (Input.instance.CheckButton(Input.ButtonType.grip, true) && left == null)
            {
                left = GameObject.CreatePrimitive(primitiveType);
                Menu.ColorChanger colorChanger = left.AddComponent<Menu.ColorChanger>();
                colorChanger.Color1 = new Color32(85, 15, 150, 1);
                colorChanger.Color2 = new Color32(125, 15, 200, 1);
                left.AddComponent<GorillaSurfaceOverride>().overrideIndex = oIndex;
                left.transform.localScale = size;
                left.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.transform.position + GorillaLocomotion.Player.Instance.leftControllerTransform.transform.right / 20;
                left.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.transform.rotation;
            }
            else if (!Input.instance.CheckButton(Input.ButtonType.grip, true) && left != null)
            {
                UnityEngine.Object.Destroy(left);
                left = null;
            }
        }

        // Climb Anywhere
        public static void ClimbAnywhere(bool reset = false)
        {
            if (!reset)
            {
                if (Input.instance.CheckButton(Input.ButtonType.grip, false) && right == null && GorillaLocomotion.Player.Instance.IsHandTouching(false))
                {
                    right = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    GameObject.Destroy(right.GetComponent<Renderer>());
                    right.transform.localScale = new Vector3(0.001f, 0.2f, 0.2f);
                    right.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.transform.position;
                    right.transform.rotation = GorillaLocomotion.Player.Instance.rightControllerTransform.transform.rotation;
                }
                else if (!Input.instance.CheckButton(Input.ButtonType.grip, false) && right != null || !GorillaLocomotion.Player.Instance.IsHandTouching(false) && right != null)
                {
                    UnityEngine.Object.Destroy(right);
                    right = null;
                }
                if (Input.instance.CheckButton(Input.ButtonType.grip, true) && left == null && GorillaLocomotion.Player.Instance.IsHandTouching(true))
                {
                    left = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    GameObject.Destroy(left.GetComponent<Renderer>());
                    left.transform.localScale = new Vector3(0.001f, 0.2f, 0.2f);
                    left.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.transform.position;
                    left.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.transform.rotation;
                }
                else if (!Input.instance.CheckButton(Input.ButtonType.grip, true) && left != null || !GorillaLocomotion.Player.Instance.IsHandTouching(true) && left != null)
                {
                    UnityEngine.Object.Destroy(left);
                    left = null;
                }
            } else
            {
                if (left != null) UnityEngine.Object.Destroy(left);
                if (right != null) UnityEngine.Object.Destroy(right);
            }
        }

        // Iron Monkey
        public static void IronMonkey(float Acceleration = 20f)
        {
            if (Input.instance.CheckButton(Input.ButtonType.primary, false))
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(new Vector3(20f * GorillaLocomotion.Player.Instance.rightControllerTransform.right.x, Acceleration * GorillaLocomotion.Player.Instance.rightControllerTransform.right.y, 20f * GorillaLocomotion.Player.Instance.rightControllerTransform.right.z), ForceMode.Acceleration);
            }
            if (Input.instance.CheckButton(Input.ButtonType.primary, true))
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(new Vector3(20f * GorillaLocomotion.Player.Instance.leftControllerTransform.right.x * -1f, 20f * GorillaLocomotion.Player.Instance.leftControllerTransform.right.y * -1f, 20f * GorillaLocomotion.Player.Instance.leftControllerTransform.right.z * -1f), ForceMode.Acceleration);
            }
        }

        // Change Gravity
        static Vector3 defaultGravity;
        public static void ChangeGravity(bool ResetGravity = false, float gravity = 3f)
        {
            if (!ResetGravity)
            {
                if (defaultGravity == Vector3.zero) defaultGravity = Physics.gravity;
                if (Physics.gravity != new Vector3(0f, -gravity, 0f)) Physics.gravity = new Vector3(0f, -gravity, 0f);
            } else if (defaultGravity != Vector3.zero && Physics.gravity != defaultGravity)
            {
                Physics.gravity = defaultGravity;
                defaultGravity = Vector3.zero;
            }
        }

        // Super Monkey
        static bool secondaryPower = false;
        static bool checkPowerOnce = false;
        public static void SuperMonkey(bool enable = true, float speed = 25f)
        {
            // flight
            if (Input.instance.CheckButton(Input.ButtonType.secondary, false))
            {
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity += GorillaLocomotion.Player.Instance.rightControllerTransform.forward * speed;
            }

            // secondary power
            if (enable)
            {
                if (!checkPowerOnce && Input.instance.CheckButton(Input.ButtonType.primary, false))
                {
                    secondaryPower = !secondaryPower;
                    checkPowerOnce = true;
                } else if (!Input.instance.CheckButton(Input.ButtonType.primary, false))
                {
                    checkPowerOnce = false;
                }
                if (secondaryPower)
                {
                    ChangeGravity(false, 0.01f);
                }
                if (!secondaryPower && defaultGravity != Vector3.zero && Physics.gravity != defaultGravity)
                {
                    ChangeGravity(true);
                }
            } else if (defaultGravity != Vector3.zero && Physics.gravity != defaultGravity)
            {
                ChangeGravity(true);
                secondaryPower = false;
                checkPowerOnce = false;
            }
        }

        // No-Clip
        public static void NoClip(bool enable = true)
        {
            if (enable)
            {
                if (Input.instance.CheckButton(Input.ButtonType.trigger, false))
                {
                    foreach (MeshCollider collider in MeshCollider.FindObjectsOfType<MeshCollider>())
                    {
                        collider.enabled = false;
                    }
                    return;
                }
                foreach (MeshCollider collider in MeshCollider.FindObjectsOfType<MeshCollider>())
                {
                    collider.enabled = true;
                }
            }
            else
            {
                foreach (MeshCollider collider in MeshCollider.FindObjectsOfType<MeshCollider>())
                {
                    collider.enabled = true;
                }
            }
        }

        // Ghost Monkey
        static bool IsGhost = false;
        static bool IsGhostCooldown = false;
        public static void GhostMonkey(bool enable = true)
        {
            if (enable)
            {
                if (!IsGhostCooldown && Input.instance.CheckButton(Input.ButtonType.primary, false))
                {
                    IsGhost = !IsGhost;
                    IsGhostCooldown = true;
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                } 
                else if (!Input.instance.CheckButton(Input.ButtonType.primary, false))
                {
                    IsGhostCooldown = false;
                }
                if (!IsGhost)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            } else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                IsGhostCooldown = false;
                IsGhost = false;
            }
        }

        // Invisibility
        static bool IsInvis = false;
        static bool IsInvisCooldown = false;
        public static void Invisibility(bool enable = true)
        {
            if (enable)
            {
                if (!IsInvisCooldown && Input.instance.CheckButton(Input.ButtonType.primary, false))
                {
                    IsInvis = !IsInvis;
                    IsInvisCooldown = true;
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.transform.position + new Vector3(0, -1000, 0);
                }
                else if (!Input.instance.CheckButton(Input.ButtonType.primary, false))
                {
                    IsInvisCooldown = false;
                }
                if (!IsInvis)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                IsInvisCooldown = false;
                IsInvis = false;
            }
        }

        // Teleport Gun
        static bool IsTeleportCooldown = false;
        public static void TeleportGun()
        {
            RaycastHit hit = GorillaExtensions.GunTemplate();
            if (!IsTeleportCooldown && Input.instance.CheckButton(Input.ButtonType.trigger, false))
            {
                GorillaPatches.TeleportPatch.Teleport(hit.point + GorillaLocomotion.Player.Instance.rightControllerTransform.up);
                IsTeleportCooldown = true;
            }
            else if (!Input.instance.CheckButton(Input.ButtonType.trigger, false))
            {
                IsTeleportCooldown = false;
            }
        }

        // Disable Quitbox - being turned on after being turned off doesnt work
        public static void DisableQuitbox(bool disable)
        {
            GameObject.Find("QuitBox").SetActive(!disable);
        }

        // Ender pearl
        public static void ProjectileTeleport(bool ride)
        {
            string[] projectilesToCheck = { "SnowballProjectile", "SlingshotProjectile" };
            foreach (string projectile in projectilesToCheck)
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag(projectile))
                {
                    if (obj.GetComponent<SlingshotProjectile>().projectileOwner == PhotonNetwork.LocalPlayer && !obj.GetComponent<GorillaExtensions.TeleportCollider>())
                    {
                        obj.AddComponent<GorillaExtensions.TeleportCollider>().ride = ride;
                    }
                }
            }
        }
    }

    public class GorillaExtensions
    {
        // gun template (can be used for all types of gun-mods)
        static GameObject gunPointer;
        static GameObject gunLine;
        static LineRenderer lineRenderer;

        public static RaycastHit GunTemplate(bool destroy_pointer = false, bool VibrateOnTarget = true)
        {
            if (destroy_pointer)
            {
                Object.Destroy(gunPointer);
                Object.Destroy(gunLine);
                Object.Destroy(lineRenderer);
                return new RaycastHit();
            }

            // make raycast gun here - done
            RaycastHit hit;
            if (Input.instance.CheckButton(Input.ButtonType.grip, false))
            {
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out hit);

                if (gunPointer == null)
                {
                    gunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Object.Destroy(gunPointer.GetComponent<Collider>());
                    Object.Destroy(gunPointer.GetComponent<Rigidbody>());
                    gunPointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    Menu.ColorChanger colorChanger = gunPointer.AddComponent<Menu.ColorChanger>();
                    colorChanger.Color1 = new Color32(85, 15, 150, 1);
                    colorChanger.Color2 = new Color32(125, 15, 200, 1);

                    gunLine = new GameObject("Line");
                    lineRenderer = gunLine.AddComponent<LineRenderer>();
                    lineRenderer.material.shader = Shader.Find("GUI/Text Shader");
                    lineRenderer.startWidth = 0.025f;
                    lineRenderer.endWidth = 0.025f;
                    lineRenderer.startColor = colorChanger.Color1;
                    lineRenderer.endColor = colorChanger.Color1;
                    lineRenderer.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                    lineRenderer.SetPosition(1, hit.point);

                    gunPointer.transform.position = hit.point;
                    return hit;
                }
                lineRenderer.startColor = gunPointer.GetComponent<Renderer>().material.color;
                lineRenderer.endColor = gunPointer.GetComponent<Renderer>().material.color;
                lineRenderer.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                lineRenderer.SetPosition(1, gunPointer.transform.position);

                if (hit.collider.GetComponentInParent<VRRig>() != null) GorillaTagger.Instance.DoVibration(UnityEngine.XR.XRNode.RightHand, 0.1f, 0.01f);

                gunPointer.transform.position = hit.point;
                return hit;
            }

            Object.Destroy(gunPointer);
            gunPointer = null;
            Object.Destroy(gunLine);
            gunLine = null;
            Object.Destroy(lineRenderer);
            lineRenderer = null;
            Physics.Raycast(new Vector3(0,0,0), new Vector3(0,-1,0), out hit);

            return hit;
        }

        // can be added to gameobjects to make them teleport you when colliding
        public class TeleportCollider : MonoBehaviour
        {
            public bool ride;
            public Vector3 velocity;

            public void LateUpdate()
            {
                // cancel script if the projectile is not yours
                if (base.GetComponent<SlingshotProjectile>().projectileOwner != PhotonNetwork.LocalPlayer) return;

                // ride projectile
                if (ride)
                {
                    GorillaPatches.TeleportPatch.Teleport(base.transform.position);
                    velocity = new Vector3(base.GetComponent<Rigidbody>().velocity.x, 0, base.GetComponent<Rigidbody>().velocity.z);
                }
            }

            public void OnCollisionEnter(Collision collision)
            {
                // cancel script if the projectile is not yours
                if (base.GetComponent<SlingshotProjectile>().projectileOwner != PhotonNetwork.LocalPlayer) return;

                // get player velocity if sudden teleport
                if (!ride) velocity = GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity;

                // final teleport
                if (!ride) GorillaPatches.TeleportPatch.Teleport(base.transform.position);

                // set new velocity and destroy projectile
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = velocity;
                Destroy(base.GetComponent<TeleportCollider>());
            }
        }
    }

    class GorillaPatches
    {
        // Makes teleporting possible
        [HarmonyPatch(typeof(GorillaLocomotion.Player))]
        [HarmonyPatch("LateUpdate")]
        internal class TeleportPatch
        {
            private static bool teleporting;
            private static Vector3 destination;
            private static bool teleportOnce;

            internal static bool Prefix(GorillaLocomotion.Player __instance, ref Vector3 ___lastPosition, ref Vector3[] ___velocityHistory, ref Vector3 ___lastHeadPosition, ref Vector3 ___lastLeftHandPosition, ref Vector3 ___lastRightHandPosition, ref Vector3 ___currentVelocity, ref Vector3 ___denormalizedVelocityAverage)
            {
                if (teleporting)
                {
                    Vector3 place = destination - __instance.bodyCollider.transform.position + __instance.transform.position;

                    try
                    {
                        __instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                        __instance.bodyCollider.attachedRigidbody.isKinematic = true;

                        ___velocityHistory = new Vector3[__instance.velocityHistorySize];
                        ___currentVelocity = Vector3.zero;
                        ___denormalizedVelocityAverage = Vector3.zero;

                        ___lastRightHandPosition = place;
                        ___lastLeftHandPosition = place;
                        ___lastHeadPosition = place;
                        __instance.transform.position = place;
                        ___lastPosition = place;

                        __instance.bodyCollider.attachedRigidbody.isKinematic = false;
                    }
                    catch { }

                    teleporting = false;
                }

                return true;
            }

            internal static void Teleport(Vector3 TeleportDestination)
            {
                teleporting = true;
                destination = TeleportDestination;
            }

            internal static void TeleportOnce(Vector3 TeleportDestination, bool stateDepender)
            {
                if (stateDepender)
                {
                    if (!teleportOnce)
                    {
                        teleporting = true;
                        destination = TeleportDestination;
                    }
                    teleportOnce = true;
                }
                else
                {
                    teleportOnce = false;
                }
            }
        }

        // Patches VRRig.OnDisable() because it causes ban
        [HarmonyPatch(typeof(VRRig))]
        [HarmonyPatch("OnDisable")]
        internal class OnDisablePatch
        {
            public static bool Prefix(VRRig __instance)
            {
                if (__instance == GorillaTagger.Instance.offlineVRRig)
                {
                    return false;
                }
                return true;
            }
        }

        // Patches SetColor because it fucks up custom gameobject-colors (i think ... don't judge, i made this script 7 months ago)
        [HarmonyPatch(typeof(Material))]
        [HarmonyPatch("SetColor", new[] { typeof(string), typeof(Color) })]
        internal class GameObjectRenderFixer
        {
            private static void Prefix(Material __instance, string name, Color value)
            {
                if (name == "_Color")
                {
                    __instance.shader = Shader.Find("GorillaTag/UberShader");
                    __instance.color = value;
                    return;
                }
            }
        }

        // Sends notifications when players join
        [HarmonyPatch(typeof(GorillaNot))]
        [HarmonyPatch("OnPlayerEnteredRoom")]
        internal class OnJoin
        {
            public static void Prefix(Photon.Realtime.Player newPlayer)
            {
                if (newPlayer == PhotonNetwork.LocalPlayer)
                {
                    NotifiLib.SendNotification($"[<color=yellow>SERVER</color>] Player {newPlayer.NickName} joined");
                }
            }
        }

        [HarmonyPatch(typeof(GorillaNot))]
        [HarmonyPatch("OnPlayerLeftRoom")]
        internal class OnLeave
        {
            public static void Prefix(Photon.Realtime.Player otherPlayer)
            {
                if (otherPlayer == PhotonNetwork.LocalPlayer)
                {
                    NotifiLib.SendNotification($"[<color=yellow>SERVER</color>] Player {otherPlayer.NickName} left");
                }
            }
        }

        // send notif if AntiCheat detects a mod, and also kinda works as an antiban
        [HarmonyPatch(typeof(GorillaNot))]
        [HarmonyPatch("SendReport")]
        internal class OnGorillaNotReport
        {
            static void Prefix(ref string susReason, ref string susId, ref string susNick)
            {
                if (susId == PhotonNetwork.LocalPlayer.UserId)
                {
                    NotifiLib.SendNotification($"[<color=red>ANTICHEAT</color>] You where reported by the anticheat for: {susReason}");
                    return;
                }
            }
        }
    }
}
