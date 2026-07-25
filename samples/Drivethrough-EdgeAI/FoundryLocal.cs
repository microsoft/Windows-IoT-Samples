using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using Microsoft.AI.Foundry.Local.OpenAI;

sealed class FoundryLocal : IAsyncDisposable
{
    private readonly dynamic transcriptionModel;
    private readonly dynamic chatModel;
    private readonly FoundryLocalManager manager;

    private FoundryLocal(OpenAIAudioClient transcription, ChatClient chat, dynamic transcriptionModel, dynamic chatModel, FoundryLocalManager manager)
    {
        Transcription = transcription;
        Chat = chat;
        this.transcriptionModel = transcriptionModel;
        this.chatModel = chatModel;
        this.manager = manager;
    }

    public OpenAIAudioClient Transcription { get; }
    public ChatClient Chat { get; }

    public static async Task<FoundryLocal> Start()
    {
        var config = new Configuration
        {
            AppName = "lightning_drivethrough",
            LogLevel = Microsoft.AI.Foundry.Local.LogLevel.Warning,
            Web = new Configuration.WebService { Urls = "http://127.0.0.1:52495" }
        };

        await FoundryLocalManager.CreateAsync(config, NullLogger.Instance);
        var manager = FoundryLocalManager.Instance;

        await manager.DownloadAndRegisterEpsAsync();
        var catalog = await manager.GetCatalogAsync();

        var transcriptionModel = await catalog.GetModelAsync("nemotron-3.5-asr-streaming-0.6b")
            ?? throw new Exception("Transcription model not found");
        var chatModel = await catalog.GetModelAsync("qwen2.5-7b")
            ?? throw new Exception("Chat model not found");

        await transcriptionModel.DownloadAsync();
        await chatModel.DownloadAsync();
        await transcriptionModel.LoadAsync();
        await chatModel.LoadAsync();
        await manager.StartWebServiceAsync();

        var openAi = new OpenAIClient(
            new ApiKeyCredential("notneeded"),
            new OpenAIClientOptions { Endpoint = new Uri(config.Web.Urls + "/v1") });

        return new FoundryLocal(
            await transcriptionModel.GetAudioClientAsync(),
            openAi.GetChatClient(chatModel.Id),
            transcriptionModel,
            chatModel,
            manager);
    }

    public async ValueTask DisposeAsync()
    {
        await manager.StopWebServiceAsync();
        await transcriptionModel.UnloadAsync();
        await chatModel.UnloadAsync();
    }
}