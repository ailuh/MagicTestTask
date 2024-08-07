using UnityEngine;
using VContainer;

public class Spell : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;
    private float _damage;
    private float _speed;
    private float _lifeTime;
    private CombatSystem _combatSystem;

    private Vector2 _direction;
    private ObjectPool<Spell> _pool;

    [Inject]
    public void Construct(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem;
    }
    
    public void SetDirection(Vector2 direction)
    {
        _direction = direction.normalized;
        RotateToDirection();
    }
    
    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    public void SetLifeTime(float lifeTime)
    {
        _lifeTime = lifeTime;
    }

    private void RotateToDirection()
    {
        var angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    public void Initialize(ObjectPool<Spell> pool, CombatSystem combatSystem)
    {
        _combatSystem = combatSystem;
        _pool = pool;
    }
    
   
    public void Activate()
    {
        _rigidbody.velocity = _direction * _speed;
        Invoke(nameof(ReturnToPool), _lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var isMonster = other.TryGetComponent<Monster>(out var monster);
        if (!isMonster) return;
        _combatSystem.ApplyDamageToMonster(monster, (int)_damage);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        CancelInvoke();
        _rigidbody.velocity = Vector2.zero;
        _pool.ReturnToPool(this);
    }
}