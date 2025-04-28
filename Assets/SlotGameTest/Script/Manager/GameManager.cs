using UnityEngine;
using Slot_Game_Test.Gameplay;

namespace Slot_Game_Test.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        public int minBet = 10;
        public int maxBet = 1000;

        [Header("Player Data")]
        public int playerBalance = 1000;
        public int currentBet = 50;

        [Header("References")]
        public ReelController[] reels;
        public PaylineManager paylineManager;

        private bool isSpinning = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartSpin()
        {
            if (isSpinning || playerBalance < currentBet) return;

            playerBalance -= currentBet;
            UIManager.Instance.UpdateBalanceUI();
            isSpinning = true;

            foreach (var reel in reels)
            {
                reel.StartSpin();
            }
        }

        public void OnSpinComplete()
        {
            isSpinning = false;
            EvaluateWins();
        }

        private void EvaluateWins()
        {
            Symbol[,] symbolGrid = GetSymbolGrid();
            int totalWin = paylineManager.CalculateWin(symbolGrid, currentBet);

            if (totalWin > 0)
            {
                playerBalance += totalWin;
                UIManager.Instance.ShowWin(totalWin);
                UIManager.Instance.UpdateBalanceUI();
            }
        }

        private Symbol[,] GetSymbolGrid()
        {
            Symbol[,] grid = new Symbol[reels.Length, reels[0].visibleSymbols];

            for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
            {
                for (int symbolPos = 0; symbolPos < reels[reelIndex].visibleSymbols; symbolPos++)
                {
                    grid[reelIndex, symbolPos] = reels[reelIndex].GetVisibleSymbol(symbolPos);
                }
            }

            return grid;
        }

        public void ChangeBet(int amount)
        {
            currentBet = Mathf.Clamp(currentBet + amount, minBet, maxBet);
            UIManager.Instance.UpdateBetUI();
        }
    }
}