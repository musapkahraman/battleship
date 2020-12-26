using UnityEngine;

namespace BattleshipGame.UI
{
    public class SpriteResizer : MonoBehaviour
    {
        private const float ReferenceRatio = 16f / 9;

        private void Awake()
        {
            var t = transform;
            float screenRatio = (float) Screen.width / Screen.height;
            if (screenRatio < ReferenceRatio)
            {
                float multiplier = ReferenceRatio / screenRatio;
                var scale = t.localScale;
                scale.y *= multiplier;
                t.localScale = scale;
            }
            else
            {
                float multiplier = screenRatio / ReferenceRatio;
                var scale = t.localScale;
                scale.x *= multiplier;
                t.localScale = scale;
            }
        }
    }
}
