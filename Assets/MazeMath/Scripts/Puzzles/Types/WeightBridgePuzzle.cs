using System;
using System.Collections.Generic;
using MazeMath.Puzzles;

namespace MazeMath.Puzzles.Types
{
    public sealed class WeightBridgePuzzle : IEnvironmentPuzzle
    {
        private readonly Dictionary<string, int> placedWeights = new Dictionary<string, int>();

        public string PuzzleId { get; }
        public PuzzleState State { get; private set; } = PuzzleState.Ready;
        public int TargetWeight { get; }
        public int CurrentWeight { get; private set; }

        public WeightBridgePuzzle(string puzzleId, int targetWeight)
        {
            if (string.IsNullOrWhiteSpace(puzzleId))
                throw new ArgumentException("Puzzle id is required.", nameof(puzzleId));
            if (targetWeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetWeight));

            PuzzleId = puzzleId;
            TargetWeight = targetWeight;
        }

        public void StartPuzzle()
        {
            if (State != PuzzleState.Solved)
                State = PuzzleState.Active;
        }

        public bool PlaceWeight(string objectId, int weight)
        {
            if (State != PuzzleState.Active || string.IsNullOrWhiteSpace(objectId) || weight <= 0)
                return false;

            if (placedWeights.TryGetValue(objectId, out var previous))
                CurrentWeight -= previous;

            placedWeights[objectId] = weight;
            CurrentWeight += weight;
            EvaluateSolved();
            return true;
        }

        public bool RemoveWeight(string objectId)
        {
            if (!placedWeights.TryGetValue(objectId, out var weight))
                return false;

            placedWeights.Remove(objectId);
            CurrentWeight -= weight;
            if (State == PuzzleState.Solved)
                State = PuzzleState.Active;
            return true;
        }

        public void ResetPuzzle()
        {
            placedWeights.Clear();
            CurrentWeight = 0;
            State = PuzzleState.Ready;
        }

        public bool IsSolved() => State == PuzzleState.Solved;

        private void EvaluateSolved()
        {
            if (CurrentWeight == TargetWeight)
                State = PuzzleState.Solved;
            else if (State == PuzzleState.Solved)
                State = PuzzleState.Active;
        }
    }
}
