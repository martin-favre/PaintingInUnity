using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HSVPicker;

public class ColorPickerSingle : MonoBehaviour
{
    static ColorPickerSingle instance;

    ColorPicker picker;

    public static ColorPicker Instance { get => instance.picker; }

    void Awake()
    {
        instance = this;
        picker = GetComponent<ColorPicker>();
    }
}
