using UnityEditor;
using UnityEngine;

namespace BattleshipGame.Readme
{
    [CustomEditor(typeof(Readme))]
    [InitializeOnLoad]
    public class ReadmeEditor : UnityEditor.Editor
    {
        private const string ShowedReadmeSessionStateName = "ReadmeEditor.showedReadme";
        private const float Space = 16f;
        [SerializeField] private GUIStyle linkStyle;
        [SerializeField] private GUIStyle titleStyle;
        [SerializeField] private GUIStyle headingStyle;
        [SerializeField] private GUIStyle bodyStyle;

        private bool _isInitialized;

        static ReadmeEditor()
        {
            EditorApplication.delayCall += SelectReadmeAutomatically;
        }

        private GUIStyle LinkStyle => linkStyle;

        private GUIStyle TitleStyle => titleStyle;

        private GUIStyle HeadingStyle => headingStyle;

        private GUIStyle BodyStyle => bodyStyle;

        private static void SelectReadmeAutomatically()
        {
            if (SessionState.GetBool(ShowedReadmeSessionStateName, false)) return;
            SelectReadme();
            SessionState.SetBool(ShowedReadmeSessionStateName, true);
        }

        [MenuItem("Help/Readme")]
        private static void SelectReadme()
        {
            var ids = AssetDatabase.FindAssets("Readme t:Readme");
            if (ids.Length != 1) return;
            var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
            Selection.objects = new[] {readmeObject};
        }

        protected override void OnHeaderGUI()
        {
            var readme = (Readme) target;
            Init();
            GUILayout.BeginHorizontal("In BigTitle");
            {
                GUILayout.Label(readme.title, TitleStyle);
            }
            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            var readme = (Readme) target;
            Init();
            foreach (var section in readme.sections)
            {
                if (!string.IsNullOrEmpty(section.heading)) GUILayout.Label(section.heading, HeadingStyle);
                if (!string.IsNullOrEmpty(section.text)) GUILayout.Label(section.text, BodyStyle);
                if (!string.IsNullOrEmpty(section.linkText))
                {
                    GUILayout.Space(Space / 2);
                    if (LinkLabel(new GUIContent(section.linkText))) Application.OpenURL(section.url);
                }

                GUILayout.Space(Space);
            }
        }

        private void Init()
        {
            if (_isInitialized) return;
            bodyStyle = new GUIStyle(EditorStyles.label) {wordWrap = true, fontSize = 14};
            titleStyle = new GUIStyle(bodyStyle) {fontSize = 26};
            headingStyle = new GUIStyle(bodyStyle) {fontSize = 18, fontStyle = FontStyle.Bold};
            linkStyle = new GUIStyle(bodyStyle)
            {
                normal = {textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f)}, stretchWidth = false
            };
            // Match selection color which works nicely for both light and dark skins
            _isInitialized = true;
        }

        private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, LinkStyle, options);
            Handles.BeginGUI();
            Handles.color = LinkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, label, LinkStyle);
        }
    }
}