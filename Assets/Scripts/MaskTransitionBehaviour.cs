using UnityEngine;

namespace Chromatic
{
    public class MaskTransitionBehaviour : MonoBehaviour
    {
        [SerializeField] private Renderer outerRingRenderer;
        [SerializeField] private Renderer circleRenderer;
        [SerializeField] private float maskEffectSpeed = 2f;
        [SerializeField] private float maskEffectBorder = 0.1f;

        private MaskChangedRendering _maskChangedRendering;

        private Material _outerRingMaterial;
        private Material _circleMaterial;

        private bool _hasStarted, _hasFinished;
        private float _radius;

        private void Awake()
        {
            _outerRingMaterial = outerRingRenderer.material;
            _circleMaterial = circleRenderer.material;

            Restart();
        }

        private void Start()
        {
            _maskChangedRendering = FindFirstObjectByType<MaskChangedRendering>();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Space))
                Restart();

            if (Input.GetKeyDown(KeyCode.R))
                ConfigureMask(MaskColor.Red);
            else if (Input.GetKeyDown(KeyCode.G))
                ConfigureMask(MaskColor.Green);
            else if (Input.GetKeyDown(KeyCode.B))
                ConfigureMask(MaskColor.Blue);

            if (_hasStarted)
                InvokeMask();

            if (_hasFinished)
            {
                _maskChangedRendering.OnChangedMasks();
                Restart();
            }
        }

        private void Restart()
        {
            _outerRingMaterial.SetFloat("_Border", maskEffectBorder);
            _outerRingMaterial.SetInteger("_ColorMasking", _outerRingMaterial.GetInteger("_ColorMasking"));

            _hasStarted = false;
            _radius = 0.0f;
            _outerRingMaterial.SetFloat("_Distance", 0.0f);
            _circleMaterial.SetFloat("_Distance", 0.0f);
            _hasFinished = false;
        }

        private void ConfigureMask(MaskColor maskColor)
        {
            if (_hasStarted) return;

            _outerRingMaterial.SetInteger("_ColorMasking", (int)maskColor);
            _maskChangedRendering.MaskColor = maskColor;

            ChangeCullingMaskUtil.ChangeCullingMaskOfACamera(_maskChangedRendering.RenderCamera, maskColor);

            _hasStarted = true;
        }

        private void InvokeMask()
        {
            _outerRingMaterial.SetFloat("_Distance", _radius);
            _circleMaterial.SetFloat("_Distance", _radius - maskEffectBorder);
            _radius += Time.deltaTime * maskEffectSpeed;

            if (_radius - maskEffectBorder >= 1f)
                _hasFinished = true;
        }
    }
}
