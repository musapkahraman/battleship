using System.Collections.Generic;
using UnityEngine;

namespace BattleshipGame.UI
{
    public class OpponentStatusMaskPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject maskPrefab;
        [SerializeField] private List<Ship> ships;
        

        private void PlaceMask()
        {
            
            // Instantiate(maskPrefab, position, Quaternion.identity, transform);
        }
        
    }
}