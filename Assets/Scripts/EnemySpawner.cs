using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyWalker enemyPrefab;   
    public float firstDelay = 1f;
    public float spawnInterval = 3f;

    void Start() => InvokeRepeating(nameof(Spawn), firstDelay, spawnInterval);

    void Spawn()
    {
        var cam = Camera.main;
        if (!cam || !enemyPrefab) return;

        bool fromLeft = Random.value < 0.5f;

        Vector3 left = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0));
        Vector3 right = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, 0));

        Vector3 pos = fromLeft ? left : right;
        pos.z = 0;

        var e = Instantiate(enemyPrefab, pos, Quaternion.identity);
        e.startMovingRight = fromLeft;
    }
}
