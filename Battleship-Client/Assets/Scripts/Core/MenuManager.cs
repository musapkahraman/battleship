using BattleshipGame.AI;
using BattleshipGame.Localization;
using BattleshipGame.UI;
using UnityEditor;
using UnityEngine;

namespace BattleshipGame.Core
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private Key popupSingleHeader;
        [SerializeField] private Key popupSingleMessage;
        [SerializeField] private Key popupSingleConfirm;
        [SerializeField] private Key popupSingleDecline;
        [SerializeField] private ButtonController quitButton;
        [SerializeField] private ButtonController singlePlayerButton;
        [SerializeField] private ButtonController multiplayerButton;
        [SerializeField] private ButtonController optionsButton;
        [SerializeField] private Canvas optionsCanvas;
        [SerializeField] private Options options;
        [SerializeField] private GameObject popUpPrefab;

        private GameManager _gameManager;

        private void Start()
        {
            quitButton.AddListener(Quit);
            singlePlayerButton.AddListener(PlayAgainstAI);
            multiplayerButton.AddListener(PlayWithFriends);
            optionsButton.AddListener(() => { optionsCanvas.enabled = true; });

            void Quit()
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
                Application.OpenURL(webQuitPage);
#else
                Application.Quit();
#endif
            }

            void PlayAgainstAI()
            {
                BuildPopUp().Show(popupSingleHeader, popupSingleMessage, popupSingleConfirm, popupSingleDecline,
                    OnEasyMode, OnHardMode);

                void OnEasyMode()
                {
                    options.aiDifficulty = Difficulty.Easy;
                    StartLocalRoom();
                }

                void OnHardMode()
                {
                    options.aiDifficulty = Difficulty.Hard;
                    StartLocalRoom();
                }

                void StartLocalRoom()
                {
                    if (!GameManager.TryGetInstance(out _gameManager)) return;
                    _gameManager.StartLocalClient();
                }
            }

            void PlayWithFriends()
            {
                if (!GameManager.TryGetInstance(out _gameManager)) return;
                _gameManager.ConnectToServer(() => multiplayerButton.SetInteractable(true));
                multiplayerButton.SetInteractable(false);
            }
        }

        private PopUpWindow BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpWindow>();
        }
    }
}