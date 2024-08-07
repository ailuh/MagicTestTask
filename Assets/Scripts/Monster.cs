using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

public class Monster : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private Health _healthComponent;
    [SerializeField] private Vector3 _healthBarOffset = new(0, 1.5f, 0);
    [SerializeField] private float _speed = 2f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackRadius = 1f;

    private Transform _playerTransform;
    private MonsterManager _monsterManager;
    private PlayerController _playerController;
    private bool _isAttacking;
    private Vector2 _movement;
    private UIManager _uiManager;

    [Inject]
    public void Construct(UIManager uiManager)
    {
        _uiManager = uiManager;
    }

    private void Start()
    {
        _healthComponent = GetComponent<Health>();
        if (_healthComponent == null)
        {
            Debug.LogError("Monster requires a Health component.");
            return;
        }

        _healthComponent.Construct(_uiManager);
        _uiManager.UpdateHealth(_healthComponent.HealthBar, _healthComponent.CurrentHealth, _healthComponent.MaxHealth);
    }

    public void Initialize(PlayerController playerController, MonsterManager manager, CombatSystem combatSystem)
    {
        _playerController = playerController;
        _playerTransform = playerController.transform;
        _monsterManager = manager;
    }

    private void FixedUpdate()
    {
        if (_playerTransform != null)
        {
            bool playerInRange = Physics2D.OverlapCircle(transform.position, _attackRadius, _playerLayer);

            if (!_isAttacking && playerInRange)
            {
                _isAttacking = true;
                _rigidbody.velocity = Vector2.zero;
                AttackPlayer().Forget();
            }
            else if (_isAttacking && !playerInRange)
            {
                _isAttacking = false;
            }

            if (!_isAttacking)
            {
                MoveTowardsPlayer();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (_playerTransform.position - transform.position).normalized;
        _movement = direction * _speed * Time.deltaTime;
        _rigidbody.MovePosition((Vector2)transform.position + _movement);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private async UniTaskVoid AttackPlayer()
    {
        while (_isAttacking)
        {
            Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, _attackRadius, _playerLayer);
            if (hitPlayer != null && hitPlayer.CompareTag("Player"))
            {
                _playerController.TakeDamage(_damage);
            }
            await UniTask.Delay(1000);
        }
    }

    public void TakeDamage(int damage)
    {
        _healthComponent.TakeDamage(damage);
        _uiManager.UpdateHealth(_healthComponent.HealthBar, _healthComponent.CurrentHealth, _healthComponent.MaxHealth);        
        if (_healthComponent.CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void LateUpdate()
    {
        if (!CompareTag("Player"))
        {
            KeepHealthBarHorizontal();
        }
    }

    private void KeepHealthBarHorizontal()
    {
        if (_healthComponent.HealthBar != null)
        {
            _healthComponent.HealthBar.transform.rotation = Quaternion.Euler(0, 0, 0);
            _healthComponent.HealthBar.transform.position = transform.position + _healthBarOffset;
        }
    }

    private void Die()
    {
        _isAttacking = false;
        _monsterManager.OnMonsterDeath(gameObject);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }
}