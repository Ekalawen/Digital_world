using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;
using TMPro;

public class DisplayInitMessageWithBinding : MonoBehaviour {

    public TimedLocalizedMessage message;
    public MessageZoneBindingParameters.Bindings binding;

    public void DisplayInitMessage(Console console) {
        LocalizedString ls = message.localizedString;
        string bindingParameter = InputManager.Instance.GetCurrentInputController().GetStringForBinding(binding);
        ls.Arguments = new object[] { bindingParameter };
        console.AjouterMessageImportant(ls, message.type, message.duree);
    }
}
