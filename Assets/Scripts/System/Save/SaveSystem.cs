using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class SaveSystem: AbstractSystem
{
    public void Save()
    {
        
    }

    public void Load()
    {
        
    }

    private HashSet<string> mSavedKeys = new HashSet<string>();

    public void SaveBool(string key, bool value)
    {
        mSavedKeys.Add(key);
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }   

    public bool LoadBool(string key,bool defaultValue = false)
    {
        mSavedKeys.Add(key);
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }

    public void SaveInt(string key, int value)
    {
        mSavedKeys.Add(key);
        PlayerPrefs.SetInt(key, value);
    }   

    public int LoadInt(string key,int defaultValue = 0)
    {
        mSavedKeys.Add(key);
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public void SaveString(string key, string value)
    {
        mSavedKeys.Add(key);
        PlayerPrefs.SetString(key, value);
    }   

    public string LoadString(string key,string defaultValue = "")
    {
        mSavedKeys.Add(key);
        return PlayerPrefs.GetString(key, defaultValue);
    }

    protected override void OnInit()
    {
        ActionKit.OnGUI.Register(() =>
        {
            if (Input.GetKey(KeyCode.L))
            {
                foreach (var key in mSavedKeys)
                {
                    GUILayout.Label(key + ": " + PlayerPrefs.GetInt(key));
                    GUILayout.Label(key + ": " + PlayerPrefs.GetString(key));
                    GUILayout.Label(key + ": " + PlayerPrefs.GetInt(key));
                }
            }
        });
    }
}
