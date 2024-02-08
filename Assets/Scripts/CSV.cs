using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CSV : MonoBehaviour
{
    
    [Header("CSV File")]
    public string filePath;
    public string directory;
    public string filename;

    private string delimiter = ","; // Change this if you want to use a different delimiter

    public List<string[]> rowData = new List<string[]>();

    private StreamWriter writer;

    // Add a new row of data to the CSV
    public void AddData(params string[] values)
    {
        rowData.Add(values);
    }

    private void Start()
    {
        directory = Application.streamingAssetsPath + "/CSV_Data/";
        // Create a StreamWriter to write data to the file

    }

    public void StartWriter()
    {
        writer = new StreamWriter(filePath);
    }

    // Write the data to a CSV file
    public void WriteToCSV()
    {
        // Write each row of data to the file
        foreach (string[] row in rowData)
        {
            string line = string.Join(delimiter, row);
            writer.WriteLine(line);
        }
    }

    public void CloseCSV()
    {
        // Close the StreamWriter
        writer.Close();
    }
    
}
