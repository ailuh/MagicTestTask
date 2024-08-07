using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 200f;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Transform _weaponTransform;
    [SerializeField] private float _fireRate = 0.2f;

    private ObjectPoolManager _poolManager;
    private ObjectPool<Spell>[] _spellPools;
    private int _currentSpellIndex;
    private InputManager _inputManager;
    private CombatSystem _combatSystem;
    private Vector2 _moveInput;
    private float _rotateInput;
    private bool _isRotating;
    private CancellationTokenSource _rotationCancellationTokenSource;
    private CancellationTokenSource _movementCancellationTokenSource;
    private CancellationTokenSource _shootingCancellationTokenSource;
    private SpellData[] _spells; 
    private Health _healthComponent;
    private UIManager _uiManager;
    
    [Inject]
    public void Construct(ObjectPoolManager poolManager, CombatSystem combatSystem, UIManager uiManager, Health healthComponent)
    {
        _poolManager = poolManager;
        _combatSystem = combatSystem;
        _uiManager = uiManager;
        _healthComponent = healthComponent;
    }
    
    private void Awake()
    {
        _inputManager = new InputManager();
        _inputManager.Player.Move.performed += OnMovePerformed;
        _inputManager.Player.Move.canceled += OnMoveCanceled;
        _inputManager.Player.Rotate.performed += OnRotatePerformed;
        _inputManager.Player.Rotate.canceled += OnRotateCanceled;
        _inputManager.Player.NextSpell.performed += _ => ChangeSpell(1);
        _inputManager.Player.PreviousSpell.performed += _ => ChangeSpell(-1);
        _inputManager.Player.Fire.performed += OnCastSpellPerformed;
        _inputManager.Player.Fire.canceled += OnCastSpellCanceled;
    }

    private void Start()
    {
        _uiManager.UpdateHealth(_healthComponent.HealthBar, _healthComponent.CurrentHealth, _healthComponent.MaxHealth);
        LoadSpellPools();
    }

    private void LoadSpellPools()
    {
        _spells = Resources.LoadAll<SpellData>("ScriptableObjects/Spells");
        _spellPools = new ObjectPool<Spell>[_spells.Length];
        for (var i = 0; i < _spells.Length; i++)
        {
            var spellData = _spells[i];
            var spellPrefab = spellData.SpellPrefab;
            _spellPools[i] = _poolManager.CreateObjectPool(spellPrefab.GetComponent<Spell>(), 10);
        }

        if (_spells.Length > 0)
        {
            _uiManager.UpdateSpellIcon(_spells[_currentSpellIndex].SpellIcon);
        }
        else
        {
            Debug.LogError("No spells found in Resources/ScriptableObjects/Spells");
        }
    }
    
    private void OnEnable()
    {
        _inputManager.Enable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
        if (_movementCancellationTokenSource == null || _movementCancellationTokenSource.IsCancellationRequested)
        {
            _movementCancellationTokenSource = new CancellationTokenSource();
            MoveContinuously(_movementCancellationTokenSource.Token).Forget();
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
        _rigidbody.velocity = Vector2.zero;
        _movementCancellationTokenSource?.Cancel();
    }

    private async UniTaskVoid MoveContinuously(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Vector2 moveDirection = transform.up * _moveInput.y * _moveSpeed;
            _rigidbody.velocity = moveDirection;
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        _rotateInput = context.ReadValue<Vector2>().x;
        if (_rotationCancellationTokenSource is { IsCancellationRequested: false }) return;
        _rotationCancellationTokenSource = new CancellationTokenSource();
        RotateContinuously(_rotationCancellationTokenSource.Token).Forget();
    }

    private void OnRotateCanceled(InputAction.CallbackContext context)
    {
        _rotateInput = 0;
        _rotationCancellationTokenSource?.Cancel();
    }

    private async UniTaskVoid RotateContinuously(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var rotation = _rotateInput * _rotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, -rotation);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }
    
    private void ChangeSpell(int direction)
    {
        _currentSpellIndex = (_currentSpellIndex + direction + _spells.Length) % _spells.Length;
        _uiManager.UpdateSpellIcon(_spells[_currentSpellIndex].SpellIcon);
        Debug.Log("Current Spell: " + _spells[_currentSpellIndex].SpellName);
    }

    private void OnCastSpellPerformed(InputAction.CallbackContext context)
    {
        if (_shootingCancellationTokenSource is { IsCancellationRequested: false }) return;
        _shootingCancellationTokenSource = new CancellationTokenSource();
        ShootContinuously(_shootingCancellationTokenSource.Token).Forget();
    }

    private void OnCastSpellCanceled(InputAction.CallbackContext context)
    {
        _shootingCancellationTokenSource?.Cancel();
    }

    private async UniTaskVoid ShootContinuously(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            CastSpell();
            await UniTask.Delay((int)(_fireRate * 1000), cancellationToken: token);
        }
    }
    
    private void CastSpell()
    { 
        Vector2 direction = transform.up;
        var spell = _spellPools[_currentSpellIndex].Get();
        spell.transform.position = _weaponTransform.position;
        spell.Initialize(_spellPools[_currentSpellIndex], _combatSystem);
        spell.SetSpeed(_spells[_currentSpellIndex].SpellSpeed);
        spell.SetDamage(_spells[_currentSpellIndex].Damage);
        spell.SetLifeTime(_spells[_currentSpellIndex].LifeTime);
        spell.SetDirection(direction);
        spell.Activate();

    }
 
    public void TakeDamage(int damage)
    {
        _healthComponent.TakeDamage(damage);
        _uiManager.UpdateHealth(_healthComponent.HealthBar, _healthComponent.CurrentHealth, _healthComponent.MaxHealth);
        if (_healthComponent.CurrentHealth <= 0)
        {
            Debug.Log("Player died");
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Boundary"))
        {
            _rigidbody.velocity = Vector2.zero;
        }
    }
    
    private void OnDisable()
    {
        _inputManager.Disable();
        _inputManager.Player.Move.performed -= OnMovePerformed;
        _inputManager.Player.Move.canceled -= OnMoveCanceled;
        _inputManager.Player.Rotate.performed -= OnRotatePerformed;
        _inputManager.Player.Rotate.canceled -= OnRotateCanceled;
        _inputManager.Player.Fire.performed -= OnCastSpellPerformed;
        _inputManager.Player.Fire.canceled -= OnCastSpellCanceled;

    }
}