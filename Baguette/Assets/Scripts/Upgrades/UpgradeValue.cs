using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UpgradeValue is a wrapper for a value that registers itself as being upgradable/changeable.
/// Upgrade systems change the value, and the scripts that have them the value is used as normal
/// </summary>
[System.Serializable]
public abstract class UpgradeValue
{
    [SerializeField]
    protected string rawValue;

    [SerializeField]
    protected string backendName;

    public string BackendName => backendName;

    public bool IsRegistered { get; set; }

    public UpgradeValue() { }

    public UpgradeValue(string s)
    {
        ParseValue(s);
    }

    ~UpgradeValue()
    {
        Deregister();
    }

    public void Init()
    {
        ParseValue(rawValue);
        Register();
    }

    public void Init(string newName, string newRawValue)
    {
        backendName = newName;
        rawValue = newRawValue;
        ParseValue(rawValue);
        Register();
    }

    /// <summary>
    /// Set this upgrade value to a new value, parsing the supplied string
    /// </summary>
    /// <param name="newValue">String representation of the new value</param>
    public virtual void SetValue(string newValue)
    {
        ParseValue(newValue);
    }

    /// <summary>
    /// Reset to our default value
    /// </summary>
    public void ResetValue()
    {
        ParseValue(rawValue);
    }

    /// <summary>
    /// Add the parsed supplied value to our value if possible
    /// </summary>
    /// <param name="addValue"></param>
    public virtual void AddValue(string addValue) { }
    /// <summary>
    /// Subtract the parsed supplied value from our value if possible
    /// </summary>
    /// <param name="subValue"></param>
    public virtual void SubtractValue(string subValue) { }

    /// <summary>
    /// Parse the supplied string into a valid value
    /// </summary>
    /// <returns>Whether parsing was successful</returns>
    protected abstract bool ParseValue(string parseString);

    /// <summary>
    /// Register this value to receive updated values
    /// </summary>
    public void Register()
    {
        UpgradeManager.RegisterUpgradeValue(this);
    }

    /// <summary>
    /// Deregister this value to no longer receive updated values
    /// </summary>
    public void Deregister()
    {
        UpgradeManager.DeregisterUpgradeValue(this);
        IsRegistered = false;
    }
}

/// <summary>
/// UpgradeValue that wraps an int value
/// </summary>
[System.Serializable]
public class UpgradeValueInt : UpgradeValue
{
    private int intValue;

    public UpgradeValueInt(string s) : base(s) { }

    public UpgradeValueInt(int i) => intValue = i;

    protected override bool ParseValue(string parseString)
    {
        if (parseString == null)
            return false;

        bool success = int.TryParse(parseString, out intValue);
        if (!success)
            throw new System.Exception($"UpgradeValueInt {backendName} could not parse value");

        return success;
    }

    public void SetValue(int i)
    {
        intValue = i;
    }

    public override void AddValue(string addValue)
    {
        int.TryParse(addValue, out int result);
        intValue += result;
    }

    public override void SubtractValue(string subValue)
    {
        int.TryParse(subValue, out int result);
        intValue -= result;
    }

    public static implicit operator int(UpgradeValueInt upi) => upi.intValue;

    public override string ToString()
    {
        return intValue.ToString();
    }
}

/// <summary>
/// UpgradeValue that wraps a float value
/// </summary>
[System.Serializable]
public class UpgradeValueFloat : UpgradeValue
{
    private float floatValue;

    public UpgradeValueFloat(string s) : base(s) { }

    public UpgradeValueFloat(float f) => floatValue = f;

    protected override bool ParseValue(string parseString)
    {
        if (parseString == null)
            return false;

        bool success = float.TryParse(parseString, out floatValue);
        if (!success)
            throw new System.Exception($"UpgradeValueInt {backendName} could not parse value");

        return success;
    }

    public void SetValue(float f)
    {
        floatValue = f;
    }

    public override void AddValue(string addValue)
    {
        float.TryParse(addValue, out float result);
        floatValue += result;
    }

    public override void SubtractValue(string subValue)
    {
        float.TryParse(subValue, out float result);
        floatValue -= result;
    }

    public static implicit operator float(UpgradeValueFloat upf) => upf.floatValue;

    public override string ToString()
    {
        return floatValue.ToString();
    }
}

/// <summary>
/// UpgradeValue that wraps a boolean value
/// </summary>
[System.Serializable]
public class UpgradeValueBool : UpgradeValue
{
    private bool boolValue;

    public UpgradeValueBool(string s) : base(s) { }

    public UpgradeValueBool(bool b) => boolValue = b;

    protected override bool ParseValue(string parseString)
    {
        if (parseString == null)
            return false;

        bool success = bool.TryParse(parseString, out boolValue);
        if (!success)
            throw new System.Exception($"UpgradeValueBool {backendName} could not parse value");

        return success;
    }

    public void SetValue(bool b)
    {
        boolValue = b;
    }

    public override void AddValue(string addValue)
    {
        bool.TryParse(addValue, out bool result);
        boolValue = result;
    }

    public static implicit operator bool(UpgradeValueBool upb) => upb.boolValue;

    public override string ToString()
    {
        return boolValue.ToString();
    }
}

/// <summary>
/// UpgradeValue that wraps a string value
/// </summary>
[System.Serializable]
public class UpgradeValueString : UpgradeValue
{
    private string stringValue;

    public UpgradeValueString(string s) => stringValue = s;

    protected override bool ParseValue(string parseString)
    {
        stringValue = parseString;
        return true;
    }
    public override void AddValue(string addValue)
    {
        stringValue += addValue;
    }

    public static implicit operator string(UpgradeValueString ups) => ups.stringValue;

    public override string ToString()
    {
        return stringValue;
    }
}