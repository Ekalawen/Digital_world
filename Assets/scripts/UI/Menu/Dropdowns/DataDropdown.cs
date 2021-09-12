using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class DataDropdown : MonoBehaviour {

    public Dropdown dropdown;
    public LocalizedString highString;
    public LocalizedString lowString;

    public void Start() {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChange;
        SetDropdownData();

        dropdown.value = PrefsManager.GetInt(PrefsManager.DATA_QUALITY_KEY, (int)MenuOptions.defaultLumiereQuality);
        dropdown.onValueChanged.AddListener(DataQualitySelected);
    }

    protected void SetDropdownData() {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(highString.GetLocalizedString().Result));
        options.Add(new Dropdown.OptionData(lowString.GetLocalizedString().Result));
        dropdown.options = options;
    }

    public void DataQualitySelected(int qualityIndex) {
        PrefsManager.SetInt(PrefsManager.DATA_QUALITY_KEY, qualityIndex);
        GameManager gm = FindObjectOfType<GameManager>();
        if(gm != null) {
            gm.map.SetDataQuality((Lumiere.LumiereQuality)qualityIndex);
        }
    }

    public void OnLocaleChange(Locale l) {
        SetDropdownData();
    }
}