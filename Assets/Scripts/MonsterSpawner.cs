using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _monsterPrefab;
    [SerializeField] private float _spawnRadius = 10f;
    private IObjectResolver _container;
    private MonsterManager _monsterManager;

    [Inject]
    public void Construct(IObjectResolver container, MonsterManager monsterManager)
    {
        _container = container;
        _monsterManager = monsterManager;
    }

    public void SpawnMonster()
    {
        var spawnPosition = GetRandomSpawnPosition();
        var monsterObject = _container.Instantiate(_monsterPrefab, spawnPosition, Quaternion.identity);
        var monster = monsterObject.GetComponent<Monster>();
        _monsterManager.RegisterMonster(monster);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        var randomPoint = Random.insideUnitCircle * _spawnRadius;
        return transform.position + new Vector3(randomPoint.x, randomPoint.y, 0);
    }
}