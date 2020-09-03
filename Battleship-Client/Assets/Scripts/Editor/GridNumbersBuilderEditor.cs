using BattleshipGame.UI;
using UnityEditor;
using UnityEngine;

namespace BattleshipGame.Editor
{
    [CustomEditor(typeof(GridNumbersBuilder))]
    public class GridNumbersBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var indexPrinter = (GridNumbersBuilder) target;
            if (GUILayout.Button("Print")) indexPrinter.Print();
            if (GUILayout.Button("Undo")) indexPrinter.Undo();
        }
    }
}