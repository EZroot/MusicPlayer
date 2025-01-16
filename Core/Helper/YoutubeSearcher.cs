using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class YouTubeSearcher
{
    public static List<VideoInfo> SearchYouTube(string query)
    {
        var videoList = new List<VideoInfo>();
        var process = new Process();

        try
        {
            var args = $"ytsearch5:\"{query}\" --dump-single-json --flat-playlist";
            process.StartInfo.FileName = "yt-dlp";
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            dynamic jsonOutput = Newtonsoft.Json.JsonConvert.DeserializeObject(output);
            foreach (var entry in jsonOutput.entries)
            {
                videoList.Add(new VideoInfo
                {
                    Title = entry.title,
                    Url = entry.url,
                    Duration = FormatDuration((int)entry.duration)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return videoList;
    }
    
    public static void DownloadAudio(string url, string outputPath, Action<string, string> progressCallback)
    {
        var process = new Process();
        try
        {
            var args = $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"{outputPath}\" \"{url}\"";
            process.StartInfo.FileName = "yt-dlp";
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    // Regular expression to capture download progress
                    var match = Regex.Match(e.Data, @"\d+\.\d+% of.*?at\s+(.*?)\s+ETA");
                    if (match.Success)
                    {
                        string percent = match.Value.Split(' ')[0]; // Get the percentage of the download
                        string speed = match.Groups[1].Value; // Get the download speed
                        progressCallback(percent, speed);
                    }
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading audio: {ex.Message}");
        }
        finally
        {
            process.Close();
        }
    }
    private static string FormatDuration(int duration)
    {
        TimeSpan time = TimeSpan.FromSeconds(duration);
        return time.ToString(@"hh\:mm\:ss");
    }
}

public class VideoInfo
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string Duration { get; set; } 
}