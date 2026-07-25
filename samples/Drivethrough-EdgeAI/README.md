# Lightning Drive-Through - Local Voice Ordering with Foundry Local

## Overview

Lightning Drive-Through is a C# console proof-of-concept for a local AI drive-through cashier. The app keeps the microphone running, live-transcribes customer speech with Foundry Local, sends each completed utterance to a local Qwen tool-calling model, and ends the order when the model calls the receipt tool.

The goal is to show a minimal edge-AI ordering loop: local speech-to-text, local small language model reasoning, and one deterministic tool call for the final bill.

## Demo

The current demo flow is:

1. Start the console app.
2. The app loads the Foundry Local live transcription model and the Qwen chat model.
3. The app speaks the welcome message.
4. The microphone stays active while the app waits for customer speech.
5. Each final transcript is sent to the model.
6. The model asks short follow-up questions until the order is complete.
7. When ready, the model calls `make_receipt`.
8. The app prints and speaks the receipt, then exits the ordering loop.

No screenshot or GIF is checked into the repository yet. If you capture a live demo, add it under `docs\media\drive-through-demo.gif` and link it from this section.

## Prerequisites

| Area | Requirement |
| --- | --- |
| Hardware | Windows device with microphone. Snapdragon X NPU is preferred for QNN-backed Foundry Local models. |
| OS | Windows 11 recommended for Foundry Local WinML execution-provider support. |
| SDK | .NET 8 SDK. |
| Runtime | Foundry Local model downloads require network access on first run. |
| Audio | Built-in microphone or USB microphone supported by NAudio. |
| Models | Foundry Local catalog entries for `nemotron-3.5-asr-streaming-0.6b` and `qwen2.5-7b`. |

## Quick Start

```powershell
git clone <repo-url>
cd lightning_drivethrough

dotnet restore .\lightning_drivethrough.csproj
dotnet build .\lightning_drivethrough.csproj -c Debug -r win-arm64
dotnet run --project .\lightning_drivethrough.csproj -c Debug -r win-arm64
```

For x64 Windows hardware, use `-r win-x64` instead.

## Project Structure

```text
Client.cs                    Minimal drive-through loop
FoundryLocal.cs              Foundry Local setup, model loading, and OpenAI-compatible chat client
Microphone.cs                Live microphone capture and streaming transcription
Prompts.cs                   Cashier system prompt and spoken app messages
ReceiptTool.cs               OpenAI tool definition and receipt function
Menu.cs                      Menu item prices
lightning_drivethrough.csproj
                             Console project and package references
```

## Model Setup

Models are downloaded through Foundry Local at runtime. No model files are checked into the repository.

The current aliases are:

| Purpose | Alias |
| --- | --- |
| Live speech-to-text | `nemotron-3.5-asr-streaming-0.6b` |
| Tool-calling chat model | `qwen2.5-7b` |

Foundry Local WinML handles execution-provider setup and picks the available hardware path. On compatible Snapdragon X devices, the Qwen model can use the QNN NPU variant.

## Configuration

Configuration is intentionally in code:

| Setting | Location | Notes |
| --- | --- | --- |
| Transcription model | `FoundryLocal.cs` | Defaults to `nemotron-3.5-asr-streaming-0.6b`. |
| Chat model | `FoundryLocal.cs` | Defaults to `qwen2.5-7b`. |
| Menu prices | `Menu.cs` | Keep prices outside the tool schema. |
| Prompt | `Prompts.cs` | Tells the model to end by calling `make_receipt`. |
| Microphone format | `Microphone.cs` | 16 kHz, 16-bit, mono PCM. |

## Architecture

The ordering path is intentionally small:

1. `FoundryLocal` initializes Foundry Local, downloads/registers execution providers, loads the transcription and chat models, and starts the local OpenAI-compatible web service.
2. `Microphone` streams live PCM audio to the Foundry Local transcription session.
3. `Client` waits for final transcript text and appends it to the chat messages.
4. Qwen responds with either cashier text or a `make_receipt` tool call.
5. `ReceiptTool` calculates the final total from `Menu.Prices`.
6. The app speaks the receipt and exits the loop.

### Design Notes

- The app uses the regular `OpenAI` C# package for chat/tool calls.
- Foundry Local is still used for local model management, WinML execution-provider setup, and live transcription.
- There is no separate `end` tool. Receipt generation is the end condition.
- The microphone is started once and kept running until the order finishes.

## Deployment

For a loose-file deployment:

```powershell
dotnet publish .\lightning_drivethrough.csproj -c Release -r win-arm64 --self-contained true -o .\publish\win-arm64
```

Copy the published folder to the target Windows device and run `lightning_drivethrough.exe`.

## Testing

There is no test project yet. For now, the build check is:

```powershell
dotnet build .\lightning_drivethrough.csproj -c Debug -r win-arm64
```

## Known Limitations and Non-Goals

- This is a proof-of-concept, not a production point-of-sale system.
- The menu is a small hard-coded dictionary.
- The app does not handle payment, inventory, refunds, substitutions, or nutrition/allergen logic.
- Speech segmentation depends on the live transcription model and microphone conditions.
- First run can be slow because Foundry Local downloads models and execution providers.

## Troubleshooting

| Symptom | Fix |
| --- | --- |
| `Transcription model not found` | Run `foundry model list` and confirm the live ASR alias exists in your catalog. |
| `Chat model not found` | Confirm `qwen2.5-7b` is available, or change the alias in `FoundryLocal.cs`. |
| No microphone input | Check Windows microphone privacy settings and confirm the input device works. |
| Slow startup | Wait for first-run model and execution-provider downloads to finish. |
| Tool call never happens | Tighten the system prompt in `Prompts.cs` or reduce menu ambiguity. |
| Receipt throws on item lookup | Add the item to `Menu.Prices` or prompt the model to use only listed menu items. |

## Contributing and License

This repository does not currently include a license file. Do not redistribute the project or model artifacts until a license is added.
