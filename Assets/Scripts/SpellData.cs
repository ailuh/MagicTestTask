using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class SpellData : ScriptableObject
{
    [SerializeField] private string _spellName;
    [SerializeField] private Sprite _spellIcon;
    [SerializeField] private GameObject _spellPrefab;
    [SerializeField] private float _spellSpeed;
    [SerializeField] private int _damage;
    [SerializeField] private float _lifeTime;

    public string SpellName => _spellName;
    public Sprite SpellIcon => _spellIcon;
    public GameObject SpellPrefab => _spellPrefab;
    public float SpellSpeed => _spellSpeed;
    public int Damage => _damage;
    public float LifeTime => _lifeTime;
}