using System;
using RiichiReign.UnityComponent;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace RiichiReign.UnityUIToolKitComponent
{
    struct PlayerContainerElement
    {
        public VisualElement HandContainer { get; private set; }

        public VisualElement TempTileContainer { get; private set; }

        public PlayerContainerElement(VisualElement handContainer, VisualElement tempTileContainer)
        {
            HandContainer = handContainer;
            TempTileContainer = tempTileContainer;
            Clear();
        }

        public readonly void Clear()
        {
            HandContainer.Clear();
            TempTileContainer.Clear();
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public class PlayerUIController : NetworkBehaviour
    {
        ulong NetworkID => this.NetworkObjectId;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                gameObject.SetActive(false);
                return;
            }

            Invoke(nameof(Register), 1f);
        }

        private void Register()
        {
            PlayerManager.Instance.RegisterPlayer(NetworkID, this);
        }
    }
}
