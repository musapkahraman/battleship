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

namespace BattleshipGame.Core
{
    public class PlanManager : MonoBehaviour
    {
        [SerializeField] private GameObject popUpPrefab;
        [SerializeField] private ButtonController clearButton;
        [SerializeField] private ButtonController randomButton;
        [SerializeField] private ButtonController continueButton;
        [SerializeField] private Map map;
        [SerializeField] private Rules rules;
        [SerializeField] private PlacementMap placementMap;
        private int _cellCount;
        private int[] _cells;
        private NetworkClient _client;
        private bool _leavePopUpIsOn;
        private NetworkManager _networkManager;
        private SortedDictionary<int, Ship> _pool;
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
            clearButton.SetText("Clear");
            randomButton.SetText("Random");
            continueButton.SetText("Continue");
            clearButton.AddListener(ResetPlacementMap);
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
            _client.SendPlacement(_cells);
            _networkManager.SetStatusText("Waiting for the opponent to place the ships.");
        }

        private void PlaceShipsRandomly()
        {
            var from = new List<int>();
            for (var i = 0; i < _cellCount; i++) from.Add(i);
            foreach (int key in _pool.Keys.ToList())
            {
                var isPlaced = false;
                while (!isPlaced)
                {
                    if (from.Count == 0) break;

                    int cell = from[Random.Range(0, from.Count)];
                    from.Remove(cell);
                    isPlaced = PlaceShip(_pool[key], GridUtils.ToCoordinate(cell, MapAreaSize.x));
                }

                if (isPlaced) continue;
                _networkManager.SetStatusText("Sorry, cannot place the ships that way!");
                break;
            }

            randomButton.SetInteractable(false);
        }

        private void ResetPlacementMap()
        {
            BeginShipPlacement();
            map.ClearAllShips();
        }

        private void BeginShipPlacement()
        {
            randomButton.SetInteractable(true);
            clearButton.SetInteractable(false);
            continueButton.SetInteractable(false);
            _networkManager.SetStatusText("Place your Ships!");
            placementMap.placements.Clear();
            _cells = new int[_cellCount];
            for (var i = 0; i < _cellCount; i++) _cells[i] = -1;
            PopulateShipPool();
        }

        private void PopulateShipPool()
        {
            _pool = new SortedDictionary<int, Ship>();
            var index = 0;
            foreach (var ship in Ships)
                for (var i = 0; i < ship.amount; i++)
                {
                    _pool.Add(index, ship);
                    index++;
                }
        }

        private int GetKeyAndRemoveFromPool(Object ship)
        {
            int key = -1;
            foreach (var kvp in _pool.Where(kvp => kvp.Value == ship))
            {
                key = kvp.Key;
                break;
            }

            _pool.Remove(key);
            return key;
        }

        public bool PlaceShip(Ship ship, Vector3Int cellCoordinate)
        {
            if (!_pool.ContainsValue(ship)) return false;
            (int shipWidth, int shipHeight) = ship.GetShipSize();
            if (!GridUtils.DoesShipFitIn(shipWidth, shipHeight, cellCoordinate, MapAreaSize.x)) return false;
            if (DoesCollideWithOtherShip(cellCoordinate, shipWidth, shipHeight)) return false;
            clearButton.SetInteractable(true);
            AddCellToPlacementMap(ship, cellCoordinate, GetKeyAndRemoveFromPool(ship));
            map.SetShip(ship, cellCoordinate, false);
            placementMap.placements.Add(new PlacementMap.Placement {ship = ship, Coordinate = cellCoordinate});
            if (_pool.Count != 0) return true;
            continueButton.SetInteractable(true);
            _networkManager.SetStatusText("Looks like you are ready.");
            return true;
        }

        private void AddCellToPlacementMap(Ship ship, Vector3Int cellCoordinate, int shipTypeIndex)
        {
            foreach (int cellIndex in ship.PartCoordinates
                .Select(p => new Vector3Int(cellCoordinate.x + p.x, cellCoordinate.y + p.y, 0))
                .Select(coordinate => GridUtils.ToCellIndex(coordinate, MapAreaSize.x)))
                _cells[cellIndex] = shipTypeIndex;
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
                    if (cellIndex >= 0 && cellIndex < _cellCount && _cells[cellIndex] < 0) continue;
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
                    BeginShipPlacement();
                    break;
                case "battle":
                    ProjectScenesManager.Instance.GoToBattleScene();
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
                ProjectScenesManager.Instance.GoToLobby();
            }
        }

        private void LeaveGame()
        {
            _client.LeaveRoom();
            ProjectScenesManager.Instance.GoToLobby();
        }

        private PopUpWindow BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpWindow>();
        }
    }
}