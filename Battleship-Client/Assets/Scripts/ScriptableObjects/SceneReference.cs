using UnityEngine;

namespace BattleshipGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Scene Reference", menuName = "Battleship/Scene Reference", order = 2)]
    public class SceneReference : ScriptableObject
    {
        public string sceneName;
    }
}