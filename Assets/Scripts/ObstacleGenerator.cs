using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ObstacleGenerator : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float initialScrollSpeed = 2f;
    public float spawnInterval = 2f;
    public float distanceBetweenObstacles = 5f;
    public int poolSize = 10;

    private bool isGenerating = false;
    private List<GameObject> obstaclePool;
    private float timeSinceLastSpawn;
    private float totalDistance;
    private float currentScrollSpeed;
    private int lastSpeedIncreaseDistance = 0;

    void Start()
    {
        obstaclePool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obstacle = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity);
            obstacle.SetActive(false);
            obstaclePool.Add(obstacle);
        }
        currentScrollSpeed = initialScrollSpeed;
    }

    void Update()
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.Playing && isGenerating)
        {
            timeSinceLastSpawn += Time.deltaTime;
            totalDistance += currentScrollSpeed * Time.deltaTime;

            if (timeSinceLastSpawn >= spawnInterval)
            {
                SpawnObstacle();
                timeSinceLastSpawn = 0f;
            }

            MoveObstacles();
            CheckObstaclesBounds();
            UpdateSpeed();

            GameManager.Instance.UpdateDistance(totalDistance);
        }
    }

    void SpawnObstacle()
    {
        GameObject obstacle = GetPooledObstacle();
        if (obstacle != null)
        {
            float xPos = Random.value > 0.5f ? -1.5f : 1.5f;
            obstacle.transform.position = new Vector3(xPos, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y + 1f, 0);
            obstacle.SetActive(true);
        }
    }

    GameObject GetPooledObstacle()
    {
        return obstaclePool.Find(o => !o.activeInHierarchy);
    }

    void MoveObstacles()
    {
        foreach (GameObject obstacle in obstaclePool)
        {
            if (obstacle.activeInHierarchy)
            {
                obstacle.transform.Translate(Vector3.down * currentScrollSpeed * Time.deltaTime);
            }
        }
    }

    void CheckObstaclesBounds()
    {
        foreach (GameObject obstacle in obstaclePool)
        {
            if (obstacle.activeInHierarchy && obstacle.transform.position.y < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 1f)
            {
                obstacle.SetActive(false);
            }
        }
    }

    void UpdateSpeed()
    {
        int currentDistanceInt = Mathf.FloorToInt(totalDistance);

        // Увеличение скорости каждые 100 метров (максимум +5)
        if (currentDistanceInt / 100 > lastSpeedIncreaseDistance / 100 && currentScrollSpeed < initialScrollSpeed + 5)
        {
            currentScrollSpeed += 1f;
            lastSpeedIncreaseDistance = currentDistanceInt;
        }
        // Дополнительное увеличение скорости каждые 1000 метров (максимум еще +5)
        else if (currentDistanceInt / 1000 > lastSpeedIncreaseDistance / 1000 && currentScrollSpeed < initialScrollSpeed + 10)
        {
            currentScrollSpeed += 1f;
            lastSpeedIncreaseDistance = currentDistanceInt;
        }
    }

    public void StartGenerating()
    {
        isGenerating = true;
        currentScrollSpeed = initialScrollSpeed;
        lastSpeedIncreaseDistance = 0;
    }

    public void StopGenerating()
    {
        isGenerating = false;
    }

    public void ResetObstacles()
    {
        foreach (GameObject obstacle in obstaclePool)
        {
            obstacle.SetActive(false);
        }
        timeSinceLastSpawn = 0f;
        totalDistance = 0f;
        isGenerating = false;
        currentScrollSpeed = initialScrollSpeed;
        lastSpeedIncreaseDistance = 0;
    }
}