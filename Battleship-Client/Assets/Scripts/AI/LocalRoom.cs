using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Schemas;
using Colyseus.Schema;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleshipGame.AI
{
    public class LocalRoom
    {
        private const int StartingFleetHealth = 19;
        private const int GridSize = 9;
        private const int ShotsSize = GridSize * GridSize;
        public readonly State State;
        private Dictionary<string, int> _health;
        private int _placementCompleteCounter;
        private Dictionary<string, int[]> _placements;
        public event Action<State, bool> OnStateChange;

        public LocalRoom(string playerId, string enemyId)
        {
            State = new State {players = new MapSchema<Player>(), phase = "waiting", currentTurn = 1};
            var player = new Player {sessionId = playerId};
            var enemy = new Player {sessionId = enemyId};
            State.players.Add(playerId, player);
            State.players.Add(enemyId, enemy);
            _health = new Dictionary<string, int> {{playerId, StartingFleetHealth}, {enemyId, StartingFleetHealth}};
            _placements = new Dictionary<string, int[]>
            {
                {playerId, new int[ShotsSize]}, {enemyId, new int[ShotsSize]}
            };
            ResetPlayers();
        }

        public void Start()
        {
            OnStateChange?.Invoke(State, true);
            State.phase = "place";
            State.TriggerAll();
        }

        public void Place(string clientId, int[] placement)
        {
            var player = State.players[clientId];
            _placements[player.sessionId] = placement;
            _placementCompleteCounter++;
            if (_placementCompleteCounter == 2)
            {
                State.players.ForEach((s, p) => _health[s] = StartingFleetHealth);
                State.playerTurn = "player"; // GetRandomUser();
                State.phase = "battle";
                
                Debug.Log(" _placements");
                foreach (var kvp in _placements)
                {
                    Debug.Log(kvp.Key);
                    foreach (int i in kvp.Value)
                    {
                        Debug.Log(i);
                    }
                }

                OnStateChange?.Invoke(State, false);
                State.TriggerAll();
            }

            string GetRandomUser()
            {
                var keys = new string[2];
                State.players.Keys.CopyTo(keys, 0);
                return keys[Random.Range(0, 2)];
            }
        }

        public void Turn(string clientId, int[] targetIndexes)
        {
            if (!State.playerTurn.Equals(clientId)) return;
            if (targetIndexes.Length != 3) return;
            var player = State.players[clientId];
            var opponent = GetOpponent(player);
            var playerShots = player.shots;
            var opponentShips = opponent.ships;
            int[] opponentPlacements = _placements[opponent.sessionId];

            foreach (int cell in targetIndexes)
            {
                if (playerShots[cell] != -1) continue;

                playerShots[cell] = State.currentTurn;
                State.players[clientId].shots.InvokeOnChange(State.currentTurn,cell);
                if (opponentPlacements[cell] >= 0)
                {
                    _health[opponent.sessionId]--;
                    switch (opponentPlacements[cell])
                    {
                        case 0: // Admiral
                            UpdateShips(opponentShips, 0, 5, State.currentTurn);
                            break;
                        case 1: // Vertical Cruiser
                            UpdateShips(opponentShips, 5, 8, State.currentTurn);
                            break;
                        case 2: // Horizontal Cruiser
                            UpdateShips(opponentShips, 8, 11, State.currentTurn);
                            break;
                        case 3: // VerticalGunBoat
                            UpdateShips(opponentShips, 11, 13, State.currentTurn);
                            break;
                        case 4: // HorizontalGunBoat
                            UpdateShips(opponentShips, 13, 15, State.currentTurn);
                            break;
                        case 5: // Scout
                        case 6: // Scout
                        case 7: // Scout
                        case 8: // Scout
                            UpdateShips(opponentShips, 15, 19, State.currentTurn);
                            break;
                    }
                }
            }

            if (_health[opponent.sessionId] <= 0)
            {
                State.winningPlayer = player.sessionId;
                State.phase = "result";
            }
            else
            {
                State.playerTurn = opponent.sessionId;
                State.currentTurn++;
            }
            
            OnStateChange?.Invoke(State, false);
            State.TriggerAll();

            void UpdateShips(ArraySchema<int> ships, int start, int end, int turn)
            {
                for (int i = start; i < end; i++)
                    if (ships[i] == -1)
                    {
                        ships[i] = turn;
                        break;
                    }
            }
        }

        private Player GetOpponent(Player player)
        {
            var opponent = new Player();
            State.players.ForEach((id, p) =>
            {
                if (p != player) opponent = p;
            });
            return opponent;
        }

        public void Rematch(bool isRematching)
        {
            if (!isRematching)
            {
                State.phase = "leave";
                return;
            }

            ResetPlayers();
            _placementCompleteCounter = 0;
            State.playerTurn = "";
            State.winningPlayer = "";
            State.currentTurn = 1;
            State.phase = "place";
        }

        private void ResetPlayers()
        {
            State.players.ForEach((id, p) =>
            {
                var shots = new Dictionary<int, int>();
                for (var i = 0; i < ShotsSize; i++) shots.Add(i, -1);
                p.shots = new ArraySchema<int>(shots);

                var ships = new Dictionary<int, int>();
                for (var i = 0; i < StartingFleetHealth; i++) ships.Add(i, -1);
                p.shots = new ArraySchema<int>(ships);
            });

            _health = _health.ToDictionary(kvp => kvp.Key, kvp => StartingFleetHealth);
            _placements = _placements.ToDictionary(kvp => kvp.Key, kvp => new int[ShotsSize]);
        }
    }
}