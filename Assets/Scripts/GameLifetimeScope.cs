using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private Health _healthComponent;
    [SerializeField] private GameObject _monsterPrefab;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CombatSystem>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

        builder.RegisterComponentInHierarchy<PlayerController>();
        builder.RegisterComponentInHierarchy<ObjectPoolManager>();
        builder.RegisterComponentInHierarchy<MonsterManager>();
        
        builder.RegisterInstance(_monsterPrefab).As<GameObject>();
        builder.RegisterComponent(_uiManager);
        builder.RegisterComponent(_healthComponent);
    }
}
