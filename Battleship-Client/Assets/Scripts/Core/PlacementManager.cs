using System;
using System.Collections.Generic;
using System.Linq;
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
        private Queue<Ship> _shipsToBePlacedRandomly;
        private SortedList<int, Ship> _shipsToBePlacedDragging;
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
            _currentShipToBePlaced = _shipsToBePlacedRandomly.Dequeue();
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

        public bool PlaceShipOnDrag(Ship ship, Vector3Int cellCoordinate)
        {
            if (!_shipsToBePlacedDragging.ContainsValue(ship)) return false;
            (int shipWidth, int shipHeight) = ship.GetShipSize();
            if (DoesCollideWithOtherShip(cellCoordinate, shipWidth, shipHeight)) return false;
            int index = _shipsToBePlacedDragging.IndexOfValue(ship);
            AddCellToPlacementMap(ship, cellCoordinate, _shipsToBePlacedDragging.Keys[index]);
            map.SetShip(ship, cellCoordinate, false);
            placementMap.placements.Add(new PlacementMap.Placement
                {ship = ship, Coordinate = cellCoordinate});
            _shipsToBePlacedDragging.RemoveAt(index);
            if (_shipsToBePlacedDragging.Count > 0) return true;
            _isShipPlacementComplete = true;
            ContinueAvailable?.Invoke();
            return true;
        }

        private void PlaceShip(Vector3Int cellCoordinate)
        {
            (int shipWidth, int shipHeight) = _currentShipToBePlaced.GetShipSize();
            if (!GridUtils.DoesShipFitIn(shipWidth, shipHeight, cellCoordinate, MapAreaSize.x)) return;
            if (DoesCollideWithOtherShip(cellCoordinate, shipWidth, shipHeight)) return;
            AddCellToPlacementMap(_currentShipToBePlaced, cellCoordinate, _shipsPlaced);
            map.SetShip(_currentShipToBePlaced, cellCoordinate, false);
            placementMap.placements.Add(new PlacementMap.Placement
                {ship = _currentShipToBePlaced, Coordinate = cellCoordinate});
            if (_shipsToBePlacedRandomly.Count > 0)
            {
                _currentShipToBePlaced = _shipsToBePlacedRandomly.Dequeue();
                _shipsPlaced++;
                return;
            }

            _isShipPlacementComplete = true;
            ContinueAvailable?.Invoke();
        }

        private void AddCellToPlacementMap(Ship ship, Vector3Int cellCoordinate, int placedShipIndex)
        {
            foreach (int cellIndex in ship.PartCoordinates
                .Select(p => new Vector3Int(cellCoordinate.x + p.x, cellCoordinate.y + p.y, 0))
                .Select(coordinate => GridUtils.ToCellIndex(coordinate, MapAreaSize.x)))
            {
                _placementMap[cellIndex] = placedShipIndex;
                Debug.Log($"Added {placedShipIndex} to array at index: [{cellIndex}] for {ship.name}");
            }
        }

        private bool DoesCollideWithOtherShip(Vector3Int cellCoordinate, int shipWidth, int shipHeight)
        {
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
                    int cellIndex = GridUtils.ToCellIndex(new Vector3Int(x, y, 0), MapAreaSize.x);
                    if (cellIndex >= 0 && cellIndex < _cellCount && _placementMap[cellIndex] < 0) continue;
                    return true;
                }
            }

            return false;
        }

        private void PopulateShipsToBePlaced()
        {
            _shipsToBePlacedRandomly = new Queue<Ship>();
            foreach (var ship in Ships)
                for (var i = 0; i < ship.amount; i++)
                    _shipsToBePlacedRandomly.Enqueue(ship);

            _shipsToBePlacedDragging = new SortedList<int, Ship>();
            var index = 0;
            foreach (var ship in Ships)
                for (var i = 0; i < ship.amount; i++)
                {
                    _shipsToBePlacedDragging.Add(index, ship);
                    index++;
                }
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
                _currentShipToBePlaced = _shipsToBePlacedRandomly.Dequeue();
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