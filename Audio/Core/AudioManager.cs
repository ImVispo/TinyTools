using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace TinyTools.Audio
{
    public static class AudioManager
    {
        // Dictionary of SoundSO linked to each gameObject AudioSource
        private static Dictionary<SoundSO, SoundObject> soundObjects = new Dictionary<SoundSO, SoundObject>();

        // Clear soundObjects on playmodechange & level loaded
        static AudioManager()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeState;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        // Clear soundObjects when changing edit/play mode
        private static void HandlePlayModeState(PlayModeStateChange state) => ClearSoundObjects();
        // Clear soundObjects when loading scenes
        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => ClearSoundObjects();

        // Delete old soundObjects after scripts compile
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() => DeleteOldSoundObjects();

        // Removes a SoundObject from dictionary
        public static void RemoveSoundObject(SoundObject soundObject) => soundObjects.Remove(soundObject.GetSound());

        // Delete all soundObjects and clear dictionary
        public static void ClearSoundObjects()
        {
            // Destroy each soundObject that still exists
            foreach (KeyValuePair<SoundSO, SoundObject> obj in soundObjects)
            {
                if (obj.Value != null)
                {
                    GameObject.DestroyImmediate(obj.Value.gameObject);
                    continue;
                }
            }

            soundObjects.Clear();
        }

        // Delete old soundObjects left in scene after scripts compile
        public static void DeleteOldSoundObjects()
        {
            // Get list of objects
            SoundObject[] oldSoundObjects = GameObject.FindObjectsOfType<SoundObject>();
            if (oldSoundObjects.Length > 0)
            {
                // Delete each object
                foreach (SoundObject soundObject in oldSoundObjects)
                    GameObject.DestroyImmediate(soundObject.gameObject);
            }
        }

        // Play SoundSO without position
        public static SoundObject PlaySoundSO(SoundSO sound) => PlaySoundSO(sound, Vector3.zero);

        // Play SoundSO and hide gameObject
        public static SoundObject PlaySoundSO(SoundSO sound, bool hide) => PlaySoundSO(sound, Vector3.zero, hide);

        // Play SoundSO with position
        public static SoundObject PlaySoundSO(SoundSO sound, Vector3 position, bool hide = false)
        {
            // If dictionary contains soundObject for sound, find it and play
            if (soundObjects.ContainsKey(sound))
            {
                // Play existing SoundObject and set position
                SoundObject soundObject = soundObjects[sound];
                soundObject.transform.position = position;
                soundObject.Play();

                return soundObjects[sound];
            }
            else
            {
                // Create new SoundObject

                SoundObject soundObject = CreateSoundObject(sound.name + "SoundObject", sound);
                soundObject.transform.position = position;

                // If hide game object, set hideflags
                if (hide)
                    soundObject.gameObject.hideFlags = HideFlags.HideInHierarchy;

                soundObject.Play();

                // return new SoundObject
                return soundObject;
            }
        }

        // Stop soundSO
        public static void StopSoundSO(SoundSO sound)
        {
            // If sound object not in dict, return
            if (!soundObjects.ContainsKey(sound))
                return;

            soundObjects[sound].Stop();
        }

        // Creates a soundObject
        public static SoundObject CreateSoundObject(string name, SoundSO sound)
        {
            // Create new gameObject with SoundObject component
            SoundObject soundObject = new GameObject(name).AddComponent<SoundObject>();
            soundObject.SetSound(sound);

            soundObject.gameObject.hideFlags = HideFlags.DontSave;

            // Add new sound object to list
            soundObjects.Add(sound, soundObject);

            return soundObject;
        }
    }
}
