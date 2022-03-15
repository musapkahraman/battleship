using System.Collections.Generic;
using System.Linq;
using BattleshipGame.AI;
using BattleshipGame.Core;
using BattleshipGame.Network;
using BattleshipGame.Tiling;
using BattleshipGame.UI;
using Colyseus.Schema;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static BattleshipGame.Core.StatusData.Status;
using static BattleshipGame.Core.GridUtils;

namespace BattleshipGame.Managers
{
    public class BattleManager : MonoBehaviour, IBattleMapClickListener, ITurnClickListener
    {
        [SerializeField] private Options options;
        [SerializeField] private Rules rules;
        [SerializeField] private BattleMap userMap;
        [SerializeField] private BattleMap opponentMap;
        [SerializeField] private PlacementMap placementMap;
        [SerializeField] private OpponentStatus opponentStatus;
        [SerializeField] private TurnHighlighter opponentTurnHighlighter;
        [SerializeField] private TurnHighlighter opponentStatusMapTurnHighlighter;
        [SerializeField] private ButtonController fireButton;
        [SerializeField] private ButtonController leaveButton;
        [SerializeField] private MessageDialog leaveMessageDialog;
        [SerializeField] private MessageDialog leaveNotRematchMessageDialog;
        [SerializeField] private OptionDialog winnerOptionDialog;
        [SerializeField] private OptionDialog loserOptionDialog;
        [SerializeField] private OptionDialog leaveConfirmationDialog;
        [SerializeField] private StatusData statusData;
        private readonly Dictionary<int, List<int>> _shots = new Dictionary<int, List<int>>();
        private readonly List<int> _shotsInCurrentTurn = new List<int>();
        private IClient _client;
        private string _enemy;
        private bool _leavePopUpIsOn;
        private string _player;
        private State _state;

        private void Awake()
        {
            if (GameManager.TryGetInstance(out var gameManager))
            {
                _client = gameManager.Client;
                _client.GamePhaseChanged += OnGamePhaseChanged;
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        private void Start()
        {
            opponentMap.SetClickListener(this);
            opponentTurnHighlighter.SetClickListener(this);
            opponentStatusMapTurnHighlighter.SetClickListener(this);

            foreach (var placement in placementMap.GetPlacements())
                userMap.SetShip(placement.ship, placement.Coordinate);

            statusData.State = BeginBattle;
            leaveButton.AddListener(LeaveGame);
            fireButton.AddListener(FireShots);
            fireButton.SetInteractable(false);

            _state = _client.GetRoomState();
            _player = _state.players[_client.GetSessionId()].sessionId;

            foreach (string key in _state.players.Keys)
                if (key != _client.GetSessionId())
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

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) LeaveGame();
        }

        private void OnDestroy()
        {
            placementMap.Clear();
            if (_client == null) return;
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

        public void OnOpponentMapClicked(Vector3Int cell)
        {
            if (_state.playerTurn != _client.GetSessionId()) return;
            int cellIndex = CoordinateToCellIndex(cell, rules.areaSize);
            if (_shotsInCurrentTurn.Contains(cellIndex))
            {
                _shotsInCurrentTurn.Remove(cellIndex);
                opponentMap.ClearMarker(cell);
            }
            else if (_shotsInCurrentTurn.Count < rules.shotsPerTurn &&
                     opponentMap.SetMarker(cellIndex, Marker.MarkedTarget))
            {
                _shotsInCurrentTurn.Add(cellIndex);
            }

            fireButton.SetInteractable(_shotsInCurrentTurn.Count == rules.shotsPerTurn);
            opponentMap.IsMarkingTargets = _shotsInCurrentTurn.Count != rules.shotsPerTurn;
        }

        public void HighlightShotsInTheSameTurn(Vector3Int coordinate)
        {
            int cellIndex = CoordinateToCellIndex(coordinate, rules.areaSize);
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
            opponentTurnHighlighter.HighlightTurnShotsOnOpponentMap(_shots[turn]);
            opponentStatusMapTurnHighlighter.HighlightTurnShotsOnOpponentStatusMap(turn);
        }

        private void OnGamePhaseChanged(string phase)
        {
            switch (phase)
            {
                case RoomPhase.Battle:
                    SwitchTurns();
                    break;
                case RoomPhase.Result:
                    ShowResult();
                    break;
                case RoomPhase.Waiting:
                    if (_leavePopUpIsOn) break;
                    leaveMessageDialog.Show(GoBackToLobby);
                    break;
                case RoomPhase.Leave:
                    _leavePopUpIsOn = true;
                    leaveNotRematchMessageDialog.Show(GoBackToLobby);
                    break;
            }

            static void GoBackToLobby()
            {
                GameSceneManager.Instance.GoToLobby();
            }

            void ShowResult()
            {
                statusData.State = BattleResult;
                if (_state.winningPlayer == _client.GetSessionId())
                    winnerOptionDialog.Show(Rematch, Leave);
                else
                    loserOptionDialog.Show(Rematch, Leave);

                void Rematch()
                {
                    _client.SendRematch(true);
                    statusData.State = WaitingOpponentRematchDecision;
                }

                void Leave()
                {
                    _client.SendRematch(false);
                    LeaveGame();
                }
            }
        }

        private void FireShots()
        {
            fireButton.SetInteractable(false);
            if (_shotsInCurrentTurn.Count == rules.shotsPerTurn)
                _client.SendTurn(_shotsInCurrentTurn.ToArray());
            _shotsInCurrentTurn.Clear();
        }

        private void LeaveGame()
        {
            leaveConfirmationDialog.Show(() =>
            {
                _client.LeaveRoom();
                if (_client is NetworkClient)
                {
                    GameSceneManager.Instance.GoToLobby();
                }
                else
                {
                    statusData.State = MainMenu;
                    GameSceneManager.Instance.GoToMenu();
                }
            });
        }

        private void SwitchTurns()
        {
            if (_state.playerTurn == _client.GetSessionId())
                TurnToPlayer();
            else
                TurnToEnemy();

            void TurnToPlayer()
            {
                opponentMap.IsMarkingTargets = true;
                statusData.State = PlayerTurn;
                opponentMap.FlashGrids();

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
                if (options.vibration && _client is NetworkClient _)
                {
                    Handheld.Vibrate();
                }
#endif
            }

            void TurnToEnemy()
            {
                statusData.State = OpponentTurn;
            }
        }
        
        private void OnStateChanged(List<DataChange> changes)
        {
            foreach (var _ in changes.Where(change => change.Field == RoomState.PlayerTurn))
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
                    _shots.Add(turn, new List<int> { cellIndex });

                return;
            }

            userMap.SetMarker(cellIndex, !(from placement in placementMap.GetPlacements()
                from part in placement.ship.partCoordinates
                select placement.Coordinate + (Vector3Int)part
                into partCoordinate
                let shot = CellIndexToCoordinate(cellIndex, rules.areaSize.x)
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