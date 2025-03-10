//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
using Localization;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    public string localizationKey;
    public object[] FormatArguments;

    private TMP_Text _textComponent;

    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
        LocalizationManager.Instance.RegisterText(this);
        RefreshText();
    }

    public void RefreshText()
    {
        try
        {
            _textComponent.text = LocalizationManager.Instance.Get(
                localizationKey, 
                FormatArguments
            );
        }
        catch
        {
            _textComponent.text = $"[ERR]{localizationKey}";
        }
    }

    private void OnDestroy()
    {
        LocalizationManager.Instance.UnregisterText(this);
    }
}