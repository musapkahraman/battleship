using System;
using UnityEngine;

namespace BattleshipGame.Core
{
    [CreateAssetMenu(menuName = "Battleship/Game State Container")]
    public class StatusData : ScriptableObject
    {
        private Status _state;

        public Status State
        {
            get => _state;
            set
            {
                _state = value;
                StateChanged?.Invoke(_state);
            }
        }

        public event Action<Status> StateChanged;

        public enum Status
        {
            GameStart,
            MainMenu,
            OptionsMenu,
            LanguageOptionsMenu,
            AiSelectionMenu,
            NetworkError,
            BeginLobby,
            Connecting,
            WaitingOpponentJoin,
            BeginPlacement,
            PlacementImpossible,
            PlacementReady,
            WaitingOpponentPlacement,
            BeginBattle,
            PlayerTurn,
            OpponentTurn,
            BattleResult,
            WaitingOpponentRematchDecision
        }
    }
}