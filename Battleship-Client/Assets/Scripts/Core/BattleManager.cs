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
        [SerializeField] private TurnHighlighter turnHighlighter;
        [SerializeField] private OpponentStatus opponentStatus;
        [SerializeField] private Rules rules;
        [SerializeField] private PlacementMap placementMap;
        private readonly Dictionary<int, List<int>> _shots = new Dictionary<int, List<int>>();
        private readonly List<int> _shotsInCurrentTurn = new List<int>();
        private NetworkClient _client;
        private bool _leavePopUpIsOn;
        private NetworkManager _networkManager;
        private string _player, _enemy;
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
                userMap.SetShip(placement.ship, placement.Coordinate);

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
                if (_state.players[_player] == null) return;
                _state.players[_player].shots.OnChange -= OnPlayerShotsChanged;
                if (_state.players[_enemy] == null) return;
                _state.players[_enemy].ships.OnChange -= OnEnemyShipsChanged;
                _state.players[_enemy].shots.OnChange -= OnEnemyShotsChanged;
            }
        }

        private void OnFirstRoomStateReceived(State initialState)
        {
            _state = initialState;
            _player = _state.players[_client.SessionId].sessionId;

            foreach (string key in _state.players.Keys)
                if (key != _client.SessionId)
                {
                    _enemy = _state.players[key].sessionId;
                    break;
                }

            RegisterToStateEvents();
            OnGamePhaseChanged(_state.phase);

            void RegisterToStateEvents()
            {
                _state.OnChange += OnStateChanged;
                _state.players[_player].shots.OnChange += OnPlayerShotsChanged;
                _state.players[_enemy].ships.OnChange += OnEnemyShipsChanged;
                _state.players[_enemy].shots.OnChange += OnEnemyShotsChanged;
            }
        }

        public void HighlightShotsInTheSameTurn(Vector3Int coordinate)
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
            turnHighlighter.HighlightTurns(_shots[turn]);
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
            markButton.SetInteractable(false);
            fireButton.SetInteractable(false);
            if (_shotsInCurrentTurn.Count == rules.shotsPerTurn)
                _client.SendTurn(_shotsInCurrentTurn.ToArray());
        }

        private void OnGamePhaseChanged(string phase)
        {
            switch (phase)
            {
                case "battle":
                    SwitchTurns();
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
                    BuildPopUp().Show("Sorry..", "Your opponent has decided not to continue for another round.", "OK",
                        GoBackToLobby);
                    break;
            }

            void GoBackToLobby()
            {
                GameSceneManager.Instance.GoToLobby();
            }

            void ShowResult()
            {
                userMap.InteractionMode = NoInteraction;
                opponentMap.InteractionMode = NoInteraction;
                _networkManager.ClearStatusText();

                bool isWinner = _state.winningPlayer == _client.SessionId;
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
            GameSceneManager.Instance.GoToLobby();
        }

        private void SwitchTurns()
        {
            if (_state.playerTurn == _client.SessionId)
                TurnToPlayer();
            else
                TurnToEnemy();

            void TurnToPlayer()
            {
                _shotsInCurrentTurn.Clear();
                userMap.InteractionMode = NoInteraction;
                if (opponentMap.InteractionMode == NoInteraction)
                    SwitchToMarkTargetsMode();
                else
                    markButton.SetInteractable(true);
                _networkManager.SetStatusText("It's your turn!");
#if UNITY_ANDROID
            Handheld.Vibrate();
#endif
            }

            void TurnToEnemy()
            {
                userMap.InteractionMode = NoInteraction;
                if (opponentMap.InteractionMode == TargetMarking)
                    SwitchToHighlightTurnMode();
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
            if (_state.playerTurn == _client.SessionId)
                markButton.SetInteractable(true);
            dragButton.SetInteractable(false);
            highlightButton.SetInteractable(true);
        }

        private void SwitchToHighlightTurnMode()
        {
            opponentMap.InteractionMode = TurnHighlighting;
            if (_state.playerTurn == _client.SessionId)
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
                SwitchTurns();
        }

        private void OnPlayerShotsChanged(int turn, int cellIndex)
        {
            if (turn <= 0) return;
            SetMarker(cellIndex, turn, true);
        }

        private void OnEnemyShotsChanged(int turn, int cellIndex)
        {
            if (turn <= 0) return;
            SetMarker(cellIndex, turn, false);
        }

        private void SetMarker(int cellIndex, int turn, bool player)
        {
            if (player)
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

        private void OnEnemyShipsChanged(int turn, int part)
        {
            opponentStatus.DisplayShotEnemyShipParts(part, turn);
        }
    }
}