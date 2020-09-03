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
    public class PlacementManager : MonoBehaviour
    {
        [SerializeField] private GameObject popUpPrefab;
        [SerializeField] private ButtonController clearButton;
        [SerializeField] private ButtonController randomButton;
        [SerializeField] private ButtonController continueButton;
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
        private Queue<Ship> _shipsPoolRandom;
        private SortedList<int, Ship> _shipPoolDrag;
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
            if (_client.RoomState != null) OnInitialRoomStateReceived(_client.RoomState);
            clearButton.SetText("Clear");
            randomButton.SetText("Random");
            continueButton.SetText("Continue");
            clearButton.AddListener(ResetPlacementMap);
            randomButton.AddListener(PlaceShipsRandomly);
            continueButton.AddListener(CompletePlacement);
            clearButton.SetInteractable(false);
            continueButton.SetInteractable(false);
        }

        private void OnDestroy()
        {
            if (_client == null) return;
            _client.InitialRoomStateReceived -= OnInitialRoomStateReceived;
            _client.GamePhaseChanged -= OnGamePhaseChanged;
        }

        private void OnInitialRoomStateReceived(State initialState)
        {
            _state = initialState;
            OnGamePhaseChanged(_state.phase);
        }

        private void CompletePlacement()
        {
            continueButton.SetInteractable(false);
            randomButton.SetInteractable(false);
            _client.SendPlacement(_placementMap);
            _networkManager.SetStatusText("Waiting for the opponent to place the ships.");
        }

        private void PlaceShipsRandomly()
        {
            Debug.Log("Resetting placement map onClick:Random");
            ResetPlacementMap();
            _currentShipToBePlaced = _shipsPoolRandom.Dequeue();
            while (!_isShipPlacementComplete)
                PlaceShip(new Vector3Int(Random.Range(0, MapAreaSize.x), Random.Range(0, MapAreaSize.y), 0));
        }

        private void ResetPlacementMap()
        {
            Debug.Log("Beginning ship placement map in ResetPlacementMap");
            BeginShipPlacement();
            _shipsPlaced = 0;
            _isShipPlacementComplete = false;
            map.ClearAllShips();
        }

        private void BeginShipPlacement()
        {
            randomButton.SetInteractable(true);
            _networkManager.SetStatusText("Place your Ships!");
            placementMap.placements.Clear();
            _placementMap = new int[_cellCount];
            for (var i = 0; i < _cellCount; i++) _placementMap[i] = -1;
            PopulateShipPool();
        }

        private void PopulateShipPool()
        {
            _shipsPoolRandom = new Queue<Ship>();
            foreach (var ship in Ships)
                for (var i = 0; i < ship.amount; i++)
                    _shipsPoolRandom.Enqueue(ship);

            _shipPoolDrag = new SortedList<int, Ship>();
            var index = 0;
            foreach (var ship in Ships)
                for (var i = 0; i < ship.amount; i++)
                {
                    _shipPoolDrag.Add(index, ship);
                    index++;
                }
        }

        public bool PlaceShipOnDrag(Ship ship, Vector3Int cellCoordinate)
        {
            if (!_shipPoolDrag.ContainsValue(ship)) return false;
            (int shipWidth, int shipHeight) = ship.GetShipSize();
            if (DoesCollideWithOtherShip(cellCoordinate, shipWidth, shipHeight)) return false;
            clearButton.SetInteractable(true);
            int index = _shipPoolDrag.IndexOfValue(ship);
            AddCellToPlacementMap(ship, cellCoordinate, _shipPoolDrag.Keys[index]);
            map.SetShip(ship, cellCoordinate, false);
            placementMap.placements.Add(new PlacementMap.Placement
                {ship = ship, Coordinate = cellCoordinate});
            _shipPoolDrag.RemoveAt(index);
            if (_shipPoolDrag.Count > 0) return true;
            _isShipPlacementComplete = true;
            continueButton.SetInteractable(true);
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
            if (_shipsPoolRandom.Count > 0)
            {
                _currentShipToBePlaced = _shipsPoolRandom.Dequeue();
                _shipsPlaced++;
                return;
            }

            _isShipPlacementComplete = true;
            continueButton.SetInteractable(true);
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

        private void OnGamePhaseChanged(string phase)
        {
            switch (phase)
            {
                case "place":
                    Debug.Log("Beginning ship placement map in OnGamePhaseChanged to place");
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