using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] private List<MonsterSpawner> _monsterSpawners;
    [SerializeField] private int _maxMonsters = 10;
    private List<Monster> _activeMonsters = new();
    private IObjectResolver _container;
    private PlayerController _playerController;
    private CombatSystem _combatSystem;

    [Inject]
    public void Construct(IObjectResolver container, PlayerController playerController, CombatSystem combatSystem)
    {
        _container = container;
        _playerController = playerController;
        _combatSystem = combatSystem;
        foreach (var spawner in _monsterSpawners)
        {
            spawner.Construct(_container, this);
        }
    }

    private void Start()
    {
        SpawnInitialMonsters();
        MaintainMonsterCount().Forget();
    }

    private void SpawnMonsterAtSpawner(int spawnerIndex)
    {
        if (spawnerIndex >= 0 && spawnerIndex < _monsterSpawners.Count)
        {
            _monsterSpawners[spawnerIndex].SpawnMonster();
        }
        else
        {
            Debug.LogWarning("Invalid spawner index.");
        }
    }

    public void RegisterMonster(Monster monster)
    {
        monster.Initialize(_playerController, this, _combatSystem);
        _activeMonsters.Add(monster);
    }

    public void OnMonsterDeath(GameObject monsterObject)
    {
        var monster = monsterObject.GetComponent<Monster>();
        if (monster == null) return;
        _activeMonsters.Remove(monster);
        Destroy(monsterObject);
    }

    private void SpawnInitialMonsters()
    {
        for (int i = 0; i < _maxMonsters; i++)
        {
            int spawnerIndex = Random.Range(0, _monsterSpawners.Count);
            SpawnMonsterAtSpawner(spawnerIndex);
        }
    }

    private async UniTaskVoid MaintainMonsterCount()
    {
        while (true)
        {
            while (_activeMonsters.Count < _maxMonsters)
            {
                var spawnerIndex = Random.Range(0, _monsterSpawners.Count);
                SpawnMonsterAtSpawner(spawnerIndex);
                await UniTask.Yield();
            }
            await UniTask.Yield();
        }
    }
}