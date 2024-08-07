using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image _spellIcon;
    public void UpdateSpellIcon(Sprite newIcon)
    {
        _spellIcon.sprite = newIcon;
    }

    public void UpdateHealth(Slider healthBar, float currentHealth, float maxHealth)
    {
        healthBar.value = currentHealth / maxHealth;
    }
}