using System;
using BattleshipGame.Core;
using BattleshipGame.Localization;
using UnityEngine;
using static BattleshipGame.Core.StatusData.Status;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(LocalizedText))]
    public class StatusTextController : MonoBehaviour
    {
        [SerializeField] private StatusData statusData;
        [SerializeField] private Key statusSelectMode;
        [SerializeField] private Key statusConnecting;
        [SerializeField] private Key statusNetworkError;
        [SerializeField] private Key statusWaitingJoin;
        [SerializeField] private Key statusPlacementImpossible;
        [SerializeField] private Key statusPlacementReady;
        [SerializeField] private Key statusWaitingPlace;
        [SerializeField] private Key statusPlaceShips;
        [SerializeField] private Key statusWaitingDecision;
        [SerializeField] private Key statusAiSelect;
        [SerializeField] private Key statusWaitingAttack;
        [SerializeField] private Key statusPlayerTurn;
        [SerializeField] private Key statusOptions;
        [SerializeField] private Key statusLanguageOptions;
        private LocalizedText _statusText;

        private void Awake()
        {
            _statusText = GetComponent<LocalizedText>();
            statusData.StateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            statusData.StateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(StatusData.Status state)
        {
            switch (state)
            {
                case GameStart:
                    _statusText.SetText(statusSelectMode);
                    break;
                case MainMenu:
                    _statusText.ClearText();
                    break;
                case NetworkError:
                    _statusText.SetText(statusNetworkError);
                    break;
                case BeginLobby:
                    _statusText.ClearText();
                    break;
                case Connecting:
                    _statusText.SetText(statusConnecting);
                    break;
                case WaitingOpponentJoin:
                    _statusText.SetText(statusWaitingJoin);
                    break;
                case BeginPlacement:
                    _statusText.SetText(statusPlaceShips);
                    break;
                case PlacementImpossible:
                    _statusText.SetText(statusPlacementImpossible);
                    break;
                case PlacementReady:
                    _statusText.SetText(statusPlacementReady);
                    break;
                case WaitingOpponentPlacement:
                    _statusText.SetText(statusWaitingPlace);
                    break;
                case BeginBattle:
                    _statusText.ClearText();
                    break;
                case PlayerTurn:
                    _statusText.SetText(statusPlayerTurn);
                    break;
                case OpponentTurn:
                    _statusText.SetText(statusWaitingAttack);
                    break;
                case BattleResult:
                    _statusText.ClearText();
                    break;
                case WaitingOpponentRematchDecision:
                    _statusText.SetText(statusWaitingDecision);
                    break;
                case OptionsMenu:
                    _statusText.SetText(statusOptions);
                    break;
                case LanguageOptionsMenu:
                    _statusText.SetText(statusLanguageOptions);
                    break;
                case AiSelectionMenu:
                    _statusText.SetText(statusAiSelect);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}