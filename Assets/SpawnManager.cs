using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemies;
    public Transform[] spawnPositions;
    public float timeBetweenSpawns;
    public int spawnAmount;

    bool spawning;

    private void Update()
    {
        if(!spawning)
        {
            spawning = true;
            StartCoroutine(Spawn());
        }
    }

    IEnumerator Spawn()
    {
        for(int i = 0; i < spawnAmount; i++)
        {
            Instantiate(enemies[Random.Range(0, enemies.Length)], spawnPositions[Random.Range(0, spawnPositions.Length)].position, Quaternion.identity);
            yield return new WaitForSeconds(.2f);
        }
        spawnAmount++;
        yield return new WaitForSeconds(timeBetweenSpawns);
        spawning = false;
    }
}
