using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConditionIconUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI durationText;
    
    public void Setup(Sprite icon, int duration)
    {
        iconImage.sprite = icon;
        UpdateDuration(duration);
    }

    public void UpdateDuration(int duration)
    {
        durationText.text = duration.ToString();
    }
}
