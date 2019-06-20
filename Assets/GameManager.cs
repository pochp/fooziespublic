using Assets.Utility.Config;
using Assets.Utility.Netplay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    const string ConfigFilePath = @"\..\..\config.xml";

    public SoundManager SoundManager;
    private ConfigSettings _configSettings = null;
    public ConfigSettings Configuration
    {
        get
        {
            if(_configSettings == null)
            {
                //locate assembly
                var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //find if config file exists
                var configFullPath = assemblyLocation + ConfigFilePath;
                //if doesn't or can't be opened, send warning
                FileStream fs = null;
                try
                {
                    using (fs = new FileStream(configFullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        // Code here
                        //var formatter = new BinaryFormatter();
                        //var config = (ConfigSettings)formatter.Deserialize(fs);
                        byte[] b = new byte[1024];
                        UTF8Encoding temp = new UTF8Encoding(true);
                        string fileContent = string.Empty;
                        while (fs.Read(b, 0, b.Length) > 0)
                        {
                            fileContent += temp.GetString(b);
                        }
                        _configSettings = JsonUtility.FromJson(fileContent, typeof(ConfigSettings)) as ConfigSettings;
                    }
                }
                finally
                {
                    if (fs != null)
                        fs.Dispose();
                }
            }
            return _configSettings;
        }
    }

    public void CreateConfigFile()
    {
        ConfigSettings config = new ConfigSettings();
        //locate assembly
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //find if config file exists
        var path = assemblyLocation + ConfigFilePath;
        //if doesn't or can't be opened, send warning
        FileStream fs = null;
        try
        {
            // Delete the file if it exists.
            if (File.Exists(path))
            {
                // Note that no lock is put on the
                // file and the possibility exists
                // that another process could do
                // something with it between
                // the calls to Exists and Delete.
                File.Delete(path);
            }

            // Create the file.
            using (fs = File.Create(path))
            {
                //Byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
                //// Add some information to the file.
                //fs.Write(info, 0, info.Length);

                // Code here
                //var formatter = new BinaryFormatter();
                //formatter.Serialize(fs, config);
                var configStr = JsonUtility.ToJson(config);
                AddText(fs, configStr);
            }
        }
        catch(Exception e)
        {

        }
        finally
        {
            if (fs != null)
                fs.Dispose();
        }
    }

    private static void AddText(FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }

    public bool IsOnlineMatch = false;

    private NetplayState _netplayState = null;
    public NetplayState NetplayState
    {
        get
        {
            if (_netplayState == null)
                _netplayState = new NetplayState();
            return _netplayState;
        }
    }
}

