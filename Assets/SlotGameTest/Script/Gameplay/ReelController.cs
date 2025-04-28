using Slot_Game_Test.Manager;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace Slot_Game_Test.Gameplay
{
    public class ReelController : MonoBehaviour
    {
        [Header("Settings")]
        public int visibleSymbols = 3;
        public float spinDuration = 2f;
        public float maxSpinSpeed = 2000f;
        public float minSpinSpeed = 300f;
        public float symbolSpacing = 100f; // Matches with Layout Group spacing

        [Header("References")]
        public Symbol[] possibleSymbols;

        [Header("Audio")]
        public AudioClip spinStartSound;
        public AudioClip spinStopSound;

        // Component references
        private VerticalLayoutGroup layoutGroup;
        private RectTransform rectTransform;
        private AudioSource audioSource;

        // Runtime variables
        private Symbol[] symbols;
        private bool isSpinning = false;
        private int spinResultIndex;
        private float spinStartTime;
        private float originalTopPadding;
        private float originalBottomPadding;

        // Symbol movement tracking
        private float totalScrollDistance = 0f;
        private float symbolHeight;

        // Timing and effects
        private float reelStartDelay = 0f;  // Will be set dynamically based on reel index

        private void Awake()
        {
            layoutGroup = GetComponent<VerticalLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Store original padding
            originalTopPadding = layoutGroup.padding.top;
            originalBottomPadding = layoutGroup.padding.bottom;

            // Calculate symbol height based on spacing and layout
            symbolHeight = symbolSpacing;
        }

        private void Start()
        {
            InitializeReel();

            // Find reel index to add cascade delay
            ReelController[] allReels = GameManager.Instance.reels;
            for (int i = 0; i < allReels.Length; i++)
            {
                if (allReels[i] == this)
                {
                    reelStartDelay = i * 0.15f;  // 150ms delay between reels
                    break;
                }
            }
        }

        private void InitializeReel()
        {
            // Clear existing symbols
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Symbol>() != null)
                {
                    Destroy(child.gameObject);
                }
            }

            // Create symbols (visible + buffers)
            int totalSymbols = visibleSymbols + 4; // Add buffer symbols above and below
            symbols = new Symbol[totalSymbols];

            for (int i = 0; i < totalSymbols; i++)
            {
                int randomIndex = Random.Range(0, possibleSymbols.Length);
                Symbol randomSymbol = possibleSymbols[randomIndex];

                symbols[i] = Instantiate(randomSymbol, transform);
            }

            // Ensure layout is refreshed
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        public void StartSpin()
        {
            if (isSpinning) return;

            // Pick random symbol to stop on
            spinResultIndex = Random.Range(0, possibleSymbols.Length);
            spinStartTime = Time.time;

            StartCoroutine(SpinReel());
        }

        private IEnumerator SpinReel()
        {
            // Add cascading delay for visual effect
            yield return new WaitForSeconds(reelStartDelay);

            // Play start sound
            if (spinStartSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(spinStartSound);
            }

            isSpinning = true;
            totalScrollDistance = 0f;

            float elapsedTime = 0f;
            float currentSpeed = 0f;

            // Speed up phase (0 to 20% of total time)
            while (elapsedTime < spinDuration * 0.2f)
            {
                float t = elapsedTime / (spinDuration * 0.2f);
                currentSpeed = Mathf.Lerp(minSpinSpeed, maxSpinSpeed, t);

                float moveDistance = currentSpeed * Time.deltaTime;
                MoveSymbols(moveDistance);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Constant speed phase (20% to 70% of total time)
            while (elapsedTime < spinDuration * 0.7f)
            {
                float moveDistance = maxSpinSpeed * Time.deltaTime;
                MoveSymbols(moveDistance);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Play stop sound at the beginning of slowdown
            if (spinStopSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(spinStopSound);
            }

            // Slow down with bounce (70% to 100% of total time)
            float slowDownStartTime = elapsedTime;
            float slowDownDuration = spinDuration * 0.3f;

            while (elapsedTime < spinDuration)
            {
                // Calculate progress through slowdown
                float slowDownProgress = (elapsedTime - slowDownStartTime) / slowDownDuration;

                // Apply easing function for natural slowdown
                float easedProgress = EaseOutBack(slowDownProgress);

                // Calculate current speed based on progress
                currentSpeed = Mathf.Lerp(maxSpinSpeed, 0, easedProgress);

                float moveDistance = currentSpeed * Time.deltaTime;
                MoveSymbols(moveDistance);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the reel stops at the exact position we want
            AlignSymbolsToFinalPosition();

            // Set the final symbols
            SetFinalSymbols();

            isSpinning = false;

            // Notify GameManager when all reels stop
            bool allStopped = true;
            foreach (ReelController reel in GameManager.Instance.reels)
            {
                if (reel.isSpinning)
                {
                    allStopped = false;
                    break;
                }
            }

            if (allStopped)
            {
                GameManager.Instance.OnSpinComplete();
            }
        }

        // EaseOutBack function for a slight overshoot at the end
        private float EaseOutBack(float x)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1;

            return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
        }

        private void MoveSymbols(float distance)
        {
            totalScrollDistance += distance;

            // Since we can't directly offset symbols in a VerticalLayoutGroup,
            // we'll use the padding to create the scrolling effect
            float offset = totalScrollDistance % symbolHeight;

            // Update the padding to create scrolling effect
            layoutGroup.padding.top = Mathf.RoundToInt(originalTopPadding - offset);
            layoutGroup.padding.bottom = Mathf.RoundToInt(originalBottomPadding + offset);

            // Force layout update
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            // Check if we've scrolled far enough to cycle symbols
            if (offset < distance)
            {
                CycleSymbols();
            }
        }

        private void CycleSymbols()
        {
            // Move the first symbol to the last position
            Transform firstSymbol = transform.GetChild(0);
            firstSymbol.SetAsLastSibling();

            // Randomize the symbol that was moved to the bottom (now will appear from the top)
            Symbol symbol = firstSymbol.GetComponent<Symbol>();
            if (symbol != null)
            {
                int randomIndex = Random.Range(0, possibleSymbols.Length);
                symbol.SetSymbol(possibleSymbols[randomIndex]);
            }
        }

        private void AlignSymbolsToFinalPosition()
        {
            // Reset padding to original values
            layoutGroup.padding.top = (int)originalTopPadding;
            layoutGroup.padding.bottom = (int)originalBottomPadding;

            // Force layout update
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        private void SetFinalSymbols()
        {
            // Calculate the indices of visible symbols
            int middleIndex = visibleSymbols / 2;

            // We need to set the child objects in the transform hierarchy
            // This ensures the symbols are properly aligned in the viewport

            // Set middle symbol to be our result
            Transform middleSymbolTransform = transform.GetChild(middleIndex);
            Symbol middleSymbol = middleSymbolTransform.GetComponent<Symbol>();
            middleSymbol.SetSymbol(possibleSymbols[spinResultIndex]);

            // Set symbols above middle
            for (int i = 0; i < middleIndex; i++)
            {
                Transform symbolTransform = transform.GetChild(i);
                Symbol symbol = symbolTransform.GetComponent<Symbol>();

                // Calculate which symbol should be here (e.g., pattern before the result)
                int index = (spinResultIndex - (middleIndex - i) + possibleSymbols.Length) % possibleSymbols.Length;
                symbol.SetSymbol(possibleSymbols[index]);
            }

            // Set symbols below middle
            for (int i = middleIndex + 1; i < transform.childCount; i++)
            {
                Transform symbolTransform = transform.GetChild(i);
                Symbol symbol = symbolTransform.GetComponent<Symbol>();

                // Calculate which symbol should be here (e.g., pattern after the result)
                int index = (spinResultIndex + (i - middleIndex)) % possibleSymbols.Length;
                symbol.SetSymbol(possibleSymbols[index]);
            }
        }

        // GetVisibleSymbol method to work with your existing GameManager
        public Symbol GetVisibleSymbol(int position)
        {
            if (position < 0 || position >= visibleSymbols)
                return null;

            // The actual symbol position needs to account for buffer symbols
            int adjustedPosition = position;

            // Get the child at the right position
            // If we have buffer symbols above, need to adjust the index
            if (transform.childCount > position)
            {
                Transform symbolTransform = transform.GetChild(adjustedPosition);
                return symbolTransform.GetComponent<Symbol>();
            }

            Debug.LogError("Symbol not found at position: " + position);
            return null;
        }
    }
}