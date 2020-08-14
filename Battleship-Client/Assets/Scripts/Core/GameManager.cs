using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Common;
using BattleshipGame.Network;
using BattleshipGame.UI;
using Colyseus.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Core
{
    public class GameManager : MonoBehaviour
    {
        private const int ShotsPerTurn = 3;
        private static int _cellCount;
        private static GameClient _client;
        private static GameMode _mode;
        private static int _myPlayerNumber;
        private static int[] _placement;
        private static int _shipsPlaced;
        private static readonly List<int> Shots = new List<int>();
        private static State _state;
        [SerializeField] private OpponentStatusMaskPlacer opponentStatusMaskPlacer;
        [SerializeField] private TMP_Text messageField;
        [SerializeField] private MapViewer opponentMap;
        [SerializeField] private int size = 9;
        [SerializeField] private MapViewer userMap;
        [SerializeField] private List<Ship> ships;
        public int MapSize => size;

        public event Action FireReady;
        public event Action FireNotReady;

        private void Start()
        {
            _client = GameClient.Instance;
            if (!_client.Connected) SceneManager.LoadScene("ConnectingScene");
            _cellCount = size * size;
            _placement = new int[_cellCount];
            for (var i = 0; i < _cellCount; i++) _placement[i] = -1;
            _client.InitialStateReceived += OnInitialStateReceived;
            _client.GamePhaseChanged += OnGamePhaseChanged;
            if (_client.State != null) OnInitialStateReceived(this, _client.State);
            opponentMap.SetDisabled();
        }

        private void OnDestroy()
        {
            _client.InitialStateReceived -= OnInitialStateReceived;
            _client.GamePhaseChanged -= OnGamePhaseChanged;
        }

        private void BeginShipPlacement()
        {
            _mode = GameMode.Placement;
            messageField.text = "Place your Ships!";
            _shipsPlaced = 0;
            userMap.SetPlacementMode();
            UpdateCursor();
        }

        private void WaitForOpponentPlaceShips()
        {
            _mode = GameMode.Placement;
            userMap.SetDisabled();
            opponentMap.SetDisabled();
            messageField.text = "Waiting for the Opponent to place the ships!";
        }

        private void WaitForOpponentTurn()
        {
            _mode = GameMode.Battle;
            userMap.SetDisabled();
            opponentMap.SetDisabled();
            messageField.text = "Waiting for the Opponent to Attack!";
        }

        private void StartTurn()
        {
            Shots.Clear();
            _mode = GameMode.Battle;
            userMap.SetDisabled();
            opponentMap.SetAttackMode();
            messageField.text = "It's Your Turn!";
        }

        private void ShowResult()
        {
            _mode = GameMode.Result;
            userMap.SetDisabled();
            opponentMap.SetDisabled();
            messageField.text = _state.winningPlayer == _myPlayerNumber ? "You Win!" : "You Lost!!!";
        }

        public void PlaceShip(Vector3Int coordinate)
        {
            if (_mode != GameMode.Placement) return;
            var ship = ships[_shipsPlaced];
            (int shipWidth, int shipHeight) = ship.GetShipSize();
            if (coordinate.x < 0 || coordinate.x + shipWidth > size || coordinate.y - (shipHeight - 1) < 0) return;
            int xMin = coordinate.x - 1;
            int xMax = coordinate.x + shipWidth;
            int yMin = coordinate.y - shipHeight;
            int yMax = coordinate.y + 1;
            for (int y = yMin; y <= yMax; y++)
            {
                if (y < 0 || y > size - 1) continue;
                for (int x = xMin; x <= xMax; x++)
                {
                    if (x < 0 || x > size - 1) continue;
                    if (!SetPlacementCell(new Vector3Int(x, y, 0), true)) return;
                }
            }

            foreach (var p in ship.PartCoordinates)
            {
                SetPlacementCell(new Vector3Int(coordinate.x + p.x, coordinate.y+ p.y, 0));
            }

            userMap.SetShip((ShipType) _shipsPlaced, coordinate);
            _shipsPlaced++;
            UpdateCursor();
            if (_shipsPlaced != 9) return;
            _client.SendPlacement(_placement);
            WaitForOpponentPlaceShips();
        }

        public void MarkTarget(Vector3Int targetCoordinate)
        {
            int targetIndex = GridConverter.ToCellIndex(targetCoordinate, size);
            if (Shots.Contains(targetIndex))
            {
                Shots.Remove(targetIndex);
                opponentMap.ClearTile(targetIndex);
            }
            else if (Shots.Count < ShotsPerTurn && opponentMap.SetMarker(targetIndex, Marker.TargetMarked))
            {
                Shots.Add(targetIndex);
            }

            if (Shots.Count == ShotsPerTurn)
                FireReady?.Invoke();
            else
                FireNotReady?.Invoke();
        }

        public static void FireShots()
        {
            _client.SendTurn(Shots.ToArray());
        }

        private void UpdateCursor()
        {
            userMap.SetShipCursor((ShipType) _shipsPlaced);
        }

        private bool SetPlacementCell(Vector3Int coordinate, bool testOnly = false)
        {
            int cellIndex = GridConverter.ToCellIndex(coordinate, size);
            if (cellIndex < 0 || cellIndex >= _cellCount) return false;
            if (_placement[cellIndex] >= 0) return false;
            if (testOnly) return true;
            _placement[cellIndex] = _shipsPlaced;
            return true;
        }

        private void OnInitialStateReceived(object sender, State initialState)
        {
            _state = initialState;
            var user = _state.players[_client.SessionId];
            _myPlayerNumber = user?.seat ?? -1;
            _state.OnChange += OnStateChanged;
            _state.player1Shots.OnChange += OnFirstPlayerShotsChanged;
            _state.player2Shots.OnChange += OnSecondPlayerShotsChanged;
            _state.player1Ships.OnChange += OnFirstPlayerShipsChanged;
            _state.player2Ships.OnChange += OnSecondPlayerShipsChanged;
            OnGamePhaseChanged(this, _state.phase);
        }

        private void OnStateChanged(object sender, OnChangeEventArgs args)
        {
            foreach (var _ in args.Changes.Where(change => change.Field == "playerTurn"))
                CheckTurn();
        }

        private void OnFirstPlayerShotsChanged(object sender, KeyValueEventArgs<int, int> change)
        {
            const int playerNumber = 1;
            SetMarker(change, playerNumber);
        }

        private void OnSecondPlayerShotsChanged(object sender, KeyValueEventArgs<int, int> change)
        {
            const int playerNumber = 2;
            SetMarker(change, playerNumber);
        }

        private void SetMarker(KeyValueEventArgs<int, int> change, int playerNumber)
        {
            Marker marker;
            if (_myPlayerNumber == playerNumber)
            {
                marker = Marker.TargetShot;
                opponentMap.SetMarker(change.Key, marker);
            }
            else
            {
                marker = _placement[change.Key] == -1 ? Marker.Missed : Marker.Hit;
                userMap.SetMarker(change.Key, marker);
            }
        }

        private void OnFirstPlayerShipsChanged(object sender, KeyValueEventArgs<int, int> change)
        {
            const int playerNumber = 1;
            SetHealth(change, playerNumber);
        }

        private void OnSecondPlayerShipsChanged(object sender, KeyValueEventArgs<int, int> change)
        {
            const int playerNumber = 2;
            SetHealth(change, playerNumber);
        }

        private void SetHealth(KeyValueEventArgs<int, int> change, int playerNumber)
        {
            if (_myPlayerNumber != playerNumber)
            {
                opponentStatusMaskPlacer.PlaceMask(change.Key, change.Value);
            }
        }

        private void CheckTurn()
        {
            if (_state.playerTurn == _myPlayerNumber)
                StartTurn();
            else
                WaitForOpponentTurn();
        }

        private static void Leave()
        {
            _client.Leave();
            SceneManager.LoadScene("ConnectingScene");
        }

        private void OnGamePhaseChanged(object sender, string phase)
        {
            switch (phase)
            {
                case "waiting":
                    Leave();
                    break;
                case "place":
                    BeginShipPlacement();
                    break;
                case "battle":
                    CheckTurn();
                    break;
                case "result":
                    ShowResult();
                    break;
            }
        }

        private enum GameMode
        {
            Placement,
            Battle,
            Result
        }
    }
}