using System.Diagnostics;

namespace ProgCloud.DataAccess
{
    public class VideoTranscriptionRepository
    {
        public static void ExtractAudioFromVideo(string inputVideoPath, string outputAudioPath)
        {
            string arguments = $"-i {inputVideoPath} -vn -acodec copy {outputAudioPath}";
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        public static void ConvertAudio(string inputAudioPath, string outputAudioPath)
        {
            // Set the FFmpeg command to convert the file
            string ffmpegCommand = $"-i \"{inputAudioPath}\" -c:a flac \"{outputAudioPath}\"";

            // Create a new process to execute the FFmpeg command
            Process process = new Process();

            // Set the process start information
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg", // or full path to ffmpeg.exe
                Arguments = ffmpegCommand,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo;

            // Start the process and wait for it to exit
            process.Start();
            process.WaitForExit();
        }

    }
}
