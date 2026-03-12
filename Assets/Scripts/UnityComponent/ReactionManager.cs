using System.Collections;
using RiichiReign.Player;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    internal class ReactionManager : MonoBehaviour
    {
        public static ReactionManager Instance { get; private set; }

        [SerializeField]
        private float _reactionTimeOut = 20f;

        #region Unity Logics

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Public Methods

        public IEnumerator WaitForPlayerInputCoroutine(
            PlayerInstance player,
            System.Action<PlayerAction> callback
        )
        {
            PlayerAction response = null;
            float passedTime = 0f;

            while (passedTime < _reactionTimeOut || response == null)
            {
                passedTime += Time.deltaTime;
                yield return null;
            }

            callback?.Invoke(response);
        }

        #endregion
    }
}
