using BattleshipGame.Common;
using UnityEngine;

namespace BattleshipGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Scene Data", menuName = "Battleship/Scene Data", order = 2)]
    public class SceneData : ScriptableObject
    {
        [Header("Information")] public SceneReference scene;
        public string shortDescription;

        [Header("Sounds")] public AudioClip music;
        [Range(0.0f, 1.0f)] public float musicVolume;
    }
}