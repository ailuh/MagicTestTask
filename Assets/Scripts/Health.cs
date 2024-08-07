using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class Health : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private Slider _healthBar;
    [Range(0, 1)]
    [SerializeField] private float _defense = 0.5f;
    private float _currentHealth;
    private UIManager _uiManager;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public Slider HealthBar => _healthBar;
    
    [Inject]
    public void Construct(UIManager uiManager)
    {
        _uiManager = uiManager;
    }

    private void Awake()
    {
        _currentHealth = _maxHealth;

        if (_healthBar == null)
        {
            Debug.LogError("HealthBar is not assigned in the inspector.");
            return;
        }

        _uiManager.UpdateHealth(_healthBar, _currentHealth, _maxHealth);
    }
    
    public void TakeDamage(float damage)
    {
        var actualDamage = damage * (1 - _defense);
        _currentHealth = Mathf.Clamp(_currentHealth - actualDamage, 0, _maxHealth);

        _uiManager.UpdateHealth(_healthBar, _currentHealth, _maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}