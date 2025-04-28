using UnityEngine;
using UnityEngine.UI;

namespace Slot_Game_Test.Gameplay
{
    public enum SymbolType
    {
        One,
        Two,
        Three,
        Four,
        Five,
        J,
        K,
        Q,
        scatter,
        Wild
    }

    public class Symbol : MonoBehaviour
    {
        public SymbolType symbolType;
        public int baseValue;

        [Header("Visuals")]
        public Image spriteRenderer;

        private bool isAnimating = false;
        private float animationTime = 3f; // total duration of one cycle
        private float timer = 0f;
        private Vector3 originalScale;
        private Color originalColor;

        private void Awake()
        {
            originalScale = transform.localScale;
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        private void Update()
        {
            if (isAnimating)
            {
                timer += Time.deltaTime;
                float t = Mathf.PingPong(timer * 2f, 1f);

                // Scale bounce
                transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.3f, t);

                // Flash effect (optional)
                if (spriteRenderer != null)
                {
                    Color flashColor = Color.white;
                    flashColor.a = Mathf.Lerp(1f, 0.5f, t); // flicker alpha
                    spriteRenderer.color = flashColor;
                }

                if (timer >= animationTime)
                {
                    StopWinAnimation();
                }
            }
        }

        public void SetSymbol(Symbol template)
        {
            symbolType = template.symbolType;
            baseValue = template.baseValue;
            spriteRenderer.sprite = template.spriteRenderer.sprite;
        }

        public void PlayWinAnimation()
        {
            if (isAnimating) return; // prevent stacking

            timer = 0f;
            isAnimating = true;
            Debug.Log("Playing Animations");
        }

        private void StopWinAnimation()
        {
            isAnimating = false;
            transform.localScale = originalScale;
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
    }
}