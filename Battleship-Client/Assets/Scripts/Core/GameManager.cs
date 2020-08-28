using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Common;
using BattleshipGame.Network;
using BattleshipGame.Schemas;
using BattleshipGame.ScriptableObjects;
using BattleshipGame.UI;
using Colyseus.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace BattleshipGame.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageField;
        [SerializeField] private GameObject popUpPrefab;
        [SerializeField] private MapViewer userMap;
        [SerializeField] private MapViewer opponentMap;
        [SerializeField] private OpponentStatusMaskPlacer opponentStatusMaskPlacer;
        [SerializeField] private Rules rules;
        private readonly List<int> _shots = new List<int>();
        private int _cellCount;
        private NetworkClient _client;
        private Ship _currentShipToBePlaced;
        private bool _isShipPlacementComplete;
        private GameMode _mode;
        private int[] _placementMap;
        private int _playerNumber;
        private int _shipsPlaced;
        private Queue<Ship> _shipsToBePlaced;
        private State _state;
        private bool _leavePopUpIsOn;
        public Vector2Int MapAreaSize => rules.AreaSize;
        public IEnumerable<Ship> Ships => rules.ships;

        private void Awake()
        {
            if (NetworkManager.TryGetInstance(out var networkManager))
            {
                _client = networkManager.Client;
                _client.InitialRoomStateReceived += OnInitialRoomStateReceived;
                _client.GamePhaseChanged += OnGamePhaseChanged;
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        private void Start()
        {
            _cellCount = MapAreaSize.x * MapAreaSize.y;
            ResetPlacementMap();
            opponentMap.SetDisabled();
            if (_client.RoomState != null) OnInitialRoomStateReceived(_client.RoomState);
        }

        private void OnDestroy()
        {
            if (_client == null) return;
            _client.InitialRoomStateReceived -= OnInitialRoomStateReceived;
            _client.GamePhaseChanged -= OnGamePhaseChanged;
            UnRegisterFromStateEvents();

            void UnRegisterFromStateEvents()
            {
                _state.OnChange -= OnStateChanged;
                _state.player1Shots.OnChange -= OnFirstPlayerShotsChanged;
                _state.player2Shots.OnChange -= OnSecondPlayerShotsChanged;
                _state.player1Ships.OnChange -= OnFirstPlayerShipsChanged;
                _state.player2Ships.OnChange -= OnSecondPlayerShipsChanged;
            }
        }

        public event Action FireReady;
        public event Action FireNotReady;
        public event Action RandomAvailable;
        public event Action RandomHidden;
        public event Action ContinueAvailable;
        public event Action ContinueHidden;

        private void OnInitialRoomStateReceived(State initialState)
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

        public void PlaceShipsRandomly()
        {
            ResetPlacementMap();
            PopulateShipsToBePlaced();
            _currentShipToBePlaced = _shipsToBePlaced.Dequeue();
            while (!_isShipPlacementComplete)
                PlaceShip(new Vector3Int(Random.Range(0, MapAreaSize.x), Random.Range(0, MapAreaSize.y), 0));
        }

        private void ResetPlacementMap()
        {
            _shipsPlaced = 0;
            _placementMap = new int[_cellCount];
            for (var i = 0; i < _cellCount; i++) _placementMap[i] = -1;
            userMap.ClearAllShips();
            _isShipPlacementComplete = false;
        }

        public void PlaceShip(Vector3Int cellCoordinate)
        {
            if (_mode != GameMode.Placement) return;
            if (!DoesShipFitIn(_currentShipToBePlaced, cellCoordinate)) return;
            (int shipWidth, int shipHeight) = _currentShipToBePlaced.GetShipSize();
            int xMin = cellCoordinate.x - 1;
            int xMax = cellCoordinate.x + shipWidth;
            int yMin = cellCoordinate.y - shipHeight;
            int yMax = cellCoordinate.y + 1;
            for (int y = yMin; y <= yMax; y++)
            {
                if (y < 0 || y > MapAreaSize.y - 1) continue;
                for (int x = xMin; x <= xMax; x++)
                {
                    if (x < 0 || x > MapAreaSize.x - 1) continue;
                    if (!AddCellToPlacementMap(new Vector3Int(x, y, 0), true)) return;
                }
            }

            foreach (var p in _currentShipToBePlaced.PartCoordinates)
                AddCellToPlacementMap(new Vector3Int(cellCoordinate.x + p.x, cellCoordinate.y + p.y, 0));

            userMap.SetShip(_currentShipToBePlaced, cellCoordinate);
            if (_shipsToBePlaced.Count > 0)
            {
                _currentShipToBePlaced = _shipsToBePlaced.Dequeue();
                _shipsPlaced++;
                UpdateCursor();
                return;
            }

            _isShipPlacementComplete = true;
            userMap.SetDisabled();
            ContinueAvailable?.Invoke();

            bool AddCellToPlacementMap(Vector3Int coordinate, bool testOnly = false)
            {
                int cellIndex = GridConverter.ToCellIndex(coordinate, MapAreaSize.y);
                if (cellIndex < 0 || cellIndex >= _cellCount) return false;
                if (_placementMap[cellIndex] >= 0) return false;
                if (testOnly) return true;
                _placementMap[cellIndex] = _shipsPlaced;
                return true;
            }
        }

        private void PopulateShipsToBePlaced()
        {
            _shipsToBePlaced = new Queue<Ship>();
            foreach (var ship in Ships)
                for (var i = 0; i < ship.amount; i++)
                    _shipsToBePlaced.Enqueue(ship);
        }

        public bool DoesShipFitIn(Ship ship, Vector3Int cellCoordinate)
        {
            (int shipWidth, int shipHeight) = ship.GetShipSize();
            return cellCoordinate.x >= 0 && cellCoordinate.x + shipWidth <= MapAreaSize.x &&
                   cellCoordinate.y - (shipHeight - 1) >= 0;
        }

        public void ContinueAfterPlacementComplete()
        {
            ContinueHidden?.Invoke();
            RandomHidden?.Invoke();
            _client.SendPlacement(_placementMap);
            WaitForOpponentPlaceShips();

            void WaitForOpponentPlaceShips()
            {
                _mode = GameMode.Placement;
                userMap.SetDisabled();
                opponentMap.SetDisabled();
                messageField.text = "Waiting for the Opponent to place the ships!";
            }
        }

        public void MarkTarget(Vector3Int targetCoordinate)
        {
            int targetIndex = GridConverter.ToCellIndex(targetCoordinate, MapAreaSize.y);
            if (_shots.Contains(targetIndex))
            {
                _shots.Remove(targetIndex);
                opponentMap.ClearMarkerTile(targetIndex);
            }
            else if (_shots.Count < rules.shotsPerTurn && opponentMap.SetMarker(targetIndex, Marker.MarkedTarget))
            {
                _shots.Add(targetIndex);
            }

            if (_shots.Count == rules.shotsPerTurn)
                FireReady?.Invoke();
            else
                FireNotReady?.Invoke();
        }

        public void FireShots()
        {
            if (_shots.Count == rules.shotsPerTurn)
                _client.SendTurn(_shots.ToArray());
        }

        private void UpdateCursor()
        {
            userMap.SetCursorTile(_currentShipToBePlaced.tile);
        }

        private void OnGamePhaseChanged(string phase)
        {
            switch (phase)
            {
                case "place":
                    BeginShipPlacement();
                    break;
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
                        .Show("Sorry..", "Your opponent has decided not to continue for another round.", "OK", GoBackToLobby);
                    break;
            }

            void GoBackToLobby()
            {
                SceneManager.LoadScene(1);
            }

            void BeginShipPlacement()
            {
                _mode = GameMode.Placement;
                RandomAvailable?.Invoke();
                messageField.text = "Place your Ships!";
                userMap.SetPlacementMode();
                PopulateShipsToBePlaced();
                _currentShipToBePlaced = _shipsToBePlaced.Dequeue();
                UpdateCursor();
            }

            void ShowResult()
            {
                _mode = GameMode.Result;
                userMap.SetDisabled();
                opponentMap.SetDisabled();
                messageField.text = "";

                bool isWinner = _state.winningPlayer == _playerNumber;
                string headerText = isWinner ? "You Win!" : "You Lost!!!";
                string messageText = isWinner ? "Winners never quit!" : "Quitters never win!";
                string declineButtonText = isWinner ? "Quit" : "Give Up";
                BuildPopUp().Show(headerText, messageText, "Rematch", declineButtonText, () =>
                {
                    _client.SendRematch(true);
                    messageField.text = "Waiting for the opponent decide.";
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
            SceneManager.LoadScene(1);
        }

        private void CheckTurn()
        {
            if (_state.playerTurn == _playerNumber)
                StartTurn();
            else
                WaitForOpponentTurn();

            void StartTurn()
            {
                _shots.Clear();
                _mode = GameMode.Battle;
                userMap.SetDisabled();
                opponentMap.SetAttackMode();
                messageField.text = "It's Your Turn!";
            }

            void WaitForOpponentTurn()
            {
                _mode = GameMode.Battle;
                userMap.SetDisabled();
                opponentMap.SetDisabled();
                messageField.text = "Waiting for the Opponent to Attack!";
            }
        }

        private void OnStateChanged(List<DataChange> changes)
        {
            foreach (var dataChange in changes)
                Debug.Log($"<color=#63B5B5>{dataChange.Field}:</color> " +
                          $"{dataChange.PreviousValue} <color=green>-></color> {dataChange.Value}");

            foreach (var _ in changes.Where(change => change.Field == "playerTurn"))
                CheckTurn();
        }

        private void OnFirstPlayerShotsChanged(int value, int key)
        {
            const int playerNumber = 1;
            SetMarker(key, playerNumber);
        }

        private void OnSecondPlayerShotsChanged(int value, int key)
        {
            const int playerNumber = 2;
            SetMarker(key, playerNumber);
        }

        private void SetMarker(int item, int playerNumber)
        {
            Marker marker;
            if (_playerNumber == playerNumber)
            {
                marker = Marker.ShotTarget;
                opponentMap.SetMarker(item, marker);
            }
            else
            {
                marker = _placementMap[item] == -1 ? Marker.Missed : Marker.Hit;
                userMap.SetMarker(item, marker);
            }
        }

        private void OnFirstPlayerShipsChanged(int value, int key)
        {
            const int playerNumber = 1;
            SetHitPoints(key, value, playerNumber);
        }

        private void OnSecondPlayerShipsChanged(int value, int key)
        {
            const int playerNumber = 2;
            SetHitPoints(key, value, playerNumber);
        }

        private void SetHitPoints(int item, int index, int playerNumber)
        {
            if (_playerNumber != playerNumber) opponentStatusMaskPlacer.PlaceMask(item, index);
        }

        private PopUpCanvas BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpCanvas>();
        }

        private enum GameMode
        {
            Placement,
            Battle,
            Result
        }
    }
}