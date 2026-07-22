using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class CassetteGeneratorEditor
{
    [MenuItem("Tools/Generate Cassettes Pile")]
    public static void GeneratePiles()
    {
        CassetteData[] allLoadedFilms = Resources.LoadAll<CassetteData>("Films");
        List<CassetteData> allFilms = new List<CassetteData>();
        GameObject spawnZoneParent = GameObject.Find("SpawnZone");
        List<GameObject> spawnPoints = new List<GameObject>();
        GameObject cassettePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Cassette.prefab");
        
        if (allLoadedFilms == null)
        {
            Debug.LogError("No Films found in Resources folder!");
            return;
        }
        
        foreach (var film in allLoadedFilms)
        {
            allFilms.Add(film);
        }

        for (int i = allFilms.Count - 1; i >= 0; i--)
        {
            int newIndex = Random.Range(0, i + 1);

            (allFilms[i], allFilms[newIndex]) = (allFilms[newIndex], allFilms[i]);
        }

        if (spawnZoneParent == null)
        {
            Debug.LogError("No SpawnZone found in scene folder!");
            return;
        }

        foreach (Transform child in spawnZoneParent.transform)
        {
            spawnPoints.Add(child.gameObject);
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No SpawnZone found in scene folder!");
            return;
        }

        int index = 0;
        
        int[] cassettesPerPointCounter = new int[spawnPoints.Count];

        foreach (var film in allFilms)
        {
            for (int i = 0; i < 4; i++)
            {
                int pointIndex = index % spawnPoints.Count;
                GameObject targetPoint = spawnPoints[pointIndex];
                
                int cassettesAtThisPoint = cassettesPerPointCounter[pointIndex];
                
                int itemsPerPileCycle = 6;
                int currentPileLayer = cassettesAtThisPoint / itemsPerPileCycle; 
                int positionInLayer = cassettesAtThisPoint % itemsPerPileCycle;  
                
                float scatterRadius = 2f;
                float heightStep = 0.05f;
                
                float dynamicRadius = Mathf.Max(0.15f, scatterRadius - (currentPileLayer * 0.07f));
                
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                float randomDistance = Mathf.Sqrt(Random.Range(0f, 1f)) * dynamicRadius; 

                float randomX = Mathf.Cos(randomAngle) * randomDistance;
                float randomZ = Mathf.Sin(randomAngle) * randomDistance;

                Vector3 spawnPosition = targetPoint.transform.position;
                spawnPosition.x += randomX;
                spawnPosition.z += randomZ;
                
                float baseHeight = targetPoint.transform.position.y + (currentPileLayer * heightStep);
                spawnPosition.y = baseHeight;
                
                float randomYaw = Random.Range(0f, 360f);
                Quaternion spawnRotation = Quaternion.Euler(0f, randomYaw, 0f);
                
                Vector3 halfExtents = new Vector3(0.06f, 0.015f, 0.09f); 

                int maxAttempts = 15;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    Collider[] hitColliders = Physics.OverlapBox(spawnPosition, halfExtents, spawnRotation);
                    
                    bool hasCollision = false;
                    foreach (var hit in hitColliders)
                    {
                        if (hit.gameObject != targetPoint)
                        {
                            hasCollision = true;
                            break;
                        }
                    }

                    if (!hasCollision)
                    {
                        break;
                    }
                    spawnPosition.y += 0.03f; 
                }

                GameObject newCassette = PrefabUtility.InstantiatePrefab(cassettePrefab) as GameObject;
                newCassette.transform.position = spawnPosition;
                newCassette.transform.rotation = spawnRotation;

                newCassette.transform.position = spawnPosition;
                
                newCassette.transform.rotation = Quaternion.Euler(0f, randomYaw, 0f);

                PhysicalCassette physicalComponent = newCassette.GetComponent<PhysicalCassette>();
                if (physicalComponent != null)
                {
                    physicalComponent.Init(film);
                    EditorUtility.SetDirty(physicalComponent);
                }

                Undo.RegisterCreatedObjectUndo(newCassette, "Generate Cassette");
                
                cassettesPerPointCounter[pointIndex]++;
                index++;
            }
        }
    }
}
