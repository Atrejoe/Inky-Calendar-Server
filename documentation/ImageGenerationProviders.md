# Image generation providers for AI calendar panels

Research date: 2026-11-05. Prices are list prices for one roughly 1024×1024 (1 MP) image. Most of them come from third-party
price trackers that quote the vendor pages, because the vendor pages could not be reached from the research environment.
**Check the vendor pricing page before switching.**

## Requirements

- Image quality similar to the current OpenAI integration (19th-century lithograph style, dithered to 2–7 e-ink colors).
- Lower price than the current setup.
- A client library maintained by the API vendor. If there isn't one, a client generated from the vendor's API spec and kept up to date automatically.
- A local/on-device option.

## Current situation (urgent)

`OpenAIService` used `gpt-image-1` at medium quality, about $0.042 per image. **OpenAI shut down `gpt-image-1` on 2026-10-23**,
and `gpt-image-1.5`/`gpt-image-1-mini` are scheduled to shut down on 2026-12-01. The OpenAI image model is now a setting (`OpenAIImageModel`),
and the default is `gpt-image-2`.

The current client, `OpenAI-DotNet` (RageAgainstThePixel), is a community library. OpenAI's own .NET SDK is the `OpenAI` NuGet package,
maintained by OpenAI and Microsoft.

The calendar panel needs two calls:
1. A chat completion that turns the day's events into an image prompt (cheap: a fraction of a cent).
2. The image generation (where the cost is).

## Comparison

| Provider / model | ≈ $ / image | Quality for this use case | .NET client | Notes |
|---|---|---|---|---|
| OpenAI `gpt-image-1` (old) | 0.042 | Reference | Community (`OpenAI-DotNet`) | **Shut down 2026-10-23** |
| OpenAI `gpt-image-2` medium | 0.053 | ≥ reference | Vendor (`OpenAI`) or community | Default after this change |
| OpenAI `gpt-image-2` low | 0.006 | Rougher, but often fine after dithering | Vendor (`OpenAI`) | Worth an A/B test on a real panel |
| Google `gemini-3.1-flash-lite-image` | 0.034 (0.017 batch) | Comparable, strong prompt following | **Vendor (`Google.GenAI`)** | Chat via Gemini is in the same SDK and the same key |
| Google `gemini-2.5-flash-image` | 0.039 | Comparable | Vendor (`Google.GenAI`) | Shutdown moved to 2027-03-15 |
| Google Imagen 4 (fast/std/ultra) | 0.02–0.06 | Good | Vendor (`Google.GenAI`) | **Shut down (Aug 2026)**, do not use |
| Black Forest Labs FLUX.2 [klein] 9B | ≈ 0.014 | Good, very fast | **None**, generated from OpenAPI | No chat model, so prompt composition comes from another provider |
| Black Forest Labs FLUX.2 [pro] | ≈ 0.03 | Excellent, especially for art styles | None, generated from OpenAPI | |
| Together AI FLUX.1 [schnell] | ≈ 0.003 | Noticeably lower | Vendor `OpenAI` SDK (OpenAI-compatible endpoint) | FLUX.2 also available, ≈ 0.03 |
| Amazon Bedrock (Nova Canvas, Stability) | 0.04–0.08 | Good | Vendor (`AWSSDK.BedrockRuntime`) | JSON body per model, heavier auth setup; not implemented |
| Stability AI (direct) | 0.03–0.08 | Good | None (OpenAPI only) | Not implemented |
| **Local**: LocalAI (stable-diffusion.cpp / diffusers: SDXL, FLUX.1, FLUX.2 klein, Z-Image) | 0 (hardware and power) | SDXL ≈ ok, FLUX ≈ good | Vendor `OpenAI` SDK (OpenAI-compatible `/v1/images/generations`) | Needs a GPU for reasonable speed; CPU works but takes minutes per image (fine, because images are cached) |
| **Local**: Ollama image generation (Z-Image Turbo, FLUX.2 klein) | 0 | Good | Vendor `OpenAI` SDK (OpenAI-compatible) | Experimental; macOS (MLX) first, CUDA support limited |

## Recommendation

1. **Google Gemini (`Google.GenAI`)**: the best match for all requirements. It is about 35% cheaper than `gpt-image-2` medium
   (about 70% cheaper with the batch tier), quality is similar, and both chat and image come from one vendor-maintained SDK with one API key.
   → branch `claude/image-gen-google-gemini`
2. **OpenAI-compatible provider (official `OpenAI` SDK)**: one implementation covers OpenAI itself (with the vendor SDK instead of
   the community one), Together AI, Azure OpenAI, and the **local** options LocalAI and Ollama.
   → branch `claude/image-gen-openai-compatible`
3. **Black Forest Labs FLUX.2 (generated from OpenAPI)**: the cheapest high-quality option (≈ 0.014–0.03).
   There is no vendor .NET SDK, so the client is generated from BFL's OpenAPI spec with Kiota, and a scheduled GitHub workflow refreshes the spec,
   regenerates the client, and opens a PR (reviewed and auto-updated the same way Renovate does for packages).
   Composing the prompt needs a chat model, which comes from another configured provider.
   → branch `claude/image-gen-bfl-flux`

All three build on `ImageGenerationServiceBase`, which handles serialising requests, rate limiting, profiling and error logging.
The provider is selected with the `ImageGenerationProvider` setting.

## Not chosen

- **Amazon Bedrock**: it has a vendor SDK, but image models take model-specific JSON bodies through `InvokeModel`, so the SDK does not help much.
  It also needs AWS credentials and region setup, and it is not cheaper.
- **Stability AI direct**: no vendor SDK and no price advantage over FLUX.2.
- **Replicate / fal.ai**: aggregators with no vendor .NET SDK. Their prices for FLUX are similar to Together and BFL.
- **In-process ONNX / StableDiffusion.NET**: possible, but it brings multi-GB model files and native GPU dependencies into the web server container.
  Running LocalAI or Ollama as a sidecar container gives the same result, and an OpenAI-compatible HTTP endpoint keeps the server image lean.

## Possible follow-ups

- Pass the panel aspect ratio to the provider (`GenerateImageAsync(prompt, width, height)`), so less of the image is cropped.
  Gemini supports `aspectRatio`, and FLUX accepts arbitrary width and height.
- Replace `OpenAI-DotNet` with the official `OpenAI` package in `OpenAIService`, once the OpenAI-compatible branch is merged.
