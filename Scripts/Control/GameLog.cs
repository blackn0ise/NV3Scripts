using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Reflection;

public class GameLog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI log = default;
    [SerializeField] private bool logNewLine = false;
    private float timeouttimer = 0;
	public static GameLog instance;

    private void Awake()
    {
		instance = this;
        Clear();
    }

    private void Update()
    {
        if (timeouttimer > 0)
			timeouttimer -= Time.deltaTime;
		else if (log.text != "")
            Clear();
    }

    public static void Log(string text, float timeoutdelay = 10)
    {
        if (instance.logNewLine)
            instance.log.text += text + "\n";
        else
            instance.log.text = text;
        instance.timeouttimer = timeoutdelay;
    }

	public static void AddLog(string text, float timeoutdelay = 10)
	{
		instance.log.text += "\n" + text + "\n";
		instance.timeouttimer = timeoutdelay;
	}
    public static void Clear()
	{
		instance.log.text = "";
    }

#if UNITY_EDITOR

    /// <summary>
    /// Clears the Unity editor log.
    /// </summary>
    public static void ClearEditorLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    /// <summary>
    /// Logs any given variables to the Unity editor console. Recommended to also pass in a string as a first parameter as a reminder of the other parameters that will be fed in, as there is currently no simple way of printing the parameter names passed in.
    /// 
    /// <para> e.g.: </para> 
    /// <code> string varnames = "distanceRemaining, dist, extents, warpDir"; </code> 
    /// <code> Debugging.LogVariables(varnames, distanceRemaining, dist, extents, warpDir); </code> 
    /// 
    /// <para>This will output something like </para> 
    /// <code> Parameter (0) = distanceRemaining, dist, extents, warpDir </code> 
    /// <code> Parameter (1) = 1.046118 </code> 
    /// <code> Parameter (2) = 9.728371 </code> 
    /// <code> Parameter (3) = 0.5 </code> 
    /// <code> Parameter (4) = (-0.8, -0.1, -0.6) </code> 
    /// 
    /// </summary>
    public static void EditorLogVariables(params object[] objects)
    {

        for (int i = 0; i < objects.Length; i++)
        {
            Debug.Log("Parameter (" + i + ") = " + objects[i].ToString());
        }
    }

#endif
}
