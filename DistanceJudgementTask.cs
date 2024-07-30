using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using TMPro;  // Import TextMeshPro namespace

public class DistanceJudgmentTask : MonoBehaviour
{
    public GameObject targetPrefab;  // Assign in Inspector, but don't place in the scene initially
    public Transform spawnPoint;  // Single spawn point, target position is adjusted based on distance
    public int trialsPerDistance = 10;
    public float trialDuration = 6.0f;  // Duration each target is displayed
    public float restTime = 1.0f;  // Time between trials

    public TMP_InputField perceivedDistanceInput;  // Assign TMP_InputField in the Inspector

    private List<float> distances = new List<float> { 1f, 3f, 5f, 7f, 9f };
    private List<float> randomizedDistances = new List<float>();
    private List<TrialData> trialDataList = new List<TrialData>();

    private int currentTrial = 0;

    private void Start()
    {
        GenerateRandomizedDistances();
        StartCoroutine(RunTrials());
    }

    private void GenerateRandomizedDistances()
    {
        // Generate the list of distances with 10 trials each
        foreach (float distance in distances)
        {
            for (int i = 0; i < trialsPerDistance; i++)
            {
                randomizedDistances.Add(distance);
            }
        }

        // Shuffle the list to randomize the order
        for (int i = 0; i < randomizedDistances.Count; i++)
        {
            float temp = randomizedDistances[i];
            int randomIndex = UnityEngine.Random.Range(i, randomizedDistances.Count);
            randomizedDistances[i] = randomizedDistances[randomIndex];
            randomizedDistances[randomIndex] = temp;
        }
    }

    private IEnumerator RunTrials()
    {
        while (currentTrial < randomizedDistances.Count)
        {
            float distance = randomizedDistances[currentTrial];

            // Set the target position based on the randomized distance
            Vector3 spawnPosition = spawnPoint.position + spawnPoint.forward * distance;
            Debug.Log($"Spawning target at distance: {distance} meters, position: {spawnPosition}");
            GameObject target = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);

            float startTime = Time.time;
            bool responded = false;
            float responseTime = 0;

            // Wait for the participant's response or the trial duration
            while (Time.time - startTime < trialDuration)
            {
                // Check for participant's response (e.g., button press)
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    responseTime = Time.time - startTime;
                    responded = true;
                    break;
                }

                yield return null;
            }

            // Log the response data
            float perceivedDistance = GetPerceivedDistance();  // Implement this function to get the perceived distance
            trialDataList.Add(new TrialData
            {
                DistanceIndex = distances.IndexOf(distance),
                TrialNumber = currentTrial,
                PerceivedDistance = perceivedDistance,
                ResponseTime = responded ? responseTime : trialDuration,  // Use trial duration if no response
                Responded = responded
            });

            // Destroy the target
            Destroy(target);

            // Rest between trials
            yield return new WaitForSeconds(restTime);

            currentTrial++;
        }

        // Task complete, save data
        SaveData();
        Debug.Log("All trials completed and data saved.");
    }

    private float GetPerceivedDistance()
    {
        float perceivedDistance;
        if (float.TryParse(perceivedDistanceInput.text, out perceivedDistance))
        {
            perceivedDistanceInput.text = "";  // Clear the input field for the next trial
            return perceivedDistance;
        }
        return -1f;  // Invalid input
    }

private void SaveData()
{
    string folderPath = "/Users/arwaadib/Desktop/UO/SAP/trial_data/";
    string fileName = $"TrialData_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
    string filePath = Path.Combine(folderPath, fileName + ".csv");

        // Ensure the directory exists
        Directory.CreateDirectory(folderPath);

    // Write data to file
    using (StreamWriter writer = new StreamWriter(filePath))
    {
    writer.WriteLine("DistanceIndex,TrialNumber,PerceivedDistance,ResponseTime,Responded");
    foreach (TrialData data in trialDataList)
{
    writer.WriteLine($"{data.DistanceIndex},{data.TrialNumber},{data.PerceivedDistance},{data.ResponseTime},{data.Responded}");
}
}

Debug.Log($"Data saved to: {filePath}");
}
}

    [System.Serializable]
public class TrialData
{
    public int DistanceIndex;
    public int TrialNumber;
    public float PerceivedDistance;
    public float ResponseTime;
    public bool Responded;
}
