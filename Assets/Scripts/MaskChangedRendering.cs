using UnityEngine;

namespace Chromatic
{
    [RequireComponent(typeof(Camera))]
    public class MaskChangedRendering : MonoBehaviour
    {
        [SerializeField] private RenderTexture renderTexture;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, renderTexture);
            Graphics.Blit(source, destination);
        }
    }
}
