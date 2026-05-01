using UnityEngine;
using System.Collections.Generic;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;

namespace PlayWild.Features.Minigames
{
    public class TreasureHunterSolver : BaseFeature
    {
        public override string Name => "Treasure Hunter Solver";

        private bool isGameActive = false;
        private bool isSolving = false;
        private bool puzzleSolved = false;
        private int gridSize = 0;
        private bool[,] solution = null;
        private Queue<Vector2Int> clickQueue = new Queue<Vector2Int>();
        private float lastClickTime = 0f;
        private float clickDelay = 0.15f;
        private string clickDelayInput = "0.15";
        private Il2CppMiniGames.TreasureHunts.TreasureHuntsGame gameInstance = null;

        public override void Initialize()
        {
            base.Initialize();
            clickDelay = GetPersistedValue("clickDelay", 0.15f);
            clickDelayInput = clickDelay.ToString("F2");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            try
            {
                var game = UnityEngine.Object.FindObjectOfType<Il2CppMiniGames.TreasureHunts.TreasureHuntsGame>();

                if (game != null)
                {
                    gameInstance = game;

                    if (!isGameActive)
                    {
                        MelonLogger.Msg("[WildBerry] Treasure Hunt detected!");
                        isGameActive = true;
                        puzzleSolved = false;
                        isSolving = false;
                        clickQueue.Clear();
                    }

                    if (isSolving)
                    {
                        if (clickQueue.Count > 0 && Time.time - lastClickTime >= clickDelay)
                            ProcessNextClick();
                        else if (clickQueue.Count == 0)
                            isSolving = false;
                    }
                }
                else if (isGameActive)
                {
                    isGameActive = false;
                    isSolving = false;
                    puzzleSolved = false;
                    gameInstance = null;
                    clickQueue.Clear();
                    solution = null;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Treasure Hunter error: {ex.Message}");
            }
        }

        private void SolvePuzzle()
        {
            try
            {
                var grid = Il2CppMiniGames.TreasureHunts.TreasureHuntsGrid.instance;
                if (grid == null)
                {
                    MelonLogger.Warning("[WildBerry] Treasure Hunt grid not found");
                    return;
                }

                var rowHintsRaw = grid.rowHints;
                var colHintsRaw = grid.columnHints;

                if (rowHintsRaw == null || colHintsRaw == null)
                {
                    MelonLogger.Warning("[WildBerry] Hints not available yet");
                    return;
                }

                gridSize = grid._gridSize;
                if (gridSize <= 0)
                    gridSize = rowHintsRaw.Length;

                MelonLogger.Msg($"[WildBerry] Grid size: {gridSize}x{gridSize}");
                MelonLogger.Msg($"[WildBerry] Row hints count: {rowHintsRaw.Length}, Col hints count: {colHintsRaw.Length}");

                var rowClues = new List<int[]>();
                var colClues = new List<int[]>();

                for (int i = rowHintsRaw.Length - 1; i >= 0; i--)
                {
                    string raw = rowHintsRaw[i];
                    MelonLogger.Msg($"[WildBerry] Raw row hint [{i}] (solver row {rowHintsRaw.Length - 1 - i}): \"{raw}\"");
                    rowClues.Add(ParseHint(raw, gridSize));
                    MelonLogger.Msg($"[WildBerry] Parsed solver row {rowClues.Count - 1}: [{string.Join(", ", rowClues[rowClues.Count - 1])}]");
                }

                for (int i = 0; i < colHintsRaw.Length; i++)
                {
                    string raw = colHintsRaw[i];
                    MelonLogger.Msg($"[WildBerry] Raw col hint [{i}]: \"{raw}\"");
                    colClues.Add(ParseHint(raw, gridSize));
                    MelonLogger.Msg($"[WildBerry] Parsed col {i}: [{string.Join(", ", colClues[i])}]");
                }

                int rowSum = 0, colSum = 0;
                foreach (var clue in rowClues)
                    foreach (int v in clue) rowSum += v;
                foreach (var clue in colClues)
                    foreach (int v in clue) colSum += v;

                MelonLogger.Msg($"[WildBerry] Validation: row sum={rowSum}, col sum={colSum} (should match)");

                if (rowSum != colSum)
                    MelonLogger.Warning("[WildBerry] Row/column sums don't match, hints may be parsed incorrectly!");

                solution = SolveNonogram(gridSize, rowClues, colClues);

                if (solution == null)
                {
                    MelonLogger.Error("[WildBerry] Could not solve nonogram puzzle");
                    return;
                }

                LogSolution();

                clickQueue.Clear();
                int filledCount = 0;

                for (int solverRow = 0; solverRow < gridSize; solverRow++)
                {
                    int gameRow = gridSize - 1 - solverRow;
                    for (int col = 0; col < gridSize; col++)
                    {
                        if (solution[solverRow, col])
                        {
                            var lockBox = grid.GetLockBox(col, gameRow);
                            if (lockBox != null && !lockBox.Opened)
                            {
                                clickQueue.Enqueue(new Vector2Int(col, gameRow));
                                filledCount++;
                            }
                            else if (lockBox != null && lockBox.Opened)
                            {
                                MelonLogger.Msg($"[WildBerry] Cell ({col},{gameRow}) already opened, skipping");
                            }
                        }
                    }
                }

                puzzleSolved = true;
                isSolving = true;
                lastClickTime = Time.time;
                MelonLogger.Msg($"[WildBerry] Puzzle solved! {filledCount} filled cells to open, {clickQueue.Count} queued");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Solve error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void LogSolution()
        {
            if (solution == null) return;

            string header = "    ";
            for (int c = 0; c < gridSize; c++) header += c + " ";
            MelonLogger.Msg($"[WildBerry] Solution:\n{header}");

            for (int r = 0; r < gridSize; r++)
            {
                string line = $"  {r} ";
                for (int c = 0; c < gridSize; c++)
                    line += (solution[r, c] ? "X " : ". ");
                MelonLogger.Msg($"[WildBerry] {line}");
            }
        }

        private void ProcessNextClick()
        {
            if (clickQueue.Count == 0)
            {
                isSolving = false;
                MelonLogger.Msg("[WildBerry] All squares processed!");
                return;
            }

            try
            {
                var cell = clickQueue.Dequeue();

                if (gameInstance != null)
                {
                    MelonLogger.Msg($"[WildBerry] Opening cell ({cell.x}, {cell.y}), {clickQueue.Count} remaining");
                    gameInstance.SendCheckSquare(cell.x, cell.y);
                    lastClickTime = Time.time;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Click error: {ex.Message}");
            }
        }

        private int[] ParseHint(string hint, int maxSize)
        {
            if (string.IsNullOrEmpty(hint) || hint.Trim() == "0")
                return new int[] { 0 };

            string trimmed = hint.Trim();

            if (trimmed.Contains(" "))
            {
                var parts = trimmed.Split(' ');
                var nums = new List<int>();
                foreach (var part in parts)
                {
                    string p = part.Trim();
                    if (!string.IsNullOrEmpty(p) && int.TryParse(p, out int val))
                        nums.Add(val);
                }
                if (nums.Count > 0)
                    return nums.ToArray();
            }

            if (trimmed.Contains("\n"))
            {
                var parts = trimmed.Split('\n');
                var nums = new List<int>();
                foreach (var part in parts)
                {
                    string p = part.Trim();
                    if (!string.IsNullOrEmpty(p) && int.TryParse(p, out int val))
                        nums.Add(val);
                }
                if (nums.Count > 0)
                    return nums.ToArray();
            }

            if (int.TryParse(trimmed, out int singleVal))
            {
                if (singleVal <= maxSize)
                    return new int[] { singleVal };

                var digits = new List<int>();
                foreach (char ch in trimmed)
                {
                    if (char.IsDigit(ch))
                        digits.Add(ch - '0');
                }
                if (digits.Count > 0)
                    return digits.ToArray();
            }

            var fallback = new List<int>();
            foreach (char ch in trimmed)
            {
                if (char.IsDigit(ch))
                    fallback.Add(ch - '0');
            }
            return fallback.Count > 0 ? fallback.ToArray() : new int[] { 0 };
        }

        private bool[,] SolveNonogram(int size, List<int[]> rowClues, List<int[]> colClues)
        {
            int[,] grid = new int[size, size];

            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    grid[r, c] = -1;

            bool changed = true;
            int iterations = 0;
            int maxIterations = size * size * 4;

            while (changed && iterations < maxIterations)
            {
                changed = false;
                iterations++;

                for (int r = 0; r < size; r++)
                {
                    int[] line = ExtractRow(grid, r, size);
                    int[] solved = SolveLine(line, rowClues[r], size);
                    for (int c = 0; c < size; c++)
                    {
                        if (grid[r, c] == -1 && solved[c] != -1)
                        {
                            grid[r, c] = solved[c];
                            changed = true;
                        }
                    }
                }

                for (int c = 0; c < size; c++)
                {
                    int[] line = ExtractCol(grid, c, size);
                    int[] solved = SolveLine(line, colClues[c], size);
                    for (int r = 0; r < size; r++)
                    {
                        if (grid[r, c] == -1 && solved[r] != -1)
                        {
                            grid[r, c] = solved[r];
                            changed = true;
                        }
                    }
                }
            }

            int unknowns = CountUnknowns(grid, size);
            MelonLogger.Msg($"[WildBerry] Constraint propagation done after {iterations} iterations, {unknowns} unknowns remaining");

            if (unknowns > 0)
            {
                MelonLogger.Msg("[WildBerry] Starting brute force search...");
                if (!BruteForce(grid, size, rowClues, colClues, 0))
                    MelonLogger.Warning("[WildBerry] Brute force could not fully resolve puzzle");
            }

            unknowns = CountUnknowns(grid, size);
            if (unknowns > 0)
                MelonLogger.Warning($"[WildBerry] {unknowns} cells still unresolved, treating as empty");

            bool[,] result = new bool[size, size];
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    result[r, c] = grid[r, c] == 1;

            return result;
        }

        private int[] ExtractRow(int[,] grid, int row, int size)
        {
            int[] line = new int[size];
            for (int c = 0; c < size; c++) line[c] = grid[row, c];
            return line;
        }

        private int[] ExtractCol(int[,] grid, int col, int size)
        {
            int[] line = new int[size];
            for (int r = 0; r < size; r++) line[r] = grid[r, col];
            return line;
        }

        private int CountUnknowns(int[,] grid, int size)
        {
            int count = 0;
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    if (grid[r, c] == -1) count++;
            return count;
        }

        private int[] SolveLine(int[] line, int[] clue, int size)
        {
            int[] result = new int[size];
            System.Array.Copy(line, result, size);

            if (clue.Length == 1 && clue[0] == 0)
            {
                for (int i = 0; i < size; i++)
                    result[i] = 0;
                return result;
            }

            var filtered = new List<bool[]>();
            GenerateAndFilter(clue, size, line, filtered);

            if (filtered.Count == 0)
                return result;

            for (int i = 0; i < size; i++)
            {
                if (result[i] != -1) continue;

                bool allFilled = true;
                bool allEmpty = true;

                foreach (var p in filtered)
                {
                    if (p[i]) allEmpty = false;
                    else allFilled = false;

                    if (!allFilled && !allEmpty) break;
                }

                if (allFilled) result[i] = 1;
                else if (allEmpty) result[i] = 0;
            }

            return result;
        }

        private void GenerateAndFilter(int[] clue, int size, int[] known, List<bool[]> results)
        {
            GeneratePlacementsRecursive(clue, 0, 0, new bool[size], size, known, results);
        }

        private void GeneratePlacementsRecursive(int[] clue, int clueIdx, int pos, bool[] current, int size, int[] known, List<bool[]> results)
        {
            if (results.Count > 100000) return;

            if (clueIdx == clue.Length)
            {
                for (int i = pos; i < size; i++)
                {
                    if (known[i] == 1) return;
                }

                bool[] copy = new bool[size];
                System.Array.Copy(current, copy, size);
                results.Add(copy);
                return;
            }

            int blockLen = clue[clueIdx];
            int remainingBlocks = 0;
            for (int i = clueIdx + 1; i < clue.Length; i++)
                remainingBlocks += clue[i] + 1;

            int maxStart = size - blockLen - remainingBlocks;

            for (int start = pos; start <= maxStart; start++)
            {
                if (start > pos && known[start - 1] == 1)
                    break;

                bool canPlace = true;
                for (int i = pos; i < start; i++)
                {
                    if (known[i] == 1) { canPlace = false; break; }
                }
                if (!canPlace) break;

                bool blockFits = true;
                for (int i = start; i < start + blockLen; i++)
                {
                    if (known[i] == 0) { blockFits = false; break; }
                }
                if (!blockFits) continue;

                int afterBlock = start + blockLen;
                if (afterBlock < size && known[afterBlock] == 1)
                    continue;

                bool[] next = new bool[size];
                System.Array.Copy(current, next, size);
                for (int i = start; i < start + blockLen; i++)
                    next[i] = true;

                GeneratePlacementsRecursive(clue, clueIdx + 1, afterBlock + 1, next, size, known, results);
            }
        }

        private bool BruteForce(int[,] grid, int size, List<int[]> rowClues, List<int[]> colClues, int cellIndex)
        {
            while (cellIndex < size * size)
            {
                int r = cellIndex / size;
                int c = cellIndex % size;
                if (grid[r, c] == -1) break;
                cellIndex++;
            }

            if (cellIndex >= size * size)
                return ValidateGrid(grid, size, rowClues, colClues);

            int row = cellIndex / size;
            int col = cellIndex % size;

            for (int val = 1; val >= 0; val--)
            {
                grid[row, col] = val;
                if (IsPartiallyValid(grid, size, rowClues, colClues, row, col))
                {
                    if (BruteForce(grid, size, rowClues, colClues, cellIndex + 1))
                        return true;
                }
            }

            grid[row, col] = -1;
            return false;
        }

        private bool IsPartiallyValid(int[,] grid, int size, List<int[]> rowClues, List<int[]> colClues, int row, int col)
        {
            bool rowComplete = true;
            for (int c = 0; c < size; c++)
            {
                if (grid[row, c] == -1) { rowComplete = false; break; }
            }
            if (rowComplete && !ValidateLine(grid, size, rowClues[row], row, true))
                return false;

            bool colComplete = true;
            for (int r = 0; r < size; r++)
            {
                if (grid[r, col] == -1) { colComplete = false; break; }
            }
            if (colComplete && !ValidateLine(grid, size, colClues[col], col, false))
                return false;

            return true;
        }

        private bool ValidateLine(int[,] grid, int size, int[] clue, int idx, bool isRow)
        {
            var runs = new List<int>();
            int run = 0;

            for (int i = 0; i < size; i++)
            {
                int val = isRow ? grid[idx, i] : grid[i, idx];
                if (val == 1) { run++; }
                else
                {
                    if (run > 0) runs.Add(run);
                    run = 0;
                }
            }
            if (run > 0) runs.Add(run);

            if (clue.Length == 1 && clue[0] == 0)
                return runs.Count == 0;

            if (runs.Count != clue.Length) return false;
            for (int i = 0; i < runs.Count; i++)
                if (runs[i] != clue[i]) return false;

            return true;
        }

        private bool ValidateGrid(int[,] grid, int size, List<int[]> rowClues, List<int[]> colClues)
        {
            for (int r = 0; r < size; r++)
                if (!ValidateLine(grid, size, rowClues[r], r, true)) return false;
            for (int c = 0; c < size; c++)
                if (!ValidateLine(grid, size, colClues[c], c, false)) return false;
            return true;
        }

        public override void OnGUI(Rect area)
        {
            float yOffset = 0;

            DrawToggle(new Rect(area.x, area.y + yOffset, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });
            yOffset += 25;

            if (IsEnabled)
            {
                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 60, 20), "Delay:");
                string newDelay = DrawStyledTextField(new Rect(area.x + 65, area.y + yOffset, 45, WildBerryTheme.TextFieldHeight), clickDelayInput);
                if (newDelay != clickDelayInput)
                {
                    clickDelayInput = newDelay;
                    if (float.TryParse(clickDelayInput, out float d) && d >= 0.05f)
                    {
                        clickDelay = d;
                        SetPersistedValue("clickDelay", clickDelay);
                    }
                }
                DrawLabel(new Rect(area.x + 115, area.y + yOffset, 20, 20), "s");
                yOffset += 28;

                if (isGameActive)
                {
                    if (!puzzleSolved)
                    {
                        if (DrawStyledButton(new Rect(area.x + 5, area.y + yOffset, 100, WildBerryTheme.ButtonHeight), "Solve Puzzle"))
                        {
                            SolvePuzzle();
                        }
                    }
                    else if (isSolving)
                    {
                        DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 20), $"Opening squares... {clickQueue.Count} remaining", WildBerryTheme.StatusWarning);
                    }
                    else
                    {
                        DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Puzzle complete!", WildBerryTheme.StatusActive);

                        yOffset += 22;
                        if (DrawStyledButton(new Rect(area.x + 5, area.y + yOffset, 80, WildBerryTheme.ButtonHeight), "Re-solve"))
                        {
                            puzzleSolved = false;
                            isSolving = false;
                            solution = null;
                            clickQueue.Clear();
                        }
                    }
                }
                else
                {
                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Treasure Hunt: Not Found", WildBerryTheme.StatusText);
                }
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return WildBerryTheme.ToggleHeight;
            if (isGameActive && puzzleSolved && !isSolving) return 100f;
            return 80f;
        }

        public override void OnDisable()
        {
            isGameActive = false;
            isSolving = false;
            puzzleSolved = false;
            clickQueue.Clear();
            solution = null;
            gameInstance = null;
        }
    }
}
