using System;
using System.Collections.Generic;

class Parameters
{
    private static readonly string parametersPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ScreenSharingParameters.dat";

    public static bool IsAutoShareEnabled { get; set; }
    public static List<string> RecentServersList { get; set; }
    public static bool IsUsingFirstTime { get; set; }

    public static bool DidInitParameters = false;
    public static void Init()
    {
        System.Diagnostics.Debug.WriteLine(" path :::+ " + parametersPath);
        var param = new BagFile();
        try
        {
            param.Load(parametersPath);
            IsAutoShareEnabled = param.IsAutoShareEnabled;
            string[] serverList = new string[param.RecentServersList.Count];
            param.RecentServersList.CopyTo(serverList,0);
            RecentServersList = new List<string>(serverList);
            DidInitParameters = true;
        }
        catch
        {
            IsUsingFirstTime = true;
            IsAutoShareEnabled = false;
            DidInitParameters = true;
            RecentServersList = new List<string>();
            Save();
        }
    }
    public static void Save()
    {
        try
        {
            if (!DidInitParameters)
            {
                System.Diagnostics.Debug.WriteLine("Init parameters first!");
                return;
            }
            var param = new BagFile();
            param.IsAutoShareEnabled = IsAutoShareEnabled;
            string[] serverList = new string[RecentServersList.Count];
            RecentServersList.CopyTo(serverList, 0);
            param.RecentServersList = new List<string>(serverList);
            param.Save(parametersPath);
        }
        catch
        {

        }
    }
}
