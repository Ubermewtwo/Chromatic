using UnityEngine;

namespace Chromatic
{
    [RequireComponent(typeof(Camera))]
    public class MaskChangedRendering : MonoBehaviour
    {
        [SerializeField] private Camera renderCamera;

        private RenderTexture _renderTexture;

        private void Awake()
        {
            if (!renderCamera) renderCamera = GetComponent<Camera>();

            _renderTexture = renderCamera.targetTexture;
        }
    }
}
