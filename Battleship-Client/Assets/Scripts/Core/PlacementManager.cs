using System;
using System.Collections.Generic;
using BattleshipGame.Common;
using BattleshipGame.Network;
using BattleshipGame.Schemas;
using BattleshipGame.ScriptableObjects;
using BattleshipGame.TilePaint;
using BattleshipGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace BattleshipGame.Core
{
    public class PlacementManager : MonoBehaviour
    {
        [SerializeField] private GameObject popUpPrefab;
        [SerializeField] private ShipTilePainter map;
        [SerializeField] private Rules rules;
        [SerializeField] private PlacementMap placementMap;
        private int _cellCount;
        private NetworkClient _client;
        private Ship _currentShipToBePlaced;
        private bool _isShipPlacementComplete;
        private bool _leavePopUpIsOn;
        private NetworkManager _networkManager;
        private int[] _placementMap;
        private int _shipsPlaced;
        private Queue<Ship> _shipsToBePlaced;
        private State _state;
        private Vector2Int MapAreaSize => rules.AreaSize;
        private IEnumerable<Ship> Ships => rules.ships;

        private void Awake()
        {
            if (NetworkManager.TryGetInstance(out _networkManager))
            {
                _client = _networkManager.Client;
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
            if (_client.RoomState != null) OnInitialRoomStateReceived(_client.RoomState);
        }

        private void OnDestroy()
        {
            if (_client == null) return;
            _client.InitialRoomStateReceived -= OnInitialRoomStateReceived;
            _client.GamePhaseChanged -= OnGamePhaseChanged;
        }

        public event Action RandomAvailable;
        public event Action RandomHidden;
        public event Action ContinueAvailable;
        public event Action ContinueHidden;

        private void OnInitialRoomStateReceived(State initialState)
        {
            _state = initialState;
            OnGamePhaseChanged(_state.phase);
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
            placementMap.placements.Clear();
            _shipsPlaced = 0;
            _placementMap = new int[_cellCount];
            for (var i = 0; i < _cellCount; i++) _placementMap[i] = -1;
            map.ClearAllShips();
            _isShipPlacementComplete = false;
        }

        private void PlaceShip(Vector3Int cellCoordinate)
        {
            if (!GridUtils.DoesShipFitIn(_currentShipToBePlaced, cellCoordinate, MapAreaSize.x)) return;
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

            map.SetShip(_currentShipToBePlaced, cellCoordinate);
            placementMap.placements.Add(new PlacementMap.Placement
                {ship = _currentShipToBePlaced, Coordinate = cellCoordinate});
            if (_shipsToBePlaced.Count > 0)
            {
                _currentShipToBePlaced = _shipsToBePlaced.Dequeue();
                _shipsPlaced++;
                return;
            }

            _isShipPlacementComplete = true;
            ContinueAvailable?.Invoke();

            bool AddCellToPlacementMap(Vector3Int coordinate, bool testOnly = false)
            {
                int cellIndex = GridUtils.ToCellIndex(coordinate, MapAreaSize.x);
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

        public void ContinueAfterPlacementComplete()
        {
            ContinueHidden?.Invoke();
            RandomHidden?.Invoke();
            _client.SendPlacement(_placementMap);
            WaitForOpponentPlaceShips();

            void WaitForOpponentPlaceShips()
            {
                _networkManager.SetStatusText("Waiting for the opponent to place the ships!");
            }
        }

        private void OnGamePhaseChanged(string phase)
        {
            switch (phase)
            {
                case "place":
                    BeginShipPlacement();
                    break;
                case "battle":
                    SceneLoader.Instance.GoToGameScene();
                    break;
                case "result":
                    break;
                case "waiting":
                    if (_leavePopUpIsOn) break;
                    BuildPopUp().Show("Sorry..", "Your opponent has quit the game.", "OK", GoBackToLobby);
                    break;
                case "leave":
                    break;
            }

            void GoBackToLobby()
            {
                Debug.Log($"[{name}] Loading scene: <color=yellow>lobbyScene</color>");
                SceneLoader.Instance.GoToLobby();
            }

            void BeginShipPlacement()
            {
                RandomAvailable?.Invoke();
                _networkManager.SetStatusText("Place your Ships!");
                PopulateShipsToBePlaced();
                _currentShipToBePlaced = _shipsToBePlaced.Dequeue();
            }
        }

        private void LeaveGame()
        {
            _client.LeaveRoom();
            Debug.Log($"[{name}] Loading scene: <color=yellow>lobbyScene</color>");
            SceneLoader.Instance.GoToLobby();
        }

        private PopUpCanvas BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpCanvas>();
        }
    }
}