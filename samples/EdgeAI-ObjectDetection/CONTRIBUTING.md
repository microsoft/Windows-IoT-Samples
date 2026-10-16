# Contributing

This project welcomes contributions and suggestions. Most contributions require you to
agree to a Contributor License Agreement (CLA) declaring that you have the right to,
and actually do, grant us the rights to use your contribution. For details, visit
https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need
to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the
instructions provided by the bot. You will only need to do this once across all repositories using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Pull Requests

1. Open an issue or comment on an existing issue before large changes.
2. Keep PRs focused on one issue or behavior change.
3. Include screenshots or logs when changing UI, camera, model, or deployment behavior.
4. Do not commit ONNX model files, build output, local logs, or secrets.
5. Run the build and test commands above before requesting review.

## Code Style

- Follow the existing C# and XAML style in the file you are editing.
- Prefer small, direct fixes over new abstractions.
- Keep error handling explicit for camera, model, hardware, and checkout paths.
- Update README or troubleshooting notes when setup, model, or deployment behavior changes.
