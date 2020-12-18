using System.Collections.Generic;
using BattleshipGame.Schemas;
using Colyseus.Schema;
using UnityEngine;

namespace BattleshipGame.AI
{
    public class LocalRoom
    {
        private const int StartingFleetHealth = 19;
        private const int GridSize = 9;
        public readonly State State;
        private Dictionary<string, int> _health;
        private int _placementCompleteCounter;
        private Dictionary<string, int[]> _placements;

        public LocalRoom(string playerId, string enemyId)
        {
            State = new State {players = new MapSchema<Player>(), phase = "waiting", currentTurn = 1};
            var player = new Player {sessionId = playerId};
            var enemy = new Player {sessionId = enemyId};
            State.players.Add(playerId, player);
            State.players.Add(enemyId, enemy);
            ResetPlayers();
            State.phase = "place";
        }

        public void Place(string clientId, int[] placement)
        {
            var player = State.players[clientId];
            _placements[player.sessionId] = placement;
            _placementCompleteCounter++;
            if (_placementCompleteCounter == 2)
            {
                State.players.ForEach((s, p) => _health[s] = StartingFleetHealth);
                State.playerTurn = GetRandomUser();
                State.phase = "battle";
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
            _health = new Dictionary<string, int>();
            _placements = new Dictionary<string, int[]>();
            _placementCompleteCounter = 0;
            State.playerTurn = "";
            State.winningPlayer = "";
            State.currentTurn = 1;
            State.phase = "place";
        }

        private void ResetPlayers()
        {
            const int shotsSize = GridSize * GridSize;
            State.players.ForEach((clientId, player) =>
            {
                var shots = new Dictionary<int, int>();
                for (var i = 0; i < shotsSize; i++) shots.Add(i, -1);
                player.shots = new ArraySchema<int>(shots);

                var ships = new Dictionary<int, int>();
                for (var i = 0; i < StartingFleetHealth; i++) ships.Add(i, -1);
                player.shots = new ArraySchema<int>(ships);
            });
        }
    }
}