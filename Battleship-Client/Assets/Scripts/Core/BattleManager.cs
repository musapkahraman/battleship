using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Common;
using BattleshipGame.Network;
using BattleshipGame.Schemas;
using BattleshipGame.ScriptableObjects;
using BattleshipGame.TilePaint;
using BattleshipGame.UI;
using Colyseus.Schema;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BattleshipGame.Common.GridUtils;
using static BattleshipGame.Common.MapInteractionMode;

namespace BattleshipGame.Core
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private GameObject popUpPrefab;
        [SerializeField] private ButtonController leaveButton;
        [SerializeField] private ButtonController fireButton;
        [SerializeField] private ButtonController markButton;
        [SerializeField] private ButtonController dragButton;
        [SerializeField] private ButtonController highlightButton;
        [SerializeField] private BattleMap userMap;
        [SerializeField] private BattleMap opponentMap;
        [SerializeField] private OpponentStatus opponentStatus;
        [SerializeField] private Rules rules;
        [SerializeField] private PlacementMap placementMap;
        private readonly Dictionary<int, List<int>> _shots = new Dictionary<int, List<int>>();
        private readonly List<int> _shotsInCurrentTurn = new List<int>();
        private NetworkClient _client;
        private bool _leavePopUpIsOn;
        private NetworkManager _networkManager;
        private int _playerNumber;
        private State _state;
        private Vector2Int MapAreaSize => rules.AreaSize;

        private void Awake()
        {
            if (NetworkManager.TryGetInstance(out _networkManager))
            {
                _client = _networkManager.Client;
                _client.FirstRoomStateReceived += OnFirstRoomStateReceived;
                _client.GamePhaseChanged += OnGamePhaseChanged;
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        private void Start()
        {
            foreach (var placement in placementMap.GetPlacements())
                userMap.SetShip(placement.ship, placement.Coordinate, default);

            opponentMap.InteractionMode = NoInteraction;
            _networkManager.ClearStatusText();

            leaveButton.SetText("Leave");
            fireButton.SetText("Fire!");
            markButton.SetText("Mark");
            dragButton.SetText("Drag");
            highlightButton.SetText("Highlight");

            leaveButton.AddListener(LeaveGame);
            fireButton.AddListener(FireShots);
            markButton.AddListener(SwitchToMarkTargetsMode);
            dragButton.AddListener(SwitchToDragShipsMode);
            highlightButton.AddListener(SwitchToHighlightTurnMode);

            fireButton.SetInteractable(false);
            markButton.SetInteractable(false);
            dragButton.SetInteractable(false);
            highlightButton.SetInteractable(false);

            if (_client.RoomState != null) OnFirstRoomStateReceived(_client.RoomState);
        }

        private void OnDestroy()
        {
            placementMap.Clear();
            if (_client == null) return;
            _client.FirstRoomStateReceived -= OnFirstRoomStateReceived;
            _client.GamePhaseChanged -= OnGamePhaseChanged;
            UnRegisterFromStateEvents();

            void UnRegisterFromStateEvents()
            {
                if (_state == null) return;
                _state.OnChange -= OnStateChanged;
                _state.player1Shots.OnChange -= OnFirstPlayerShotsChanged;
                _state.player2Shots.OnChange -= OnSecondPlayerShotsChanged;
                _state.player1Ships.OnChange -= OnFirstPlayerShipsChanged;
                _state.player2Ships.OnChange -= OnSecondPlayerShipsChanged;
            }
        }

        private void OnFirstRoomStateReceived(State initialState)
        {
            _state = initialState;
            var player = _state.players[_client.SessionId];
            if (player != null)
                _playerNumber = player.seat;
            else
                _playerNumber = -1;
            RegisterToStateEvents();
            OnGamePhaseChanged(_state.phase);

            void RegisterToStateEvents()
            {
                _state.OnChange += OnStateChanged;
                _state.player1Shots.OnChange += OnFirstPlayerShotsChanged;
                _state.player2Shots.OnChange += OnSecondPlayerShotsChanged;
                _state.player1Ships.OnChange += OnFirstPlayerShipsChanged;
                _state.player2Ships.OnChange += OnSecondPlayerShipsChanged;
            }
        }

        public void HighlightTurn(Vector3Int coordinate)
        {
            int cellIndex = CoordinateToCellIndex(coordinate, MapAreaSize);
            foreach (var keyValuePair in from keyValuePair in _shots
                from cell in keyValuePair.Value
                where cell == cellIndex
                select keyValuePair)
            {
                HighlightTurn(keyValuePair.Key);
                return;
            }
        }

        public void HighlightTurn(int turn)
        {
            if (!_shots.ContainsKey(turn)) return;
            opponentMap.HighlightTurns(_shots[turn]);
        }

        public void MarkTarget(Vector3Int targetCoordinate)
        {
            int targetIndex = CoordinateToCellIndex(targetCoordinate, MapAreaSize);
            if (_shotsInCurrentTurn.Contains(targetIndex))
            {
                _shotsInCurrentTurn.Remove(targetIndex);
                opponentMap.ClearMarker(targetCoordinate);
            }
            else if (_shotsInCurrentTurn.Count < rules.shotsPerTurn &&
                     opponentMap.SetMarker(targetIndex, Marker.MarkedTarget))
            {
                _shotsInCurrentTurn.Add(targetIndex);
            }

            fireButton.SetInteractable(_shotsInCurrentTurn.Count == rules.shotsPerTurn);
            opponentMap.IsMarkingTargets = _shotsInCurrentTurn.Count != rules.shotsPerTurn;
        }

        private void FireShots()
        {
            fireButton.SetInteractable(false);
            if (_shotsInCurrentTurn.Count == rules.shotsPerTurn)
                _client.SendTurn(_shotsInCurrentTurn.ToArray());
        }

        private void OnGamePhaseChanged(string phase)
        {
            switch (phase)
            {
                case "battle":
                    CheckTurn();
                    break;
                case "result":
                    ShowResult();
                    break;
                case "waiting":
                    if (_leavePopUpIsOn) break;
                    BuildPopUp().Show("Sorry..", "Your opponent has quit the game.", "OK", GoBackToLobby);
                    break;
                case "leave":
                    _leavePopUpIsOn = true;
                    BuildPopUp()
                        .Show("Sorry..", "Your opponent has decided not to continue for another round.", "OK",
                            GoBackToLobby);
                    break;
            }

            void GoBackToLobby()
            {
                ProjectScenesManager.Instance.GoToLobby();
            }

            void ShowResult()
            {
                userMap.InteractionMode = NoInteraction;
                opponentMap.InteractionMode = NoInteraction;
                _networkManager.ClearStatusText();

                bool isWinner = _state.winningPlayer == _playerNumber;
                string headerText = isWinner ? "You Win!" : "You Lost!!!";
                string messageText = isWinner ? "Winners never quit!" : "Quitters never win!";
                string declineButtonText = isWinner ? "Quit" : "Give Up";
                BuildPopUp().Show(headerText, messageText, "Rematch", declineButtonText, () =>
                {
                    _client.SendRematch(true);
                    _networkManager.SetStatusText("Waiting for the opponent decide.");
                }, () =>
                {
                    _client.SendRematch(false);
                    LeaveGame();
                });
            }
        }

        private void LeaveGame()
        {
            _client.LeaveRoom();
            ProjectScenesManager.Instance.GoToLobby();
        }

        private void CheckTurn()
        {
            if (_state.playerTurn == _playerNumber)
                StartTurn();
            else
                WaitForOpponentTurn();

            void StartTurn()
            {
                _shotsInCurrentTurn.Clear();
                userMap.InteractionMode = NoInteraction;
                SwitchToMarkTargetsMode();
                _networkManager.SetStatusText("It's your turn!");
            }

            void WaitForOpponentTurn()
            {
                userMap.InteractionMode = NoInteraction;
                SwitchToDragShipsMode();
                _networkManager.SetStatusText("Waiting for the opponent to attack.");
            }
        }

        private void SwitchToMarkTargetsMode()
        {
            opponentMap.InteractionMode = TargetMarking;
            markButton.SetInteractable(false);
            dragButton.SetInteractable(true);
            highlightButton.SetInteractable(true);
        }

        private void SwitchToDragShipsMode()
        {
            opponentMap.InteractionMode = ShipDragging;
            if (_state.playerTurn == _playerNumber)
                markButton.SetInteractable(true);
            dragButton.SetInteractable(false);
            highlightButton.SetInteractable(true);
        }

        private void SwitchToHighlightTurnMode()
        {
            opponentMap.InteractionMode = TurnHighlighting;
            if (_state.playerTurn == _playerNumber)
                markButton.SetInteractable(true);
            dragButton.SetInteractable(true);
            highlightButton.SetInteractable(false);
        }

        private PopUpWindow BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpWindow>();
        }

        private void OnStateChanged(List<DataChange> changes)
        {
            foreach (var _ in changes.Where(change => change.Field == "playerTurn"))
                CheckTurn();
        }

        private void OnFirstPlayerShotsChanged(int turn, int cellIndex)
        {
            const int playerNumber = 1;
            SetMarker(cellIndex, turn, playerNumber);
        }

        private void OnSecondPlayerShotsChanged(int turn, int cellIndex)
        {
            const int playerNumber = 2;
            SetMarker(cellIndex, turn, playerNumber);
        }

        private void SetMarker(int cellIndex, int turn, int playerNumber)
        {
            if (_playerNumber == playerNumber)
            {
                opponentMap.SetMarker(cellIndex, Marker.ShotTarget);
                if (_shots.ContainsKey(turn))
                    _shots[turn].Add(cellIndex);
                else
                    _shots.Add(turn, new List<int> {cellIndex});

                return;
            }

            userMap.SetMarker(cellIndex, !(from placement in placementMap.GetPlacements()
                from part in placement.ship.PartCoordinates
                select placement.Coordinate + (Vector3Int) part
                into partCoordinate
                let shot = CellIndexToCoordinate(cellIndex, MapAreaSize.x)
                where partCoordinate.Equals(shot)
                select partCoordinate).Any()
                ? Marker.Missed
                : Marker.Hit);
        }

        private void OnFirstPlayerShipsChanged(int turn, int part)
        {
            const int playerNumber = 1;
            RegisterShotParts(part, turn, playerNumber);
        }

        private void OnSecondPlayerShipsChanged(int turn, int part)
        {
            const int playerNumber = 2;
            RegisterShotParts(part, turn, playerNumber);
        }

        private void RegisterShotParts(int part, int shotTurn, int playerNumber)
        {
            if (_playerNumber != playerNumber) opponentStatus.RegisterAndDisplay(part, shotTurn);
        }
    }
}