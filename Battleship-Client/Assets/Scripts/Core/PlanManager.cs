using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Network;
using BattleshipGame.Schemas;
using BattleshipGame.ScriptableObjects;
using BattleshipGame.TilePaint;
using BattleshipGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BattleshipGame.Common.GridUtils;

namespace BattleshipGame.Core
{
    public class PlanManager : MonoBehaviour
    {
        private const int EmptyCell = -1;
        [SerializeField] private GameObject popUpPrefab;
        [SerializeField] private ButtonController leaveButton;
        [SerializeField] private ButtonController clearButton;
        [SerializeField] private ButtonController randomButton;
        [SerializeField] private ButtonController continueButton;
        [SerializeField] private Map map;
        [SerializeField] private GridSpriteMapper gridSpriteMapper;
        [SerializeField] private Rules rules;
        [SerializeField] private PlacementMap placementMap;
        private int _cellCount;
        private int[] _cells;
        private NetworkClient _client;
        private bool _leavePopUpIsOn;
        private NetworkManager _networkManager;

        private bool _opponentExists;
        private List<PlacementMap.Placement> _placements = new List<PlacementMap.Placement>();
        private SortedDictionary<int, Ship> _pool;
        private List<int> _shipsNotDragged = new List<int>();
        private State _state;
        private Vector2Int MapAreaSize => rules.AreaSize;
        private IEnumerable<Ship> Ships => rules.ships;

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
            _cellCount = MapAreaSize.x * MapAreaSize.y;
            if (_client.RoomState != null) OnFirstRoomStateReceived(_client.RoomState);

            leaveButton.SetText("Leave");
            clearButton.SetText("Clear");
            randomButton.SetText("Random");
            continueButton.SetText("Continue");

            leaveButton.AddListener(LeaveGame);
            clearButton.AddListener(OnClearButtonPressed);
            randomButton.AddListener(PlaceShipsRandomly);
            continueButton.AddListener(CompletePlacement);
        }

        private void OnDestroy()
        {
            if (_client == null) return;
            _client.FirstRoomStateReceived -= OnFirstRoomStateReceived;
            _client.GamePhaseChanged -= OnGamePhaseChanged;
        }

        private void OnFirstRoomStateReceived(State initialState)
        {
            _state = initialState;
            OnGamePhaseChanged(_state.phase);
        }

        private void CompletePlacement()
        {
            continueButton.SetInteractable(false);
            randomButton.SetInteractable(false);
            clearButton.SetInteractable(false);
            _client.SendPlacement(_cells);
            _networkManager.SetStatusText("Waiting for the opponent to place the ships.");
        }

        private void PlaceShipsRandomly()
        {
            ResetPlacementMap();

            if (_shipsNotDragged.Count == 0) _shipsNotDragged = _pool.Keys.ToList();

            foreach (var placement in _placements.Where(placement => !_shipsNotDragged.Contains(placement.shipId)))
            {
                map.SetShip(placement.ship, placement.Coordinate);
                RegisterShipToCells(placement.shipId, placement.ship, placement.Coordinate);
                placementMap.PlaceShip(placement.shipId, placement.ship, placement.Coordinate);
            }

            var from = new List<int>();
            for (var i = 0; i < _cellCount; i++) from.Add(i);

            foreach (int key in _shipsNotDragged)
            {
                var isPlaced = false;
                while (!isPlaced)
                {
                    if (from.Count == 0) break;

                    int cell = from[Random.Range(0, from.Count)];
                    from.Remove(cell);
                    isPlaced = PlaceShip(_pool[key], default, CellIndexToCoordinate(cell, MapAreaSize.x), false, key);
                }

                if (isPlaced) continue;
                _networkManager.SetStatusText("Sorry, cannot place the ships that way!");
                break;
            }

            gridSpriteMapper.CacheSpritePositions();
            continueButton.SetInteractable(true);
            _networkManager.SetStatusText("Looks like you are ready.");
        }

        private void OnClearButtonPressed()
        {
            _shipsNotDragged.Clear();
            ResetPlacementMap();
        }

        private void ResetPlacementMap()
        {
            BeginShipPlacement();
            map.ClearAllShips();
            gridSpriteMapper.ClearSpritePositions();
            gridSpriteMapper.CacheSpritePositions();
        }

        private void BeginShipPlacement()
        {
            randomButton.SetInteractable(true);
            clearButton.SetInteractable(false);
            continueButton.SetInteractable(false);
            _networkManager.SetStatusText("Place your Ships!");
            placementMap.Clear();
            _cells = new int[_cellCount];
            for (var i = 0; i < _cellCount; i++) _cells[i] = EmptyCell;
            PopulateShipPool();

            void PopulateShipPool()
            {
                _pool = new SortedDictionary<int, Ship>();
                var shipId = 0;
                foreach (var ship in Ships)
                    for (var i = 0; i < ship.amount; i++)
                    {
                        _pool.Add(shipId, ship);
                        shipId++;
                    }
            }
        }

        private int GetShipId(Ship ship, Vector3Int grabbedFrom, bool isMovedIn)
        {
            if (!isMovedIn)
            {
                int cellIndex = CoordinateToCellIndex(grabbedFrom, MapAreaSize);
                if (_cells[cellIndex] != EmptyCell) return _cells[cellIndex];
            }

            foreach (var kvp in _pool.Where(kvp => kvp.Value.rankOrder == ship.rankOrder)) return kvp.Key;

            return EmptyCell;
        }

        public bool PlaceShip(Ship ship, Vector3Int from, Vector3Int to, bool isMovedIn, int shipId = EmptyCell)
        {
            var shouldRemoveFromPool = false;
            if (shipId == EmptyCell)
            {
                shipId = GetShipId(ship, from, isMovedIn);
                shouldRemoveFromPool = true;
            }

            (int shipWidth, int shipHeight) = ship.GetShipSize();
            if (!DoesShipFitIn(shipWidth, shipHeight, to, MapAreaSize.x)) return false;
            if (DoesCollideWithOtherShip(shipId, to, shipWidth, shipHeight)) return false;
            clearButton.SetInteractable(true);
            map.SetShip(ship, to);
            RegisterShipToCells(shipId, ship, to);
            placementMap.PlaceShip(shipId, ship, to);
            if (shouldRemoveFromPool)
            {
                _pool.Remove(shipId);
                _shipsNotDragged = _pool.Keys.ToList();
                _placements = placementMap.GetPlacements();
            }

            if (_pool.Count != 0) return true;
            randomButton.SetInteractable(false);
            continueButton.SetInteractable(true);
            _networkManager.SetStatusText("Looks like you are ready.");
            return true;
        }

        private void RegisterShipToCells(int shipId, Ship ship, Vector3Int pivot)
        {
            // Clear the previous placement of this ship
            for (var i = 0; i < _cellCount; i++)
                if (_cells[i] == shipId)
                    _cells[i] = EmptyCell;

            // Find each cell the ship covers and register the ship on them
            foreach (int cellIndex in ship.PartCoordinates
                .Select(part => new Vector3Int(pivot.x + part.x, pivot.y + part.y, 0))
                .Select(coordinate => CoordinateToCellIndex(coordinate, MapAreaSize)))
                if (cellIndex != OutOfMap)
                    _cells[cellIndex] = shipId;
        }

        private bool DoesCollideWithOtherShip(int selfShipId, Vector3Int cellCoordinate, int shipWidth, int shipHeight)
        {
            // Create a frame of one cell thickness
            int xMin = cellCoordinate.x - 1;
            int xMax = cellCoordinate.x + shipWidth;
            int yMin = cellCoordinate.y - shipHeight;
            int yMax = cellCoordinate.y + 1;
            for (int y = yMin; y <= yMax; y++)
            {
                if (y < 0 || y > MapAreaSize.y - 1) continue; // Avoid this row if it is out of the map
                for (int x = xMin; x <= xMax; x++)
                {
                    if (x < 0 || x > MapAreaSize.x - 1) continue; // Avoid this column if it is out of the map
                    int cellIndex = CoordinateToCellIndex(new Vector3Int(x, y, 0), MapAreaSize);
                    if (cellIndex != OutOfMap &&
                        (_cells[cellIndex] == EmptyCell || _cells[cellIndex] == selfShipId)) continue;

                    return true;
                }
            }

            return false;
        }

        private void OnGamePhaseChanged(string phase)
        {
            switch (phase)
            {
                case "place":
                    _opponentExists = true;
                    BeginShipPlacement();
                    break;
                case "battle":
                    GameSceneManager.Instance.GoToBattleScene();
                    break;
                case "result":
                    break;
                case "waiting":
                    _opponentExists = false;
                    if (_leavePopUpIsOn) break;
                    BuildPopUp().Show("Sorry..", "Your opponent has quit the game.", "OK", GoBackToLobby);
                    break;
                case "leave":
                    break;
            }

            void GoBackToLobby()
            {
                if (_opponentExists)
                    OnClearButtonPressed();
                else
                    GameSceneManager.Instance.GoToLobby();
            }
        }

        private void LeaveGame()
        {
            _client.LeaveRoom();
            GameSceneManager.Instance.GoToLobby();
        }

        private PopUpWindow BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpWindow>();
        }
    }
}