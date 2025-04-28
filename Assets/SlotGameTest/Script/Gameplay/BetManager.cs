using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Slot_Game_Test.Manager;

namespace Slot_Game_Test.Gameplay
{
    public class BetManager : MonoBehaviour
    {
        [Header("UI References")]
        public Button betIncreaseButton;
        public Button betDecreaseButton;
        public TMP_Text betText;

        private void Start()
        {
            betIncreaseButton.onClick.AddListener(() => GameManager.Instance.ChangeBet(10));
            betDecreaseButton.onClick.AddListener(() => GameManager.Instance.ChangeBet(-10));
            UpdateBetUI();
        }

        public void UpdateBetUI()
        {
            betText.text = $"BET: {GameManager.Instance.currentBet}";

            betDecreaseButton.interactable = GameManager.Instance.currentBet > GameManager.Instance.minBet;
            betIncreaseButton.interactable = GameManager.Instance.currentBet < GameManager.Instance.maxBet;
        }
    }
}