using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Core;
using BattleshipGame.Network;
using BattleshipGame.Tiling;
using BattleshipGame.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static BattleshipGame.Core.StatusData.Status;
using static BattleshipGame.Core.GridUtils;

namespace BattleshipGame.Managers
{
    public class PlanManager : MonoBehaviour, IPlanMapMoveListener
    {
        private const int EmptyCell = -1;
        [SerializeField] private MessageDialog leaveMessageDialog;
        [SerializeField] private ButtonController leaveButton;
        [SerializeField] private ButtonController clearButton;
        [SerializeField] private ButtonController randomButton;
        [SerializeField] private ButtonController continueButton;
        [SerializeField] private PlanMap planMap;
        [SerializeField] private GridSpriteMapper gridSpriteMapper;
        [SerializeField] private GridSpriteMapper poolGridSpriteMapper;
        [SerializeField] private Rules rules;
        [SerializeField] private PlacementMap placementMap;
        [SerializeField] private StatusData statusData;
        private int _cellCount;
        private int[] _cells;
        private IClient _client;
        private List<PlacementMap.Placement> _placements = new List<PlacementMap.Placement>();
        private SortedDictionary<int, Ship> _pool;
        private List<int> _shipsNotDragged = new List<int>();
        private Vector2Int MapAreaSize => rules.areaSize;

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
            planMap.SetPlaceListener(this);

            _cellCount = MapAreaSize.x * MapAreaSize.y;
            leaveButton.AddListener(LeaveGame);
            clearButton.AddListener(OnClearButtonPressed);
            randomButton.AddListener(PlaceShipsRandomly);
            continueButton.AddListener(CompletePlacement);

            BeginShipPlacement();

            void OnClearButtonPressed()
            {
                _shipsNotDragged.Clear();
                ResetPlacementMap();
            }

            void PlaceShipsRandomly()
            {
                ResetPlacementMap();

                if (_shipsNotDragged.Count == 0) _shipsNotDragged = _pool.Keys.ToList();

                // Avoid ships that the player dragged into the map.
                foreach (var placement in _placements.Where(placement => !_shipsNotDragged.Contains(placement.shipId)))
                {
                    planMap.SetShip(placement.ship, placement.Coordinate);
                    RegisterShipToCells(placement.shipId, placement.ship, placement.Coordinate);
                    placementMap.PlaceShip(placement.shipId, placement.ship, placement.Coordinate);
                }

                // Place the remaining ships randomly
                foreach (int shipId in _shipsNotDragged)
                {
                    var uncheckedCells = new List<int>();
                    for (var i = 0; i < _cellCount; i++) uncheckedCells.Add(i);
                    var isPlaced = false;
                    while (!isPlaced)
                    {
                        if (uncheckedCells.Count == 0) break;

                        int cell = uncheckedCells[Random.Range(0, uncheckedCells.Count)];
                        uncheckedCells.Remove(cell);
                        isPlaced = PlaceShip(_pool[shipId], default, CellIndexToCoordinate(cell, MapAreaSize.x), false,
                            shipId);
                    }

                    if (!isPlaced)
                    {
                        statusData.State = PlacementImpossible;
                        clearButton.SetInteractable(true);
                        randomButton.SetInteractable(false);
                        return;
                    }
                }

                gridSpriteMapper.CacheSpritePositions();
                poolGridSpriteMapper.CacheSpritePositions();
                continueButton.SetInteractable(true);
                statusData.State = PlacementReady;
            }

            void CompletePlacement()
            {
                continueButton.SetInteractable(false);
                randomButton.SetInteractable(false);
                clearButton.SetInteractable(false);
                _client.SendPlacement(_cells);
                statusData.State = WaitingOpponentPlacement;
            }

            void ResetPlacementMap()
            {
                BeginShipPlacement();
                planMap.ClearAllShips();
                gridSpriteMapper.ClearSpritePositions();
                gridSpriteMapper.CacheSpritePositions();
                poolGridSpriteMapper.ClearSpritePositions();
                poolGridSpriteMapper.CacheSpritePositions();
            }

            void BeginShipPlacement()
            {
                randomButton.SetInteractable(true);
                clearButton.SetInteractable(false);
                continueButton.SetInteractable(false);
                statusData.State = BeginPlacement;
                placementMap.Clear();
                _cells = new int[_cellCount];
                for (var i = 0; i < _cellCount; i++) _cells[i] = EmptyCell;
                PopulateShipPool();

                void PopulateShipPool()
                {
                    _pool = new SortedDictionary<int, Ship>();
                    var shipId = 0;
                    foreach (var ship in rules.ships)
                        for (var i = 0; i < ship.amount; i++)
                        {
                            _pool.Add(shipId, ship);
                            shipId++;
                        }
                }
            }
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) LeaveGame();
        }

        private void OnDestroy()
        {
            if (_client == null) return;
            _client.GamePhaseChanged -= OnGamePhaseChanged;
        }

        public bool OnShipMoved(Ship ship, Vector3Int from, Vector3Int to, bool isMovedIn)
        {
            return PlaceShip(ship, from, to, isMovedIn);
        }

        private void LeaveGame()
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
        }

        private void OnGamePhaseChanged(string phase)
        {
            switch (phase)
            {
                case RoomPhase.Battle:
                    GameSceneManager.Instance.GoToBattleScene();
                    break;
                case RoomPhase.Result:
                    break;
                case RoomPhase.Waiting:
                    leaveMessageDialog.Show(() => GameSceneManager.Instance.GoToLobby());
                    break;
                case RoomPhase.Leave:
                    break;
            }
        }

        private bool PlaceShip(Ship ship, Vector3Int from, Vector3Int to, bool isMovedIn, int shipId = EmptyCell)
        {
            var shouldRemoveFromPool = false;
            if (shipId == EmptyCell)
            {
                shipId = GetShipId(from);
                shouldRemoveFromPool = true;
            }

            (int shipWidth, int shipHeight) = ship.GetShipSize();
            if (!IsInsideBoundaries(shipWidth, shipHeight, to, MapAreaSize)) return false;
            if (DoesCollideWithOtherShip(shipId, to)) return false;
            clearButton.SetInteractable(true);
            planMap.SetShip(ship, to);
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
            statusData.State = PlacementReady;
            return true;

            int GetShipId(Vector3Int grabbedFrom)
            {
                if (!isMovedIn)
                {
                    int cellIndex = CoordinateToCellIndex(grabbedFrom, MapAreaSize);
                    if (_cells[cellIndex] != EmptyCell) return _cells[cellIndex];
                }

                foreach (var kvp in _pool.Where(kvp => kvp.Value.rankOrder == ship.rankOrder)) return kvp.Key;

                return EmptyCell;
            }

            bool DoesCollideWithOtherShip(int selfShipId, Vector3Int cellCoordinate)
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
        }

        private void RegisterShipToCells(int shipId, Ship ship, Vector3Int pivot)
        {
            // Clear the previous placement of this ship
            for (var i = 0; i < _cellCount; i++)
                if (_cells[i] == shipId)
                    _cells[i] = EmptyCell;

            // Find each cell the ship covers and register the ship on them
            foreach (int cellIndex in ship.partCoordinates
                .Select(part => new Vector3Int(pivot.x + part.x, pivot.y + part.y, 0))
                .Select(coordinate => CoordinateToCellIndex(coordinate, MapAreaSize)))
                if (cellIndex != OutOfMap)
                    _cells[cellIndex] = shipId;
        }
    }
}