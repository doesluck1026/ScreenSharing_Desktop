using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
class BagFile
{
    public bool IsAutoShareEnabled;
    public List<string> RecentServersList;

    public void Save(string url)
    {
        FileStream writerFileStream = new FileStream(url, FileMode.Create, FileAccess.Write);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(writerFileStream, this);
        writerFileStream.Close();
    }
    public void Load(string url)
    {
        var bagFile = this;
        FileStream readerFileStream = new FileStream(url, FileMode.Open, FileAccess.Read);
        // Reconstruct data
        BinaryFormatter formatter = new BinaryFormatter();
        bagFile = (BagFile)formatter.Deserialize(readerFileStream);
        this.IsAutoShareEnabled = bagFile.IsAutoShareEnabled;
        this.RecentServersList = bagFile.RecentServersList;
        // Close the readerFileStream when we are done
        readerFileStream.Close();
    }
}
