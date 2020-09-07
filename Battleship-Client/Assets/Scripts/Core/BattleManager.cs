using System;
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
        [SerializeField] private BattleMap userMap;
        [SerializeField] private BattleMap opponentMap;
        [SerializeField] private OpponentStatusMaskPlacer opponentStatusMaskPlacer;
        [SerializeField] private Rules rules;
        [SerializeField] private PlacementMap placementMap;
        private readonly List<int> _shots = new List<int>();
        private NetworkClient _client;
        private bool _leavePopUpIsOn;
        private NetworkManager _networkManager;
        private int _playerNumber;
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
                userMap.SetShip(placement.ship, placement.Coordinate, default);
            opponentMap.InteractionMode = Disabled;
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
                _state.player1Shots.OnChange -= OnFirstPlayerShotsChanged;
                _state.player2Shots.OnChange -= OnSecondPlayerShotsChanged;
                _state.player1Ships.OnChange -= OnFirstPlayerShipsChanged;
                _state.player2Ships.OnChange -= OnSecondPlayerShipsChanged;
            }
        }

        public event Action FireReady;
        public event Action FireNotReady;

        private void OnFirstRoomStateReceived(State initialState)
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

        public void MarkTarget(Vector3Int targetCoordinate)
        {
            int targetIndex = CoordinateToCellIndex(targetCoordinate, MapAreaSize);
            if (_shots.Contains(targetIndex))
            {
                _shots.Remove(targetIndex);
                opponentMap.ClearMarkerTile(targetCoordinate);
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

        private void OnGamePhaseChanged(string phase)
        {
            switch (phase)
            {
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
                        .Show("Sorry..", "Your opponent has decided not to continue for another round.", "OK",
                            GoBackToLobby);
                    break;
            }

            void GoBackToLobby()
            {
                Debug.Log($"[{name}] Loading scene: <color=yellow>lobbyScene</color>");
                ProjectScenesManager.Instance.GoToLobby();
            }

            void ShowResult()
            {
                userMap.InteractionMode = Disabled;
                opponentMap.InteractionMode = Disabled;
                _networkManager.ClearStatusText();

                bool isWinner = _state.winningPlayer == _playerNumber;
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
            Debug.Log($"[{name}] Loading scene: <color=yellow>lobbyScene</color>");
            ProjectScenesManager.Instance.GoToLobby();
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
                userMap.InteractionMode = Disabled;
                opponentMap.InteractionMode = MarkTargets;
                _networkManager.SetStatusText("It's your turn!");
            }

            void WaitForOpponentTurn()
            {
                userMap.InteractionMode = Disabled;
                opponentMap.InteractionMode = Disabled;
                _networkManager.SetStatusText("Waiting for the opponent to attack.");
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
            if (_playerNumber == playerNumber)
            {
                opponentMap.SetMarker(item, Marker.ShotTarget);
                return;
            }

            userMap.SetMarker(item, !(from placement in placementMap.GetPlacements()
                from part in placement.ship.PartCoordinates
                select placement.Coordinate + (Vector3Int) part
                into partCoordinate
                let shot = CellIndexToCoordinate(item, MapAreaSize.x)
                where partCoordinate.Equals(shot)
                select partCoordinate).Any()
                ? Marker.Missed
                : Marker.Hit);
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

        private PopUpWindow BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpWindow>();
        }
    }
}