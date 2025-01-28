using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderY : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void ChangementValeur(float val)
    { 
        text.text=val.ToString();
    
    }
}
