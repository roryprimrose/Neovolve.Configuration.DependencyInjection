# Neovolve.Configuration.DependencyInjection

## Project context

This library provides `IHostBuilder` and `IServiceCollection` extension methods (`ConfigureWith<T>`) for registering strong typed configuration bindings as services, with nested type registration, hot reload, and validation. It ships a Roslyn source generator so binding, change application, and validation run without runtime reflection (AOT friendly). Targets `netstandard2.0;net8.0;net9.0;net10.0`; the generator and code fixes target `netstandard2.0`.

## Code style

- Follow [.editorconfig](../.editorconfig). 4-space indent, CRLF, UTF-8 with BOM for C#; 2 spaces for project/JSON/XML/YAML.
- Do not use `this.`. Do not add an `Async` suffix to task-returning methods.
- One type per file, file name matches the type.
- Allman braces; always brace control structures. Place binary operators at the start of wrapped lines.
- Member order: constants, static fields, instance fields, constructors, delegates, events, methods, operators, properties, indexers, nested types — public to private within each.
- Naming: `_camelCase` for private fields, PascalCase publics, camelCase locals/parameters.
- `using` statements only import namespaces actually used in the file; remove unused imports.
- xmldoc must not `<see cref>`/reference a type unless the file's code already references that type (or a related type already in scope) — do not add a dependency just to document it.
- Only add a `csproj` package reference (and central version pin) when the package is actually used; do not pin dependencies that nothing consumes.

## Accessibility

- Types and members are `internal` by default. Only make something `public`/`protected` if consumers call it directly or it is an extensibility point (interfaces like `IConfigUpdater`, `IConfigValidator`, options/enums, the extension classes) or generated code in consumer assemblies references it (everything in the `Generated` namespace).
- Concrete defaults resolved through DI (`DefaultConfigUpdater`, `DefaultConfigValidator`) stay `internal`; consumers extend via the interface.

## xmldoc

- Required on all public packable members and on every interface, enum and struct (and their members) regardless of scope.
- Order: summary, params, returns, exceptions, remarks, example. Use `<inheritdoc />` for inherited members; write null as `<c>null</c>`.

## Async

- Interface methods are task based with a `CancellationToken`. No blocking (`.Result`/`.Wait()`). Use `.ConfigureAwait(false)` in packable libraries, not in tests/examples.

## Logging

- Structured logging via `ILogger<T>` using source-generated `LoggerMessage` in a `*.Logging.cs` partial. Definitions are private/protected with a class-unique event id.

## Testing

- xUnit v3 on the Microsoft Testing Platform. Frameworks: NSubstitute (mocks), FluentAssertions, Neovolve.Streamline (`Tests<T>`/`TestsInternal`/`TestsPartsOf<T>`), Neovolve.Logging.Xunit.v3 (`output.BuildLoggerFor<T>()`), ModelBuilder.
- Test class `<Type>Tests.cs`; method names have no underscores: `<Method><ExpectedResult>` or `<Method><ExpectedResult>When<Condition>` (use `Throws<T>` for exceptions).
- Black-box only: do not widen accessibility to test; internals are reachable via `InternalsVisibleTo`. Prefer `[Theory]` over duplicate facts; vary inputs.
- Assert observable behaviour, not logs — except to prove a security guarantee (no secrets/JWTs logged).
- Build: `dotnet build Neovolve.Configuration.DependencyInjection.sln`. Test: `dotnet test Neovolve.Configuration.DependencyInjection.UnitTests\Neovolve.Configuration.DependencyInjection.UnitTests.csproj`. Coverage: `dotnet coverage collect "dotnet test <project> -f net10.0" -f cobertura -o cov.xml`.

## Project configuration

- SDK-style projects, shared props in [Directory.Build.props](../Directory.Build.props), central package versions in [Directory.Packages.props](../Directory.Packages.props). No hardcoded secrets.

## Temporary files

- Any temporary files Copilot creates must live under the `.scratch` folder (gitignored), never elsewhere in the repo.
- Place them in a per-session subfolder, e.g. `.scratch/<session>/`, and remove that folder once the files are no longer needed.
