using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DataParser
{
    private static DataParser sharedInstance;

    public static DataParser Instance()
    {
        if (sharedInstance == null)
            sharedInstance = new DataParser();
        return sharedInstance;
    }

    private DataParser()
    {

    }

    public int[,] ParseIntDataTable(TextAsset asset)
    {
        int[,] parsedTable;

        string textContent = asset.text;
        string[] tempList = textContent.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        string[] rowContents = new string[0];
        int numCols = tempList[0].Split(new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries).Length;
        parsedTable = new int[tempList.Length + 1, numCols + 1];//buffer of 1 row and column of 0s
        for (int i = 0; i < tempList.Length + 1; i++)
        {      
            if (i < tempList.Length)
                rowContents = tempList[i].Split(new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < numCols + 1; j++)
            {
                if (i == tempList.Length || j == numCols)
                    parsedTable[i, j] = 0;
                else if (int.TryParse(rowContents[j], out int x))
                {
                    parsedTable[i, j] = x;
                }
            }
        }

        return parsedTable;
    }

    public float[,] ParseFloatDataTable(TextAsset asset)
    {
        float[,] parsedTable;

        string textContent = asset.text;
        string[] tempList = textContent.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        string[] rowContents = new string[0];
        int numCols = tempList[0].Split(new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries).Length;
        parsedTable = new float[tempList.Length, numCols];
        for (int i = 0; i < tempList.Length; i++)
        {
            rowContents = tempList[i].Split(new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < numCols; j++)
            {
                if (float.TryParse(rowContents[j], out float x))
                {
                    parsedTable[i, j] = x;
                }
            }
        }

        return parsedTable;
    }
}
