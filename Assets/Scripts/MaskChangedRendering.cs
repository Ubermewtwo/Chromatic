using UnityEngine;

namespace Chromatic
{
    [RequireComponent(typeof(Camera))]
    public class MaskChangedRendering : MonoBehaviour
    {
        [SerializeField] private Camera renderCamera;
        public Camera RenderCamera { get { return renderCamera; } }
        public MaskColor MaskColor { get; set; }

        private Camera _mainCamera;

        private void Awake()
        {
            if (!renderCamera) renderCamera = GetComponent<Camera>();

            _mainCamera = Camera.main;
        }

        public void OnChangedMasks()
        {
            ChangeCullingMaskUtil.ChangeCullingMaskOfACamera(_mainCamera, MaskColor);
        }
    }
}
