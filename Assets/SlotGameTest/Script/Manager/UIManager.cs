using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Slot_Game_Test.Gameplay;

namespace Slot_Game_Test.Manager
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Elements")]
        public TMP_Text balanceText;
        public TMP_Text winText;
        public Button spinButton;
        public GameObject winEffect;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            spinButton.onClick.AddListener(GameManager.Instance.StartSpin);
        }

        public void UpdateBalanceUI()
        {
            balanceText.text = $"BALANCE: {GameManager.Instance.playerBalance}";
            spinButton.interactable = GameManager.Instance.playerBalance >= GameManager.Instance.currentBet;
        }

        public void ShowWin(int amount)
        {
            winText.text = $"WIN: {amount}";
            winEffect.SetActive(true);
            CancelInvoke(nameof(HideWin));
            Invoke(nameof(HideWin), 2f);
        }

        private void HideWin()
        {
            winText.text = "";
            winEffect.SetActive(false);
        }

        public void UpdateBetUI()
        {
            // Forward to BetManager if you have one
            transform.GetComponent<BetManager>()?.UpdateBetUI();
        }
    }
}