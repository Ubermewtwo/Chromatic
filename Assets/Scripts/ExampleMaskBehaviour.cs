using UnityEngine;

namespace Chromatic
{
    

    public class ExampleMaskBehaviour : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer outerRingSprite;
        [SerializeField] private SpriteRenderer circleSprite;
        [SerializeField] private float maskEffectSpeed = 2f;
        [SerializeField] private float maskEffectBorder = 0.1f;

        private Material _outerRingMaterial;
        private Material _circleMaterial;

        private bool _hasStarted;
        private float _radius;

        private void Awake()
        {
            _outerRingMaterial = outerRingSprite.material;
            _circleMaterial = circleSprite.material;

            _outerRingMaterial.SetFloat("_IsRing", 1f);
            _outerRingMaterial.SetFloat("_Border", maskEffectBorder);

            // TODO: Select Color Masking from the editor
            _outerRingMaterial.SetInteger("_ColorMask", 0);

            Restart();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.R))
                Restart();

            if (Input.GetKeyDown(KeyCode.M))
                _hasStarted = true;

            if (_hasStarted)
            {
                _outerRingMaterial.SetFloat("_Distance", _radius);
                _circleMaterial.SetFloat("_Distance", _radius - maskEffectBorder);
                _radius += Time.deltaTime * maskEffectSpeed;
            }
        }

        private void Restart()
        {
            _hasStarted = false;
            _radius = 0.0f;
            _outerRingMaterial.SetFloat("_Distance", 0.0f);
            _circleMaterial.SetFloat("_Distance", 0.0f);
        }
    }
}
