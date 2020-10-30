using UnityEngine;
using System;
using System.Collections;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute
{
    //The name of the bool field that will be in control
    public string ConditionalSourceField = "";
    //TRUE = Hide in inspector / FALSE = Disable in inspector 
    public bool HideInInspector = false;
    //TRUE = Hide if true / FALSE = Hide if false (default behavior)
    public bool ShouldReverseDisplay = false;

    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector) {
        if (!conditionalSourceField.StartsWith("!")) {
            this.ConditionalSourceField = conditionalSourceField;
            ShouldReverseDisplay = false;
        } else {
            this.ConditionalSourceField = conditionalSourceField.Substring(1);
            ShouldReverseDisplay = true;
        }
        this.HideInInspector = true;
    }

    public ConditionalHideAttribute(string conditionalSourceField) : this(conditionalSourceField, true) {
    }
}