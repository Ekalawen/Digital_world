using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button), typeof(EventTrigger))]
public class ButtonSounds : MonoBehaviour {
    public void Start() {
        Button button = GetComponent<Button>();

        button.onClick.AddListener(new UnityAction(UISoundManager.Instance.PlayClickedButtonClip));

        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { PlayHoverSound(data); });
        trigger.triggers.Add(entry);
    }

    public void PlayHoverSound(BaseEventData eventData) {
        UISoundManager.Instance.PlayHoverButtonClip();
    }
}
