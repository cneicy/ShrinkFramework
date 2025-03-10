//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using Event;
using UnityEngine;

namespace Localization
{
    [System.Serializable]
    public class LocalizationConfig
    {
        public bool autoUpdateTexts = true;
        public string missingKeyPrefix = "[MISSING]";
        public char placeholderStart = '{';
        public char placeholderEnd = '}';
    }
    public enum Language
    {
        EnUs,
        ZhCn,
        ZhTw,
        JaJp,
        KoKr,
    }

    public class LocalizationManager : MonoBehaviour
    {
        public LocalizationConfig config = new();
        
        private Language _currentLanguage;
        private readonly Dictionary<string, string> _localizedText = new();
        private readonly List<LocalizedText> _textComponents = new();

        #region Singleton
        public static LocalizationManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeLanguage();
            RegisterEvents();
        }
        #endregion

        #region 初始化
        private void InitializeLanguage()
        {
            var targetLang = config.autoUpdateTexts ? 
                DetectSystemLanguage() : 
                _currentLanguage;
            
            LoadLanguage(targetLang);
        }

        private Language DetectSystemLanguage()
        {
            // 更精确的系统语言检测逻辑
            return Application.systemLanguage switch
            {
                SystemLanguage.ChineseSimplified => Language.ZhCn,
                SystemLanguage.Chinese => Language.ZhCn,
                SystemLanguage.ChineseTraditional => Language.ZhTw,
                SystemLanguage.Japanese => Language.JaJp,
                SystemLanguage.Korean => Language.KoKr,
                _ => Language.EnUs
            };
        }
        #endregion

        #region 核心功能
        public void LoadLanguage(Language language)
        {
            StartCoroutine(LoadLanguageAsync(language));
        }

        private IEnumerator LoadLanguageAsync(Language language)
        {
            var request = Resources.LoadAsync<TextAsset>($"Localization/{language}");
            yield return request;

            if (request.asset is not TextAsset textAsset)
            {
                Debug.LogError($"{language} 加载失败");
                yield break;
            }

            ParseLocalizationData(textAsset.text);
            _currentLanguage = language;
            
            // 触发双重事件机制
            OnLanguageChanged?.Invoke();
            EventManager.Instance.TriggerEvent("OnLanguageChanged", language);
        }

        private void ParseLocalizationData(string data)
        {
            _localizedText.Clear();
            
            foreach (var line in data.SplitLines())
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if (line.StartsWith("#")) continue;

                var (key, value) = ParseLine(line);
                if (!string.IsNullOrEmpty(key))
                {
                    _localizedText[key] = ProcessPlaceholders(value);
                }
            }
        }

        private (string key, string value) ParseLine(string line)
        {
            var index = line.IndexOf('=');
            if (index <= 0) return (null, null);

            return (
                line[..index].Trim(),
                line[(index+1)..].Trim().UnescapeNewlines()
            );
        }
        #endregion

        #region 占位符系统
        private string ProcessPlaceholders(string rawValue)
        {
            return rawValue.Replace("{{", "{").Replace("}}", "}");
        }

        public string Get(string key, params object[] args)
        {
            if (!_localizedText.TryGetValue(key, out var value))
            {
                Debug.LogWarning($"丢失的本地化键: {key}");
                return $"{config.missingKeyPrefix}{key}";
            }

            try
            {
                return string.Format(value, args);
            }
            catch (FormatException e)
            {
                Debug.LogError($"格式错误: {key}\n{value}\n{e.Message}");
                return value;
            }
        }
        #endregion

        #region 事件系统集成
        public event Action OnLanguageChanged;

        private void RegisterEvents()
        {
            EventManager.Instance.RegisterEvent<Language>(
                "RequestLanguageChange", 
                HandleLanguageChange
            );
        }

        private object HandleLanguageChange(Language newLanguage)
        {
            if (newLanguage != _currentLanguage)
            {
                LoadLanguage(newLanguage);
            }
            return true;
        }
        #endregion

        #region 文本组件管理
        public void RegisterText(LocalizedText textComponent)
        {
            _textComponents.Add(textComponent);
        }

        public void UnregisterText(LocalizedText textComponent)
        {
            _textComponents.Remove(textComponent);
        }

        [EventSubscribe("OnLanguageChanged")]
        public void UpdateAllTexts(Language _)
        {
            foreach (var text in _textComponents)
            {
                text.RefreshText();
            }
        }
        #endregion

        #region 调试工具
        public Dictionary<string, string> GetRawData() => new(_localizedText);
        
        public void ReloadCurrentLanguage()
        {
            LoadLanguage(_currentLanguage);
        }
        #endregion
    }

    // 扩展方法
    public static class LocalizationExtensions
    {
        public static IEnumerable<string> SplitLines(this string input)
        {
            return input.Split(new[] { "\r\n", "\r", "\n" }, 
                StringSplitOptions.RemoveEmptyEntries);
        }

        public static string UnescapeNewlines(this string input)
        {
            return input.Replace("\\n", "\n");
        }
    }
}