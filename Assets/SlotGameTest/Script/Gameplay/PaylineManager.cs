using System;
using UnityEngine;

namespace Slot_Game_Test.Gameplay
{
    // A simple data class to represent a single Payline
    [Serializable]
    public class Payline
    {
        public int[] pattern;            // Sequence of symbol positions for each reel (0 = top row, 1 = middle, 2 = bottom)
    }

    // Manager class that handles paylines and win calculation
    public class PaylineManager : MonoBehaviour
    {
        // Define all the paylines with their symbol patterns
        public Payline[] paylines = new Payline[]
        {
        // 1-3: Straight rows (top, middle, bottom)
        new Payline { pattern = new int[] { 0, 0, 0, 0, 0 },},
        new Payline { pattern = new int[] { 1, 1, 1, 1, 1 },},
        new Payline { pattern = new int[] { 2, 2, 2, 2, 2 },},

        // 4-5: Diagonals (V and Inverted V shapes)
        new Payline { pattern = new int[] { 0, 1, 2, 1, 0 },},
        new Payline { pattern = new int[] { 2, 1, 0, 1, 2 },},

        // 6-9: Simple curves
        new Payline { pattern = new int[] { 0, 0, 1, 0, 0 },},
        new Payline { pattern = new int[] { 2, 2, 1, 2, 2 },},
        new Payline { pattern = new int[] { 1, 0, 0, 0, 1 },},
        new Payline { pattern = new int[] { 1, 2, 2, 2, 1 },},

        // 10-13: Zigzag patterns
        new Payline { pattern = new int[] { 0, 1, 0, 1, 0 },},
        new Payline { pattern = new int[] { 2, 1, 2, 1, 2 },},
        new Payline { pattern = new int[] { 1, 0, 1, 0, 1 },},
        new Payline { pattern = new int[] { 1, 2, 1, 2, 1 },},

        // 14-16: Hard diagonals
        new Payline { pattern = new int[] { 0, 1, 1, 1, 2 },},
        new Payline { pattern = new int[] { 2, 1, 1, 1, 0 },},
        new Payline { pattern = new int[] { 0, 1, 2, 2, 2 },},
        };

        // Calculates total winnings based on the current grid and bet per line
        public int CalculateWin(Symbol[,] symbolGrid, int betPerLine)
        {
            int totalWin = 0;

            // Loop through each payline
            foreach (Payline line in paylines)
            {
                Symbol firstSymbol = symbolGrid[0, line.pattern[0]]; // Get the first symbol of the line
                bool isWinningLine = true;
                int consecutiveCount = 1;

                // Check symbols across the reels
                for (int reel = 1; reel < symbolGrid.GetLength(0); reel++)
                {
                    Symbol currentSymbol = symbolGrid[reel, line.pattern[reel]];

                    // If the symbol matches the first one, or is a Wild, increase consecutive counter
                    if (currentSymbol.symbolType == firstSymbol.symbolType ||
                        currentSymbol.symbolType == SymbolType.Wild)
                    {
                        consecutiveCount++;
                    }
                    // Special case: if the first symbol was a Wild, reassign first symbol
                    else if (firstSymbol.symbolType == SymbolType.Wild)
                    {
                        firstSymbol = currentSymbol;
                        consecutiveCount = 1; // reset the count
                    }
                    else
                    {
                        isWinningLine = false; // Not matching, break the line
                        break;
                    }
                }

                // Only consider it a win if at least 3 consecutive matching symbols
                if (isWinningLine && consecutiveCount >= 3)
                {
                    int winAmount = CalculatePayout(firstSymbol.symbolType, consecutiveCount, betPerLine);
                    totalWin += winAmount;

                    // Play win animation on the symbols in the winning line
                    for (int reel = 0; reel < symbolGrid.GetLength(0); reel++)
                    {
                        symbolGrid[reel, line.pattern[reel]].PlayWinAnimation();
                    }
                }
            }
            return totalWin; // Return total winnings for this spin
        }

        // Calculates payout based on symbol type, count of matches, and bet per line
        private int CalculatePayout(SymbolType symbolType, int count, int betPerLine)
        {
            // Different payout rules depending on symbol type
            switch (symbolType)
            {
                case SymbolType.One:
                    return count switch
                    {
                        3 => betPerLine * 5,
                        4 => betPerLine * 15,
                        5 => betPerLine * 50,
                        _ => 0
                    };
                case SymbolType.Two:
                case SymbolType.Three:
                    return count switch
                    {
                        3 => betPerLine * 3,
                        4 => betPerLine * 10,
                        5 => betPerLine * 25,
                        _ => 0
                    };
                case SymbolType.Four:
                case SymbolType.Five:
                    return count switch
                    {
                        3 => betPerLine * 2,
                        4 => betPerLine * 8,
                        5 => betPerLine * 20,
                        _ => 0
                    };
                case SymbolType.J:
                case SymbolType.Q:
                case SymbolType.K:
                    return count switch
                    {
                        3 => betPerLine * 1,
                        4 => betPerLine * 4,
                        5 => betPerLine * 10,
                        _ => 0
                    };
                case SymbolType.scatter:
                    // Scatter symbols usually pay anywhere, not only on lines
                    return count switch
                    {
                        3 => betPerLine * 5,
                        4 => betPerLine * 20,
                        5 => betPerLine * 100,
                        _ => 0
                    };
                case SymbolType.Wild:
                    // Wilds usually substitute, but could have their own payouts too
                    return count switch
                    {
                        3 => betPerLine * 10,
                        4 => betPerLine * 50,
                        5 => betPerLine * 200,
                        _ => 0
                    };
                default:
                    return 0;
            }
        }
    }
}
