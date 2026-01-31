using UnityEngine;

namespace Chromatic
{
    enum MaskColor
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        None = 3
    }

    public class ExampleMaskBehaviour : MonoBehaviour
    {
        [SerializeField] private Renderer outerRingRenderer;
        [SerializeField] private Renderer circleRenderer;
        [SerializeField] private MaskColor maskColor;
        [SerializeField] private float maskEffectSpeed = 2f;
        [SerializeField] private float maskEffectBorder = 0.1f;

        private Material _outerRingMaterial;
        private Material _circleMaterial;

        private bool _hasStarted;
        private float _radius;

        private void Awake()
        {
            _outerRingMaterial = outerRingRenderer.material;
            _circleMaterial = circleRenderer.material;

            Restart();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.R))
                Restart();

            if (Input.GetKeyDown(KeyCode.M))
                _hasStarted = true;

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.M) && false)
                UnityEditor.EditorApplication.isPaused = true;
#endif

            if (_hasStarted)
                InvokeMask();
        }

        private void Restart()
        {
            _outerRingMaterial.SetFloat("_Border", maskEffectBorder);
            _outerRingMaterial.SetInteger("_ColorMasking", (int)maskColor);

            _hasStarted = false;
            _radius = 0.0f;
            _outerRingMaterial.SetFloat("_Distance", 0.0f);
            _circleMaterial.SetFloat("_Distance", 0.0f);
        }

        private void InvokeMask()
        {
            _outerRingMaterial.SetFloat("_Distance", _radius);
            _circleMaterial.SetFloat("_Distance", _radius - maskEffectBorder);
            _radius += Time.deltaTime * maskEffectSpeed;
        }
    }
}
