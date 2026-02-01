using UnityEngine;

namespace Chromatic
{
    class ChangeCullingMaskUtil
    {
        public static void ChangeCullingMaskOfACamera(Camera camera, MaskColor maskColor)
        {
            switch (maskColor)
            {
                case MaskColor.Red:
                    camera.cullingMask |= (1 << LayerMask.NameToLayer("Red"));
                    camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Green"));
                    camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Blue"));
                    break;
                case MaskColor.Green:
                    camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Red"));
                    camera.cullingMask |= (1 << LayerMask.NameToLayer("Green"));
                    camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Blue"));
                    break;
                case MaskColor.Blue:
                    camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Red"));
                    camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Green"));
                    camera.cullingMask |= (1 << LayerMask.NameToLayer("Blue"));
                    break;
                case MaskColor.None:
                    camera.cullingMask |= (1 << LayerMask.NameToLayer("Red"));
                    camera.cullingMask |= (1 << LayerMask.NameToLayer("Green"));
                    camera.cullingMask |= (1 << LayerMask.NameToLayer("Blue"));
                    break;
                case MaskColor.All:
                    camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Red"));
                    camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Green"));
                    camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Blue"));
                    break;
            }
        }
    }
}
