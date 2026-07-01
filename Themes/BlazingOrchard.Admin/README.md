# BlazingOrchard.Admin

BlazingOrchard.Admin is the Blazor WebAssembly admin UI for BlazingOrchard.

The Orchard server-side theme manifest is provided by the sibling project:

```text
../BlazingOrchard.Admin.Manifest
```

This split keeps the browser-wasm Blazor app separate from the server-side Orchard theme manifest because browser-wasm projects cannot reference the Orchard server runtime stack.

## Versioning

This theme follows the BlazingOrchard five-part compatibility version:

```text
{orchard-major}.{orchard-minor}.{orchard-patch}.{blazing-security}.{blazing-bug}
```

Current compatibility version: `3.0.0.0.0`.

- `3.0.0` = Orchard Core compatibility line.
- trailing `.0.0` = BlazingOrchard-owned security and bug counters.

When Orchard Core moves to a new tested compatibility line, the first three parts change. The Blazing-owned counters reset on a new Orchard main release.
