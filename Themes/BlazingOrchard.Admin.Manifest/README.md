# BlazingOrchard.Admin.Manifest

This project registers the `BlazingOrchard.Admin` Orchard admin theme from a server-side assembly.

The Blazor WebAssembly app lives in:

```text
../BlazingOrchard.Admin
```

The manifest is separate because the browser-wasm project cannot compile Orchard server-side manifest attributes without pulling in server runtime references that are invalid for `browser-wasm`.

## Versioning

Current compatibility version: `3.0.0.0.0`.

Version format:

```text
{orchard-major}.{orchard-minor}.{orchard-patch}.{blazing-security}.{blazing-bug}
```

The manifest `Version` is the public Orchard-facing five-part version. .NET assembly/package properties use safe forms where required, with the five-part version stored as informational metadata.
