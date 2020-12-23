﻿using BattleshipGame.AI;
using UnityEngine;

namespace BattleshipGame.Core
{
    [CreateAssetMenu(fileName = "Options", menuName = "Battleship/Options")]
    public class Options : ScriptableObject
    {
        public Difficulty aiDifficulty = Difficulty.Easy;
    }
}