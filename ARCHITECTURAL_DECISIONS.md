# Architectural Decisions

> This document records the key architectural and design decisions made during the planning and exploration phase of WebLibraryProject.
>
> **Important:** These are design decisions — not implementation records. Each entry reflects a deliberate choice made during architecture exploration, with clear reasoning for why that approach was chosen over alternatives.
>
> Build status is marked on each entry:
> - `[built]` — decision is reflected in working code
> - `[partially built]` — structure exists, full implementation incomplete
> - `[planned]` — decision is made, implementation not yet started
>
> For a precise list of what is actually implemented, see [README.md](./README.md).

---

## Index

| # | Decision | Status |
|---|---|---|
| ADR-001 | Separate `WebLibrary.AgenticApi` project instead of embedding AI in `WebLibrary` | `[built]` |
| ADR-002 | Five-project solution structure with dedicated class library projects | `[built]` |
| ADR-003 | `WebLibrary.AgenticApi` has no direct database access | `[built]` |
| ADR-004 | No RAG pipeline for metadata extraction | `[built]` |
| ADR-005 | Vector database scoped exclusively to plagiarism checking | `[built]` |
| ADR-006 | Gemini 2.0 Flash over OpenAI GPT-4o | `[built]` |
| ADR-007 | Microsoft Agent Framework over raw Semantic Kernel | `[built]` |
| ADR-008 | Multi-agent concurrent design over a single LLM call | `[partially built]` |
| ADR-009 | Plagiarism check as a function node, not an AI agent | `[built]` |
| ADR-010 | Pinecone free tier with integrated embedding | `[built]` |
| ADR-011 | Cover images staged in a temporary blob container | `[partially built]` |
| ADR-012 | 21-day sales window for the price adjustment agent | `[planned]` |
| ADR-013 | AI search to use SQL product data, not vector similarity | `[planned]` |
| ADR-014 | Separate Docker containers and App Service instances per project | `[planned]` |
| ADR-015 | API Key authentication for service-to-service calls, not JWT | `[planned]` |
| ADR-016 | Centralised exception handling via a single middleware class | `[built]` |

---

## ADR-001 — Separate `WebLibrary.AgenticApi` project instead of embedding AI in `WebLibrary` `[built]`

**Context:**
The system needs to run an AI pipeline when an admin uploads a PDF book. The initial question was whether to add this logic directly to `WebLibrary`'s `ProductController` or build a separate project.

**Design intent:**
AI concerns are separated into a dedicated `WebLibrary.AgenticApi` project rather than adding agent logic to MVC controllers.

**Reasoning:**
AI agent logic is fundamentally different in character from web application logic. It involves long-running async operations, external LLM calls with unpredictable latency, retry logic, multi-step orchestration, and specialised error handling. Mixing this into MVC controllers would violate the Single Responsibility Principle at the project level and make both concerns harder to reason about, test, and maintain independently.

A dedicated API also means the AI layer can be scaled, deployed, versioned, and tested independently. If the agent pipeline needs more compute resources, only the `WebLibrary.AgenticApi` container needs scaling — not the entire MVC application.

**Trade-off acknowledged:**
More infrastructure complexity — two deployments, two Docker containers, inter-service HTTP calls. Accepted as a worthwhile cost at this architectural level.

---

## ADR-002 — Five-project solution structure with dedicated class library projects `[built]`

**Context:**
Standard ASP.NET Core tutorials often put everything in one project. The question was how to structure a multi-concern solution cleanly.

**Design intent:**
The solution is structured into five projects: `WebLibrary` (MVC), `WebLibrary.AgenticApi` (REST API), `WebLibrary.DataAccess` (EF Core and repositories), `Weblibrary.Models` (domain entities), and `WebLibrary.Utilities` (shared helpers).

**Reasoning:**
Separating concerns at the project level enforces clean dependency rules at compile time — not just by convention. `WebLibrary.AgenticApi` cannot accidentally import `WebLibrary.DataAccess` because it does not reference it. `Weblibrary.Models` contains domain entities used by both MVC and DataAccess without either depending on the other. This mirrors real-world enterprise .NET solution structures and makes each project independently testable.

---

## ADR-003 — `WebLibrary.AgenticApi` has no direct database access `[built]`

**Context:**
The Agent API needs product data for the AI search feature and needs to write updated prices for the price adjustment agent. The question was whether to give it a direct SQL Server connection.

**Design intent:**
`WebLibrary.AgenticApi` is designed to have no connection to SQL Server. Product data is read and price updates are written exclusively through internal HTTP endpoints exposed by `WebLibrary`.

**Reasoning:**
The database belongs to `WebLibrary`. Two projects sharing direct read/write access to the same database creates two owners of the same data — a recognised architectural anti-pattern. If the Product table schema changes in `WebLibrary`, a directly connected Agent API would break silently with no compile-time safety net.

Going through `WebLibrary`'s internal API means `WebLibrary` controls exactly what data is exposed and how writes are validated. `WebLibrary.AgenticApi` acts as a consumer, not an owner. This boundary is already reflected in the build — `WebLibrary.AgenticApi` has no EF Core package, no `DbContext`, and no connection string pointing to the application database.

**Trade-off acknowledged:**
An extra HTTP hop for data access. Accepted given the benefit of clear data ownership and a stable contract between projects.

---

## ADR-004 — No RAG pipeline for metadata extraction `[built]`

**Context:**
An early design question was whether to chunk the PDF into segments, store them in a vector database, and use retrieval-augmented generation to extract metadata — a common pattern for LLM document workflows.

**Design intent:**
The metadata extraction workflow sends the first 30 pages of PDF text directly to the LLM in a single call. No chunking, no vector retrieval, and no RAG pipeline are used for this task.

**Reasoning:**
RAG is designed to solve a specific problem: answering questions that require searching across a large corpus too large to fit in an LLM context window. Metadata extraction from a single PDF is a different problem entirely. Thirty pages of extracted book text is approximately 40,000 tokens. Gemini 2.0 Flash supports a one million token context window. The entire relevant content fits in a single prompt with significant room to spare.

Adding RAG would introduce chunking logic, a vector write-before-read pattern, retrieval complexity, and additional latency — all for no benefit over a direct LLM call.

---

## ADR-005 — Vector database scoped exclusively to plagiarism checking `[built]`

**Context:**
Pinecone was introduced to the system. The design question was how broadly to use it across the pipeline.

**Design intent:**
Pinecone is used for one purpose only — semantic similarity search to detect whether a newly uploaded book is suspiciously similar to a book already in the system.

**Reasoning:**
Vector databases solve one problem well: finding semantically similar content across a stored corpus. Plagiarism detection is exactly this problem. For every other task — metadata extraction, price research, AI search — a more appropriate tool exists. Overusing the vector database would mean maintaining embeddings for every product, synchronising them on every product change, and adding retrieval complexity where it provides no advantage over a direct LLM call or SQL query.

---

## ADR-006 — Gemini 2.0 Flash over OpenAI GPT-4o `[built]`

**Context:**
The system requires an LLM for agent tasks. OpenAI GPT-4o was the initial candidate given MAF's native OpenAI support.

**Design intent:**
Google Gemini 2.0 Flash is the chosen LLM, connected via Google's OpenAI-compatible endpoint so MAF's `ChatClient` can communicate with it without a native Gemini connector.

**Reasoning:**
OpenAI has no free tier. GPT-4o costs $2.50 per million input tokens with no free allowance for new accounts in many regions. Gemini 2.0 Flash offers 1,500 requests per day and one million tokens per day on its free tier with no credit card required. For an exploration and portfolio project, cost is a genuine constraint that shapes architecture decisions.

The OpenAI-compatible endpoint (`generativelanguage.googleapis.com/v1beta/openai/`) means `Microsoft.Extensions.AI.OpenAI`'s `ChatClient` connects to Gemini by changing only the base URL and API key. No code changes are required in agent or workflow classes.

**Trade-off acknowledged:**
MAF's tooling is better tested against native OpenAI endpoints. Tool-calling behaviour via the compatibility layer may have edge cases. Mitigation: test tool-calling early before committing to it across all agents.

---

## ADR-007 — Microsoft Agent Framework over raw Semantic Kernel `[built]`

**Context:**
.NET AI orchestration options considered: Semantic Kernel, AutoGen, and the newly released Microsoft Agent Framework 1.0.

**Design intent:**
MAF 1.0 is the chosen agent orchestration framework for `WebLibrary.AgenticApi`.

**Reasoning:**
MAF 1.0 (released April 2026) is the direct successor to both Semantic Kernel and AutoGen, combining Semantic Kernel's enterprise features — dependency injection integration, middleware, type safety, telemetry — with AutoGen's multi-agent orchestration patterns including concurrent workflows and agent-to-agent communication. Semantic Kernel is now in maintenance mode. Building on MAF means building on the framework Microsoft is actively investing in, with a clear migration path for existing SK code.

**Trade-off acknowledged:**
MAF is newer and has thinner community documentation than Semantic Kernel. Accepted as a worthwhile trade-off given the direction of Microsoft's investment.

---

## ADR-008 — Multi-agent concurrent design over a single LLM call `[partially built]`

**Context:**
The PDF analysis pipeline needs to extract metadata, check for plagiarism, research pricing, and generate a cover image. The design question was whether to do this in one large LLM call or distribute it across specialised agents.

**Design intent:**
Three specialised agents are designed to run concurrently using `Task.WhenAll` — `MetadataExtractorAgent`, a plagiarism check function, and `PriceAndCoverAgent`. Each has one focused responsibility and no dependency on the others' output.

**Current build status:**
The `BookMetadataWorkflow` shell with `Task.WhenAll` is built. `MetadataExtractorAgent` and the plagiarism check function are built. `PriceAndCoverAgent` is not yet implemented.

**Reasoning:**
A single prompt asking for metadata extraction, plagiarism detection, price research, and cover generation would produce worse results than focused prompts. LLMs perform poorly when given too many responsibilities in one call. A combined approach also cannot run tasks in parallel, cannot retry one failed task without rerunning everything, and mixes fundamentally different types of work into one call. Specialised agents with narrow, defined responsibilities produce more reliable, testable, and maintainable results.

---

## ADR-009 — Plagiarism check as a function node, not an AI agent `[built]`

**Context:**
The plagiarism check was initially planned as a `PlagiarismCheckerAgent` class — a MAF `AIAgent` using the LLM to assess similarity.

**Design intent:**
The plagiarism check is a plain async function node in `BookMetadataWorkflow` that calls `VectorDbService` directly. No LLM call is involved.

**Reasoning:**
An agent is appropriate when a task requires LLM reasoning — interpreting natural language, making judgements, generating content. Plagiarism checking requires none of this. It is a mathematical operation: send text to Pinecone, receive a cosine similarity score, compare the score to a configured threshold. The result is a number, not a reasoned judgement. Wrapping this in a MAF `AIAgent` would consume LLM tokens, add latency, and introduce non-determinism where a deterministic mathematical result is both sufficient and preferable.

---

## ADR-010 — Pinecone free tier with integrated embedding `[built]`

**Context:**
Vector similarity search requires converting text to embeddings. Options considered: generate embeddings via Gemini's embedding API, use OpenAI's embedding endpoint, or use Pinecone's integrated embedding feature.

**Design intent:**
Pinecone's integrated embedding with `llama-text-embed-v2` (NVIDIA hosted) is used. `WebLibrary.AgenticApi` sends raw text to Pinecone; Pinecone handles vectorisation internally.

**Reasoning:**
Integrated embedding removes the need for a separate embedding API call entirely — no Gemini embedding call, no additional API key for a separate service, no embedding dimension management in application code. The free tier provides 2GB of storage with no credit card required — sufficient for a portfolio-scale book catalog. The Pinecone.Client 4.0.2 official SDK (`SearchRecordsAsync`, `UpsertRecordsAsync`) supports this pattern directly.

---

## ADR-011 — Cover images staged in a temporary blob container `[partially built]`

**Context:**
After the AI generates a book cover image, the admin needs to review and either accept or reject it before it becomes the product's permanent cover image.

**Design intent:**
Generated cover images are uploaded by `WebLibrary.AgenticApi` to a dedicated `bookcoverstemp` Azure Blob container. The temp URL is returned in `BookMetadataResult`. On acceptance, `WebLibrary` moves the image to the permanent container. On rejection, the temp blob is deleted. A Hangfire cleanup job removes temp covers older than one hour to handle abandoned sessions.

**Current build status:**
`BookCoverService` — including Hugging Face image generation, 2-attempt retry loop, and Azure Blob temp upload and delete — is built. The `WebLibrary` MVC accept/reject flow and Hangfire cleanup job are not yet implemented.

**Reasoning:**
`WebLibrary.AgenticApi` and `WebLibrary` are separate projects that cannot share a local filesystem. Azure Blob Storage is the canonical asset store for the application and is already integrated. A temporary staging container gives the admin a review window without polluting permanent storage. The pattern mirrors how professional systems handle provisional assets — stage first, promote on confirmation, clean up on rejection or timeout.

---

## ADR-012 — 21-day sales window for the price adjustment agent `[planned]`

**Context:**
The planned `PriceAdjustmentAgent` needs a lookback window when evaluating recent sales performance to decide whether to raise or lower a book's price.

**Design intent:**
The agent is designed to evaluate the most recent 21 days of sales data per product, running as a nightly Hangfire job. Price adjustments are bounded by admin-set limits — a floor of 60% and a ceiling of 150% of the admin-defined base price.

**Reasoning:**
A 7-day window is too short — a single day of unusually high or low sales caused by a promotion or external mention would skew the result significantly. A 30-day window is too slow to react to genuine trends. 21 days represents three full weeks of trading data, capturing a meaningful sales pattern without over-weighting short-term anomalies. Bounding the adjustment range ensures the agent cannot move prices into unreasonable territory — the admin retains control of the base price and the allowed band.

---

## ADR-013 — AI search to use SQL product data, not vector similarity `[planned]`

**Context:**
The planned AI search feature needs to handle natural language queries. The initial instinct was to use vector similarity search against stored book embeddings.

**Design intent:**
The `AiSearchAgent` is designed to retrieve the full product catalog from `WebLibrary`'s internal API and pass it to the LLM, which reasons over the structured data to rank and return relevant results. No vector database is involved in the search flow.

**Reasoning:**
Vector similarity search is designed for large corpus retrieval where document count exceeds what fits in an LLM context window. A bookstore catalog of under 500 books with short descriptions is approximately 50,000–100,000 tokens — well within Gemini's one million token context window. The LLM can receive the entire catalog, understand the user's intent contextually, and rank results with explanatory reasoning. Vector search would lose this contextual understanding and introduce significant operational overhead — embeddings must be generated for every product and kept synchronised on every product update. SQL data is simpler, always current, and sufficient at this scale.

---

## ADR-014 — Separate Docker containers and App Service instances per project `[planned]`

**Context:**
Both runnable projects need to be containerised and deployed to Azure. The question was whether to deploy them together or separately.

**Design intent:**
`WebLibrary` and `WebLibrary.AgenticApi` are designed to be built into separate Docker images using multi-stage Linux builds, deployed to separate Azure App Service instances, and sourced from a shared Azure Container Registry.

**Reasoning:**
The two projects have different resource profiles. `WebLibrary` handles fast HTTP requests with low compute requirements. `WebLibrary.AgenticApi` handles long-running LLM calls that are CPU-intensive with unpredictable latency. Separate deployments allow each to be scaled independently. Failures are isolated — if `WebLibrary.AgenticApi` becomes unavailable, the MVC bookstore continues to function. Deployment pipelines are also independent — a UI change does not require rebuilding and redeploying the AI backend.

---

## ADR-015 — API Key authentication for service-to-service calls, not JWT `[planned]`

**Context:**
`WebLibrary` needs to authenticate its calls to `WebLibrary.AgenticApi`. JWT tokens were considered as the authentication mechanism.

**Design intent:**
Service-to-service authentication between `WebLibrary` and `WebLibrary.AgenticApi` is designed to use a shared API Key sent in an `X-Api-Key` request header, validated by `ApiKeyMiddleware` in `WebLibrary.AgenticApi` on every incoming request.

**Reasoning:**
JWT is designed for user authentication — proving that a specific human user is who they claim to be, with claims, expiry, and refresh cycles. `WebLibrary.AgenticApi` has no human users. It is called exclusively by `WebLibrary`'s backend server. API Key authentication is the appropriate pattern for machine-to-machine calls: a shared secret, injected from environment variables, validated in middleware. It is simpler, has no token expiry complexity, and is standard practice for internal service authentication. JWT would add token issuance infrastructure, expiry handling, and refresh logic — significant complexity with no security benefit in a server-to-server context.

---

## ADR-016 — Centralised exception handling via a single middleware class `[built]`

**Context:**
`WebLibrary.AgenticApi` makes calls to multiple external services — Azure Blob, Gemini, Pinecone, Hugging Face — each of which can fail in different ways. The design question was whether to handle errors locally in each service or centrally.

**Design intent:**
All unhandled exceptions in `WebLibrary.AgenticApi` are caught and handled by a single `GlobalErrorMiddleware` class registered at the top of the middleware pipeline. Individual services and agents throw specific, typed exceptions and do not produce HTTP responses themselves.

**Reasoning:**
Catching all exceptions locally in every service and converting them to HTTP error responses creates duplicated, inconsistent error handling distributed across the codebase. A centralised middleware approach means one place defines the complete mapping from exception type to HTTP status code. Services stay simple and focused — they throw the appropriate exception type and trust the middleware to handle presentation. The error response format can be changed in one file. Every unhandled exception is logged in one place with full request context, ensuring consistent observability across the entire pipeline.
