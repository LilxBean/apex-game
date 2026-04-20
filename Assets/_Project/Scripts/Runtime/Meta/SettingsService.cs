using System;
using UnityEngine;

namespace APEX.Meta
{
    /// <summary>
    /// Persistent singleton that holds the active SettingsStore and applies it to Unity
    /// subsystems. Lives on a DontDestroyOnLoad GameObject created by MainMenuBootstrap.
    /// </summary>
    public class SettingsService : MonoBehaviour
    {
        public static SettingsService Instance { get; private set; }

        public SettingsStore Store { get; private set; }

        public event Action OnSettingsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Store = SettingsStore.Load();
            Apply();
        }

        /// <summary>Push the current Store values onto Unity subsystems. Raises OnSettingsChanged.</summary>
        public void Apply()
        {
            Screen.fullScreen = Store.fullscreen;
            QualitySettings.vSyncCount = Store.vsync ? 1 : 0;
            // masterVolume: stored only. Audio system is a later session.

            OnSettingsChanged?.Invoke();
        }

        public void Save()
        {
            SettingsStore.Save(Store);
        }
    }
}
