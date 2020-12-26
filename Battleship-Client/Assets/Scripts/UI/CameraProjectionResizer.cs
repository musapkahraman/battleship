using UnityEngine;

namespace BattleshipGame.UI
{
    public class CameraProjectionResizer : MonoBehaviour
    {
        private const float ReferenceRatio = 16f / 9;

        private void Awake()
        {
            float screenRatio = (float) Screen.width / Screen.height;
            if (screenRatio < ReferenceRatio)
            {
                float multiplier = ReferenceRatio / screenRatio;
                GetComponent<Camera>().orthographicSize *= multiplier;
            }
        }
    }
}