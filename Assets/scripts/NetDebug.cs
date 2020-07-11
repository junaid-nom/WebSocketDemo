using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class NetDebug
{
    static int debugLevel = 7;//higher the level, the less that gets printed. 0-7. 7 is for player ready version

    static string textSaved = "";
    static int netLinesMax = 10;
    static string netDebug
    {
        get
        {
            string textToPrint = textSaved;
            int netDebugLines = textToPrint.Split('\n').Length;
            if (netDebugLines > netLinesMax)
            {
                string[] statements = textToPrint.Split('\n');
                //print("statementsL = " + statements.Length);
                textToPrint = "";
                for (int i = statements.Length-netLinesMax; i < statements.Length; i++)
                {
                    string end = "\n";
                    if (i == statements.Length - 1)
                    {
                        end = "";
                    }
                    textToPrint = textToPrint + statements[i] + end;
                }
            }
            return textToPrint;
        }

        set
        {
            textSaved = value;
        }

    }

    public static string getText()
    {
        return netDebug;
    }

    public static string printDictionaryKeys<T1,T2>(Dictionary<T1,T2> cb)
    {
        string keys = "";
        foreach (T1 k in cb.Keys)
        {
            keys = keys + " " + k;
        }
        return keys;
    }

    //public static void setDebugText(Text inText)
    //{
    //    netDebug = inText;
    //}

    public static void printDebug(string statement, int level)
    {
        if (level >= debugLevel)
            textSaved = textSaved + "\n" + statement;
    }

    static List<string> usedDebugStrings = new List<string>();

    /// <summary>
    /// Checks if already printed statementPrefix, then won't print again
    /// </summary>
    /// <param name="statementPreFix"></param>
    /// <param name="statementSuffix"></param>
    public static void printOnceDebug(string statementPreFix, string statementSuffix)
    {
        if (!usedDebugStrings.Contains(statementPreFix))
        {
            printDebug(statementPreFix + statementSuffix,7);
            usedDebugStrings.Add(statementPreFix);
        }
    }

    public static void printBoth(string str, int level)
    {
        if (level >= debugLevel)
        {
            printDebug(str,level);
            Debug.Log(str + " " + System.DateTime.Now.ToString("h:mm:ss tt"));
        }
    }

    public static void printBoth(string str)
    {
        int level = debugLevel;
        if (level >= debugLevel)
        {
            printDebug(str, level);
            Debug.Log(str + " " + System.DateTime.Now.ToString("h:mm:ss tt"));
        }
    }
}
