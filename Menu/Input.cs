using UnityEngine;

namespace Aspect.MenuLib
{
    public class Input
    {
        public static Input instance = new Input();

        public bool CheckButton(ButtonType type, bool leftHand)
        {
            if (leftHand)
            {
                switch (type)
                {
                    case ButtonType.trigger: return ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f;
                    case ButtonType.grip: return ControllerInputPoller.instance.leftControllerGripFloat > 0.5f;
                    case ButtonType.secondary: return ControllerInputPoller.instance.leftControllerSecondaryButton;
                    case ButtonType.primary: return ControllerInputPoller.instance.leftControllerPrimaryButton;

                    default:
                        break;
                }
            } else
            {
                switch (type)
                {
                    case ButtonType.trigger: return ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
                    case ButtonType.grip: return ControllerInputPoller.instance.rightControllerGripFloat > 0.5f;
                    case ButtonType.secondary: return ControllerInputPoller.instance.rightControllerSecondaryButton;
                    case ButtonType.primary: return ControllerInputPoller.instance.rightControllerPrimaryButton;

                    default:
                        break;
                }
            }
            return false;
        }

        public Vector2 GetJoystickVector()
        {
            return ControllerInputPoller.instance.rightControllerPrimary2DAxis;
        }

        public enum ButtonType
        {
            grip,
            trigger,
            primary,
            secondary
        }
    }
}
