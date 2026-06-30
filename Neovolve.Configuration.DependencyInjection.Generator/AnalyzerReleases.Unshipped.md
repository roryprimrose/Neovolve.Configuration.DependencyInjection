; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
NCDI001 | Neovolve.Configuration | Warning | Configuration type cannot be hot reloaded (value type or no writable properties); reports the root configuration type and the configuration path the type is bound at
NCDI002 | Neovolve.Configuration | Warning | Read-only collection configuration property cannot be hot reloaded; recommends a settable property on the class and a get-only property on the interface
