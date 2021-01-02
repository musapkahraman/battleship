using System;
using System.Collections;
using BattleshipGame.Core;
using BattleshipGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BattleshipGame.Core.GameStateContainer.GameState;

namespace BattleshipGame.Managers
{
    public class MenuManager : MonoBehaviour
    {
#pragma warning disable CS0414
        [SerializeField] private string webQuitPage = "about:blank";
#pragma warning restore CS0414
        [SerializeField] private OptionDialog singlePlayerOptionDialog;
        [SerializeField] private ButtonController quitButton;
        [SerializeField] private ButtonController singlePlayerButton;
        [SerializeField] private ButtonController multiplayerButton;
        [SerializeField] private ButtonController cancelButton;
        [SerializeField] private ButtonController optionsButton;
        [SerializeField] private Canvas optionsCanvas;
        [SerializeField] private Options options;
        [SerializeField] private GameObject progressBarCanvasPrefab;
        [SerializeField] private GameStateContainer gameStateContainer;
        private bool _isConnecting;
        private bool _isConnectionCanceled;
        private GameObject _progressBar;
        private GameManager _gameManager;

        private void Awake()
        {
            if (!GameManager.TryGetInstance(out _gameManager)) SceneManager.LoadScene(0);
        }

        private void Start()
        {
            quitButton.AddListener(Quit);
            singlePlayerButton.AddListener(PlayAgainstAI);
            multiplayerButton.AddListener(PlayWithFriends);
            cancelButton.AddListener(CancelConnection);
            cancelButton.Hide();
            optionsButton.AddListener(() => { optionsCanvas.enabled = true; });

            void PlayAgainstAI()
            {
                singlePlayerOptionDialog.Show(() =>
                {
                    options.aiDifficulty = Difficulty.Easy;
                    StartLocalRoom();
                }, () =>
                {
                    options.aiDifficulty = Difficulty.Hard;
                    StartLocalRoom();
                });

                void StartLocalRoom()
                {
                    _gameManager.StartLocalClient();
                }
            }

            void PlayWithFriends()
            {
                if (!_isConnecting)
                {
                    _isConnecting = true;
                    singlePlayerButton.SetInteractable(false);
                    optionsButton.SetInteractable(false);
                    quitButton.SetInteractable(false);
                    multiplayerButton.Hide();
                    cancelButton.Show();
                    if (progressBarCanvasPrefab) _progressBar = Instantiate(progressBarCanvasPrefab);
                    _gameManager.ConnectToServer(() =>
                    {
                        _isConnecting = false;
                        if (_isConnectionCanceled)
                            StartCoroutine(FinishNetworkClient());
                        else
                            GameSceneManager.Instance.GoToLobby();
                    }, () =>
                    {
                        _isConnecting = false;
                        _isConnectionCanceled = false;
                        gameStateContainer.State = NetworkError;
                        ResetMenu();
                    });
                }

                IEnumerator FinishNetworkClient()
                {
                    yield return new WaitForSecondsRealtime(1);
                    _gameManager.FinishNetworkClient();
                    _isConnectionCanceled = false;
                    multiplayerButton.SetInteractable(true);
                }
            }
            
            void CancelConnection()
            {
                _isConnectionCanceled = true;
                ResetMenu();
                multiplayerButton.SetInteractable(false);
            }
            
            void ResetMenu()
            {
                singlePlayerButton.SetInteractable(true);
                optionsButton.SetInteractable(true);
                quitButton.SetInteractable(true);
                multiplayerButton.Show();
                cancelButton.Hide();
                Destroy(_progressBar);
                gameStateContainer.State = MainMenu;
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape)) OnNavigateBack?.Invoke();
        }

        public event Action OnNavigateBack;

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
                Application.OpenURL(webQuitPage);
#else
                Application.Quit();
#endif
        }
    }
}