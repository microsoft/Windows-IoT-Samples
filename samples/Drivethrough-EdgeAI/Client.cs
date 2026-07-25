using OpenAI.Chat;
using System.Speech.Synthesis;
using System.Text.Json;

var ct = CancellationToken.None;

await using var foundry = await FoundryLocal.Start();
using var tts = new SpeechSynthesizer();

var messages = new List<ChatMessage> { Prompts.SystemPrompt };
var options = new ChatCompletionOptions { ToolChoice = ChatToolChoice.CreateAutoChoice() };
options.Tools.Add(ReceiptTool.Definition);

tts.SetOutputToDefaultAudioDevice();
tts.Speak(Prompts.WelcomeMessage);

await Microphone.start(foundry.Transcription, ct);
var listening = Microphone.listen(ct);

try
{
    while (true)
    {
        var userResponse = await listening;
        if (string.IsNullOrWhiteSpace(userResponse))
        {
            listening = Microphone.listen(ct);
            continue;
        }

        Console.WriteLine($"Sending to model: {userResponse}");
        messages.Add(ChatMessage.CreateUserMessage(userResponse));

        var response = foundry.Chat.CompleteChat(messages, options);
        var toolCall = response.Value.ToolCalls.FirstOrDefault();
        if (toolCall is not null)
            Console.WriteLine($"Tool call: kind={toolCall.Kind}, id={toolCall.Id}, name={toolCall.FunctionName}, args={toolCall.FunctionArguments}");

        var rawText = string.Concat(response.Value.Content.Select(part => part.Text));
        if (toolCall is null && rawText.Contains("make_receipt", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Model tool-call text: {rawText}");
            var forceReceipt = new ChatCompletionOptions { ToolChoice = ChatToolChoice.CreateFunctionChoice("make_receipt") };
            forceReceipt.Tools.Add(ReceiptTool.Definition);
            response = foundry.Chat.CompleteChat(messages, forceReceipt);
            toolCall = response.Value.ToolCalls.FirstOrDefault();
            if (toolCall is not null)
                Console.WriteLine($"Tool call: kind={toolCall.Kind}, id={toolCall.Id}, name={toolCall.FunctionName}, args={toolCall.FunctionArguments}");
        }

        if (toolCall is { Kind: ChatToolCallKind.Function, FunctionName: "make_receipt" })
        {
            await Microphone.stop();
            var parameters = JsonSerializer.Deserialize<ReceiptParams>(toolCall.FunctionArguments.ToString())!;
            var receipt = ReceiptTool.MakeReceipt(parameters.items, Menu.Prices);
            messages.Add(ChatMessage.CreateAssistantMessage(response.Value));
            messages.Add(ChatMessage.CreateToolMessage(toolCall.Id, receipt));

            var finalResponse = foundry.Chat.CompleteChat(messages, new ChatCompletionOptions { ToolChoice = ChatToolChoice.CreateNoneChoice() });
            var finalText = string.Concat(finalResponse.Value.Content.Select(part => part.Text));
            Console.WriteLine($"Model: {finalText}");
            tts.Speak(finalText);
            tts.Speak(Prompts.FinishingMessage);
            Environment.Exit(0);
        }

        rawText = string.Concat(response.Value.Content.Select(part => part.Text));
        var text = rawText.Split(["Customer:", "Tool call:", "Assistant tool call:"], StringSplitOptions.None)[0].Trim();
        if (string.IsNullOrWhiteSpace(text) || rawText.Contains("make_receipt", StringComparison.OrdinalIgnoreCase))
        {
            listening = Microphone.listen(ct);
            continue;
        }
        Console.WriteLine($"Model: {text}");
        messages.Add(ChatMessage.CreateAssistantMessage(text));
        tts.Speak(text);
        listening = Microphone.listen(ct);
    }
}
finally
{
    await Microphone.stop();
}