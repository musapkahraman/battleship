using BattleshipGame.UI;
using UnityEditor;
using UnityEngine;

namespace BattleshipGame.Editor
{
    [CustomEditor(typeof(CellLabelPrinter))]
    public class CellLabelPrinterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var indexPrinter = (CellLabelPrinter) target;
            if (GUILayout.Button("Print")) indexPrinter.Print();
            if (GUILayout.Button("Undo")) indexPrinter.Undo();
        }
    }
}