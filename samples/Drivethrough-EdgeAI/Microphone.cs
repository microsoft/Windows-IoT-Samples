using NAudio.Wave;
using Microsoft.AI.Foundry.Local;
using Microsoft.AI.Foundry.Local.OpenAI;

static class Microphone
{
    const int SilenceMsToStop = 2000;
    const short SpeechThreshold = 500; // ponytail: tune this if the mic is too quiet/noisy.
    static OpenAIAudioClient? audioClient;
    static WaveInEvent? waveIn;
    static Action<WaveInEventArgs>? onAudio;

    public static Task start(OpenAIAudioClient client, CancellationToken ct)
    {
        audioClient = client;
        waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 16, 1), BufferMilliseconds = 100 };
        waveIn.DataAvailable += (_, e) => onAudio?.Invoke(e);
        waveIn.StartRecording();
        return Task.CompletedTask;
    }

    public static async Task<string> listen(CancellationToken ct)
    {
        var text = new List<string>();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var session = audioClient!.CreateLiveTranscriptionSession();

        session.Settings.SampleRate = 16000;
        session.Settings.Channels = 1;
        session.Settings.Language = "en";

        await session.StartAsync(ct);
        var read = Task.Run(async () =>
        {
            await foreach (var result in session.GetStream(CancellationToken.None))
            {
                var transcript = result.Content?[0]?.Text;
                if (result.IsFinal && !string.IsNullOrWhiteSpace(transcript))
                {
                    text.Add(transcript);
                    done.TrySetResult();
                }
            }
        });

        var heardSpeech = false;
        var silentMs = 0;
        var recording = true;
        onAudio = e =>
        {
            if (!recording) return;
            if (isSpeech(e.Buffer, e.BytesRecorded)) { heardSpeech = true; silentMs = 0; }
            else if (heardSpeech && (silentMs += waveIn!.BufferMilliseconds) >= SilenceMsToStop) done.TrySetResult();
            session.AppendAsync(e.Buffer.AsMemory(0, e.BytesRecorded), ct).GetAwaiter().GetResult();
        };

        Console.WriteLine("Listening...");
        await done.Task.WaitAsync(ct);
        recording = false;
        onAudio = null;
        await session.StopAsync(CancellationToken.None);
        await read;

        var heard = string.Join(" ", text).Trim();
        return heard;
    }

    public static Task stop()
    {
        onAudio = null;
        waveIn?.StopRecording();
        waveIn?.Dispose();
        waveIn = null;
        return Task.CompletedTask;
    }

    static bool isSpeech(byte[] buffer, int bytes)
    {
        for (var i = 0; i + 1 < bytes; i += 2)
            if (Math.Abs((int)BitConverter.ToInt16(buffer, i)) > SpeechThreshold) return true;
        return false;
    }
}
