//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
using UnityEngine;

namespace ScreenAspect
{
    [AutoAttachToMainCamera(CreateIfMissing = true)]
    public class ScreenAspector : MonoBehaviour
    {
        [Range(0.1f, 3f)] public float targetAspect = 16f / 9f;
    
        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();
            UpdateAspect();
        }

        private void UpdateAspect()
        {
            var currentAspect = (float)Screen.width / Screen.height;
            var scaleHeight = currentAspect / targetAspect;

            Rect rect = _camera.rect;

            if (scaleHeight < 1.0f)
            {
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.y = (1.0f - scaleHeight) / 2.0f;
            }
            else
            {
                float scaleWidth = 1.0f / scaleHeight;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
            }

            _camera.rect = rect;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && _camera != null)
                UpdateAspect();
        }
#endif
    }
}