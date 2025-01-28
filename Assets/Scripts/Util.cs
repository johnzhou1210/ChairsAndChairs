using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static T Choice<T>(List<T> choices) {
        if (choices == null || choices.Count == 0) {
            throw new System.Exception("No choices");
        }
        return choices[Random.Range(0, choices.Count)];
    }
    
}
