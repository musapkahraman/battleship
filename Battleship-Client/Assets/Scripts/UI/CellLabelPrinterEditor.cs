#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BattleshipGame.UI
{
    [CustomEditor(typeof(CellLabelPrinter))]
    public class CellLabelPrinterEditor : Editor
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
#endif