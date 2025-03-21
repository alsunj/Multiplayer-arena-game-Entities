using TMPro;
using Unity.Entities;
using UnityEngine;

namespace TMG.NFE_Tutorial
{
    public class RespawnUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _respawnPanel;
        [SerializeField] private TextMeshProUGUI _respawnCountdownText;

        private void OnEnable() // Use OnEnable for subscribing
        {
            _respawnPanel.SetActive(false);

            // Subscribe directly to the static events:
            RespawnPlayerSystem.OnUpdateRespawnCountdown += UpdateRespawnCountdownText;
            RespawnPlayerSystem.OnRespawn += CloseRespawnPanel;
        }

        private void OnDisable()
        {
            // Unsubscribe directly from the static events (IMPORTANT):
            RespawnPlayerSystem.OnUpdateRespawnCountdown -= UpdateRespawnCountdownText;
            RespawnPlayerSystem.OnRespawn -= CloseRespawnPanel;
        }

        private void UpdateRespawnCountdownText(int countdownTime)
        {
            if (!_respawnPanel.activeSelf) _respawnPanel.SetActive(true);

            _respawnCountdownText.text = countdownTime.ToString();
        }

        private void CloseRespawnPanel()
        {
            _respawnPanel.SetActive(false);
        }
    }
}