// Copyright 2025 Spellbound Studio Inc.

using Spellbound.Core.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Spellbound.Controller.Samples {
    public class SpawnAndTestExample : MonoBehaviour {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Button spawnPlayerBtn;
        [SerializeField] private Transform spawnPoint;

        private void Awake() {
            if (playerPrefab == null)
                Log.Error("Player prefab is null");

            if (spawnPlayerBtn == null)
                spawnPlayerBtn = GetComponent<Button>();
        }

        private void OnEnable() => spawnPlayerBtn.onClick.AddListener(SpawnPlayer);

        private void OnDisable() => spawnPlayerBtn.onClick.RemoveListener(SpawnPlayer);

        private void SpawnPlayer() =>
                playerPrefab = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}