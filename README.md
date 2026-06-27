# WebLibraryProject — AI-Integrated .NET Bookstore System

An ASP.NET Core bookstore with a companion multi-agent AI backend for automated book metadata extraction, plagiarism detection, and cover image generation.

> **Development is intentionally paused.**
> Focus has shifted to strengthening core C# and .NET fundamentals before resuming.
> See [ARCHITECTURE.md](./ARCHITECTURE.md) for full system design and [ARCHITECTURAL_DECISIONS.md](./ARCHITECTURAL_DECISIONS.md) for reasoning behind every key technical choice.

---

## What this is

A five-project .NET solution:

| Project | Type | Purpose |
|---|---|---|
| `WebLibrary` | ASP.NET Core MVC | Bookstore frontend and admin panel |
| `WebLibrary.AgenticApi` | ASP.NET Core Web API | Multi-agent AI pipeline |
| `WebLibrary.DataAccess` | Class Library | EF Core, DbContext, Repository, Unit of Work |
| `Weblibrary.Models` | Class Library | Domain entities and shared contracts |
| `WebLibrary.Utilities` | Class Library | Shared constants and helpers |

The AI pipeline in `WebLibrary.AgenticApi` is triggered when an admin uploads a PDF book. It runs multiple agents concurrently to extract metadata, check for plagiarism, and generate a cover image — returning structured results to pre-fill the product form in the MVC application.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Web framework | ASP.NET Core (.NET 10) |
| Language | C# 13 |
| ORM | Entity Framework Core |
| Database | SQL Server |
| LLM | Gemini 2.0 Flash (via OpenAI-compatible endpoint) |
| LLM abstraction | Microsoft.Extensions.AI |
| Agent framework | Microsoft Agent Framework 1.0 (MAF) |
| Vector database | Pinecone (free tier — integrated embedding via llama-text-embed-v2) |
| Image generation | Hugging Face — FLUX.1-schnell |
| PDF extraction | itext7 |
| Blob storage | Azure Blob Storage |
| Logging | Serilog (structured, daily rolling file + console) |
| API documentation | ASP.NET Core built-in OpenAPI |
| Containerisation | Docker (Linux containers) |

---

## What is actually built

### WebLibrary (MVC)
- Product CRUD — create, read, update, delete with form validation
- Category CRUD — full management
- Repository pattern with generic `Repository<T>`
- Unit of Work pattern coordinating multiple repositories
- EF Core with SQL Server — full migrations history
- ASP.NET Core Identity — basic user authentication

### WebLibrary.DataAccess
- `ApplicationDbContext` with EF Core
- Generic `IRepository<T>` and `Repository<T>` implementation
- `IUnitOfWork` and `UnitOfWork` coordinating all repositories
- Full migration history

### Weblibrary.Models
- Domain entities — `Product`, `Category`, `ApplicationUser`
- Shared data contracts used across projects

### WebLibrary.Utilities
- `SD.cs` — static details, role constants, shared string values

### WebLibrary.AgenticApi
- `Program.cs` — DI registration, Serilog, CORS, OpenAPI, Gemini client wiring
- `appsettings.json` — structured config for all services
- `GlobalErrorMiddleware` — all exceptions caught centrally, returned as typed JSON with correct HTTP status codes
- `RequestLoggingMiddleware` — every request logged with method, path, status code, and elapsed time
- `AnalyzePdfRequest` — input contract with `[Required]`, `[Url]`, `[Range]` validation attributes
- `BookMetadataResult` — typed response with nested `PlagiarismInfo` and `AgentMetadata`
- `AgentOutputModels` — per-agent typed output contracts with confidence scoring
- `BlobFetcherService` — downloads PDF bytes from Azure Blob URL, enforces 50MB size guardrail
- `PdfExtractorService` — text extraction via itext7, enforces 100-page limit, detects unreadable scanned PDFs
- `GuardrailService` — input text validation, LLM output confidence threshold check, price sanity range check
- `VectorDbService` — Pinecone integration for plagiarism similarity search and book embedding storage using integrated embedding
- `BookCoverService` — Hugging Face image generation with 2-attempt retry loop, Azure Blob temp container upload and delete
- `MetadataExtractorAgent` — Gemini structured JSON output with confidence scoring and output guardrail validation
- `ProductAnalysisController` — `POST /api/productanalysis/analyze-pdf`, all four response codes documented via `[ProducesResponseType]`
- `BookMetadataWorkflow` — sequential + concurrent orchestration using `Task.WhenAll` for parallel agent execution

---

## What is not built yet

### WebLibrary (MVC)
- Azure Blob Storage integration for file uploads
- Custom UI (DM Sans font, teal accent, Tabler Icons, dark/light CSS variables, collapsible sidebar)
- Stripe payment integration
- Shopping cart and order management
- Email sending via ASP.NET Core Identity
- Role management UI

### WebLibrary.AgenticApi
- `PriceAndCoverAgent` — price web search + cover prompt generation + Gemini vision quality check
- `WebSearchTool` — MAF plugin wrapping Bing Search API
- `AiSearchAgent` — natural language product search over SQL catalog via internal HTTP
- `PriceAdjustmentAgent` — dynamic pricing based on 21-day sales window via Hangfire
- MVC → Agent API integration (HTTP call from `ProductController`)
- Admin UI for accepting or rejecting AI-suggested cover images
- `StoreBookEmbeddingAsync` call after admin confirms product save
- API Key authentication middleware
- Health check endpoints for LLM, blob, and vector DB
- Rate limiting middleware
- CI/CD pipeline (GitHub Actions)
- Azure deployment
- Unit tests

---

## Running locally

```bash
# Clone the repo
git clone https://github.com/Harsh2292/Web-library.git

# Restore packages
dotnet restore

# Set up secrets for WebLibrary.AgenticApi
# Create WebLibrary.AgenticApi/appsettings.Development.json with:
# {
#   "AzureBlob": { "ConnectionString": "..." },
#   "Gemini": { "ApiKey": "..." },
#   "VectorDb": { "ApiKey": "...", "IndexName": "web-library" },
#   "HuggingFace": { "ApiKey": "..." }
# }

# Apply EF Core migrations (from solution root)
dotnet ef database update --project Weblibrary.DataAccess --startup-project WebLibrary

# Run WebLibrary MVC
dotnet run --project WebLibrary

# Run WebLibrary.AgenticApi (separate terminal)
dotnet run --project WebLibrary.AgenticApi

# AgenticApi OpenAPI spec available at:
# https://localhost:7111/openapi/v1.json
```

---

## Development Status

**Intentionally paused.** The architecture is designed, the core Agent API pipeline is partially implemented, and the MVC CRUD foundation is complete. Development is paused to focus on strengthening core C# and .NET fundamentals. This is an architecture exploration and portfolio project — not a production-ready system.

---

## Author

**Harsh Patel** — .NET Backend Developer
[GitHub](https://github.com/Harsh2292) · [LinkedIn](https://linkedin.com/in/)
