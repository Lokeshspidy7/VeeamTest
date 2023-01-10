using System;
using System.IO;
using System.Threading;

class FolderSync
{
    static void Main(string[] args)
    {
        // Get command line arguments
        string sourcePath = args[0];
        string replicaPath = args[1];
        int interval;
        try
        {
            interval = Convert.ToInt32(args[2]);
        }
        catch (FormatException)
        {
            interval = 5;
        }
        interval = interval * 60 * 60000;

        string logFile;
        try
        {
            logFile = args[3];
        }
        catch (IndexOutOfRangeException)
        {
            logFile = "sync.log";
        }


        // Write log message to console and log file
        void Log(string message)
        {
            Console.WriteLine(message);
            File.AppendAllText(logFile, message + Environment.NewLine);
        }

        // Synchronize folders
        while (true)
        {
            try
            {
                // Get source and replica directories
                var source = new DirectoryInfo(sourcePath);
                var replica = new DirectoryInfo(replicaPath);

                // Copy new and updated files from source to replica
                foreach (var file in source.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    var destination = Path.Combine(replica.FullName, file.FullName.Substring(source.FullName.Length));
                    if (!File.Exists(destination) || File.GetLastWriteTimeUtc(destination) < file.LastWriteTimeUtc)
                    {
                        File.Copy(file.FullName, destination, true);
                        Log($"Copied {file.FullName} to {destination}");
                    }
                }

                // Remove deleted files from replica
                foreach (var file in replica.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    var sourceFile = Path.Combine(source.FullName, file.FullName.Substring(replica.FullName.Length));
                    if (!File.Exists(sourceFile))
                    {
                        File.Delete(file.FullName);
                        Log($"Deleted {file.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
            Thread.Sleep(interval);
        }
    }
}
