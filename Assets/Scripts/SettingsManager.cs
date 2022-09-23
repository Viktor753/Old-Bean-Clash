using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SettingsManager : MonoBehaviour
{
    public abstract void Save();
    public abstract void Load();
}
