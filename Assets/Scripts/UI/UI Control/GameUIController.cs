using RiichiReign.UnityComponent;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace RiichiReign.UnityUIToolKitComponent
{
    [RequireComponent(typeof(UIDocument))]
    public class GameUIController : MonoBehaviour
    {
        VisualElement _gameContainer;
        VisualElement _networkContainer;
        Button _startHostBtn;
        Button _startClientBtn;
        Button _startGameBtn;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _gameContainer = root.Q<VisualElement>("GameContainer");
            _networkContainer = root.Q<VisualElement>("NetworkContainer");
            _startHostBtn = root.Q<Button>("StartHostBtn");
            _startClientBtn = root.Q<Button>("StartClientBtn");
            _startGameBtn = root.Q<Button>("StartGameBtn");

            _startHostBtn.RegisterCallback<ClickEvent>(OnStartHostBtnClicked);
            _startClientBtn.RegisterCallback<ClickEvent>(OnStartClientBtnClicked);
            _startGameBtn.RegisterCallback<ClickEvent>(OnStartGameBtnClicked);
        }

        private void OnStartHostBtnClicked(ClickEvent evt)
        {
            try
            {
                NetworkManager.Singleton.StartHost();
                Debug.Log($"[{GetType().Name}] Started as host", this);
            }
            catch
            {
                Debug.LogError($"[{GetType().Name}] Error caught when starting as host", this);
            }

            _networkContainer.style.display = DisplayStyle.None;
            _gameContainer.style.display = DisplayStyle.Flex;
        }

        private void OnStartClientBtnClicked(ClickEvent evt)
        {
            try
            {
                NetworkManager.Singleton.StartClient();
                Debug.Log($"[{GetType().Name}] Joined as client", this);
            }
            catch
            {
                Debug.LogError($"[{GetType().Name}] Error caught when joining as client", this);
            }

            gameObject.SetActive(false);
        }

        private void OnStartGameBtnClicked(ClickEvent evt)
        {
            GameManager.Instance.StartGame();
            gameObject.SetActive(false);
        }
    }
}
