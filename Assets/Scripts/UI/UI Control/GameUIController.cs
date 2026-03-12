using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace RiichiReign.UnityUIToolKitComponent
{
    [RequireComponent(typeof(UIDocument))]
    public class GameUIController : MonoBehaviour
    {
        Button _startHostBtn;
        Button _startClientBtn;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _startHostBtn = root.Q<Button>("StartHostBtn");
            _startClientBtn = root.Q<Button>("StartClientBtn");

            _startHostBtn.RegisterCallback<ClickEvent>(OnStartHostBtnClicked);
            _startClientBtn.RegisterCallback<ClickEvent>(OnStartClientBtnClicked);
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

            gameObject.SetActive(false);
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
    }
}
