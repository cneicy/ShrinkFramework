//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Singleton;
using UnityEngine;
using Event;

namespace Input.KeyboardInput
{
    public class KeySettingManager : Singleton<KeySettingManager>
    {
        #region 事件系统
        public static class KeyEvents
        {
            public const string KeyAdded = "KeyAdded";
            public const string KeyRemoved = "KeyRemoved";
            public const string KeyChanged = "KeyChanged";
            public const string KeyConflict = "KeyConflict";
            public const string SettingsLoaded = "KeySettingsLoaded";
            public const string SettingsSaved = "KeySettingsSaved";
            public const string SettingsReset = "KeySettingsReset";
        }

        public class KeyEventArgs
        {
            public string ActionName { get; set; }
            public KeyCode OldKey { get; set; }
            public KeyCode NewKey { get; set; }
            public List<KeyMapping> AllMappings { get; set; }
        }
        #endregion

        #region 核心实现
        private Dictionary<string, KeyMapping> _keyMappingDict = new();
        private string _filePath;
        
        private readonly KeyMapping[] _defaultMappings = 
        {
            new("Attack", KeyCode.J),
            new("Left", KeyCode.A),
            new("Right", KeyCode.D),
            new("Up", KeyCode.W),
            new("Down", KeyCode.S)
        };

        public Vector2 Direction { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            InitializeFilePath();
            LoadKeySettings();
        }

        private void InitializeFilePath()
        {
            _filePath = Path.Combine(
                Application.persistentDataPath, 
                "KeySettings.json"
            );
        }
        #endregion

        #region 公开接口
        public void AddKeyMapping(string actionName, KeyCode defaultKey, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(actionName)) return;

            if (_keyMappingDict.TryGetValue(actionName, out var existing))
            {
                if (overwrite) UpdateKeyInternal(actionName, defaultKey);
                return;
            }

            var newMapping = new KeyMapping(actionName, defaultKey);
            _keyMappingDict.Add(actionName, newMapping);
            SaveKeySettings();
            TriggerKeyEvent(KeyEvents.KeyAdded, actionName, KeyCode.None, defaultKey);
        }

        public bool RemoveKeyMapping(string actionName)
        {
            if (!_keyMappingDict.TryGetValue(actionName, out var mapping)) return false;

            var removedKey = mapping.keyCode;
            _keyMappingDict.Remove(actionName);
            SaveKeySettings();
            TriggerKeyEvent(KeyEvents.KeyRemoved, actionName, removedKey, KeyCode.None);
            return true;
        }

        public KeyCode? GetKey(string actionName)
        {
            return _keyMappingDict.TryGetValue(actionName, out var mapping) 
                ? mapping.keyCode 
                : (KeyCode?)null;
        }

        public bool SetKey(string actionName, KeyCode newKeyCode)
        {
            if (!_keyMappingDict.ContainsKey(actionName)) return false;
            if (IsKeyOccupied(newKeyCode, out var conflictAction))
            {
                TriggerConflictEvent(actionName, conflictAction, newKeyCode);
                return false;
            }

            return UpdateKeyInternal(actionName, newKeyCode);
        }

        public void ResetToDefaults()
        {
            _keyMappingDict.Clear();
            EnsureDefaultMappings();
            SaveKeySettings();
            TriggerSettingsEvent(KeyEvents.SettingsReset);
        }

        public List<KeyMapping> GetAllMappings()
        {
            return _keyMappingDict.Values.ToList();
        }
        #endregion

        #region 内部实现
        private bool UpdateKeyInternal(string actionName, KeyCode newKeyCode)
        {
            var oldKey = _keyMappingDict[actionName].keyCode;
            _keyMappingDict[actionName].keyCode = newKeyCode;
            SaveKeySettings();
            TriggerKeyEvent(KeyEvents.KeyChanged, actionName, oldKey, newKeyCode);
            return true;
        }

        private bool IsKeyOccupied(KeyCode key, out string conflictAction)
        {
            foreach (var pair in _keyMappingDict)
            {
                if (pair.Value.keyCode == key)
                {
                    conflictAction = pair.Key;
                    return true;
                }
            }
            conflictAction = null;
            return false;
        }

        private void LoadKeySettings()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var loadedMappings = JsonConvert.DeserializeObject<List<KeyMapping>>(json);
                    _keyMappingDict = loadedMappings.ToDictionary(m => m.actionName);
                }
                EnsureDefaultMappings();
                TriggerSettingsEvent(KeyEvents.SettingsLoaded);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"键位配置加载失败: {e}");
                EventManager.Instance.TriggerEvent("KeySettingError", e.Message);
            }
        }

        private void SaveKeySettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_keyMappingDict.Values, Formatting.Indented);
                File.WriteAllText(_filePath, json);
                TriggerSettingsEvent(KeyEvents.SettingsSaved);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"键位配置保存失败: {e}");
                EventManager.Instance.TriggerEvent("KeySettingError", e.Message);
            }
        }

        private void EnsureDefaultMappings()
        {
            foreach (var defaultMapping in _defaultMappings)
            {
                _keyMappingDict.TryAdd(defaultMapping.actionName, defaultMapping);
            }
        }
        #endregion

        #region 事件触发
        private void TriggerKeyEvent(string eventType, string actionName, KeyCode oldKey, KeyCode newKey)
        {
            EventManager.Instance.TriggerEvent(eventType, new KeyEventArgs
            {
                ActionName = actionName,
                OldKey = oldKey,
                NewKey = newKey,
                AllMappings = GetAllMappings()
            });
        }

        private void TriggerConflictEvent(string attemptedAction, string conflictAction, KeyCode keyCode)
        {
            EventManager.Instance.TriggerEvent(KeyEvents.KeyConflict, new
            {
                AttemptedAction = attemptedAction,
                ConflictingAction = conflictAction,
                KeyCode = keyCode,
                Time = Time.time
            });
        }

        private void TriggerSettingsEvent(string eventType)
        {
            EventManager.Instance.TriggerEvent(eventType, new
            {
                Mappings = GetAllMappings(),
                Timestamp = System.DateTime.Now
            });
        }
        #endregion

        #region 输入更新
        private void Update()
        {
            UpdateMovementInput();
        }

        private void UpdateMovementInput()
        {
            float horizontal = GetAxis("Left", "Right");
            float vertical = GetAxis("Down", "Up");
            Direction = new Vector2(horizontal, vertical);
        }

        private float GetAxis(string negativeKey, string positiveKey)
        {
            float value = 0;
            if (GetKeyState(positiveKey)) value += 1;
            if (GetKeyState(negativeKey)) value -= 1;
            return value;
        }

        private bool GetKeyState(string actionName)
        {
            return _keyMappingDict.TryGetValue(actionName, out var mapping) 
                && UnityEngine.Input.GetKey(mapping.keyCode);
        }
        #endregion
    }

    [System.Serializable]
    public class KeyMapping
    {
        public string actionName;
        public KeyCode keyCode;

        public KeyMapping(string actionName, KeyCode keyCode)
        {
            this.actionName = actionName;
            this.keyCode = keyCode;
        }
    }
}