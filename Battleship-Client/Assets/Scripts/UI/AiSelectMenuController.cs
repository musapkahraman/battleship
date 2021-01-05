﻿using BattleshipGame.Core;
using BattleshipGame.Managers;
using UnityEngine;
using static BattleshipGame.Core.StatusData.Status;

namespace BattleshipGame.UI
{
    public class AiSelectMenuController : MonoBehaviour
    {
        [SerializeField] private StatusData statusData;
        [SerializeField] private Options options;
        [SerializeField] private MainMenuController mainMenuController;
        [SerializeField] private ButtonController easyButton;
        [SerializeField] private ButtonController hardButton;
        [SerializeField] private ButtonController backButton;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            backButton.AddListener(Close);
            easyButton.AddListener(() =>
            {
                options.aiDifficulty = Difficulty.Easy;
                StartLocalRoom();
            });
            hardButton.AddListener(() =>
            {
                options.aiDifficulty = Difficulty.Hard;
                StartLocalRoom();
            });

            void StartLocalRoom()
            {
                GameManager.Instance.StartLocalClient();
            }
        }

        public void Show()
        {
            _canvas.enabled = true;
            statusData.State = AiSelectionMenu;
        }

        public void Close()
        {
            _canvas.enabled = false;
            mainMenuController.Show();
        }
    }
}