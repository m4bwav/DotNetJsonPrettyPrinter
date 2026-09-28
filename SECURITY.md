# Security policy

## Reporting a vulnerability

Report it privately through GitHub: open the repository's **Security** tab and choose **Report a vulnerability**. Please do not open a public issue for a security problem.

A confirmed problem is fixed in a new release, and the advisory is published once the fix is on nuget.org. Affected versions are then marked deprecated on nuget.org with the fixed version as the alternate.

## Supported versions

Only the latest major version (3.x) gets security fixes.

## What this package is not

A formatter, not a validator: it indents whatever text it is given and passes through comments, bare words and other input that is not JSON, so pretty printing is no check that a document is safe or well formed. It reads no files and makes no requests. `ToJson` and `DeserializeFromJson` are thin helpers over System.Text.Json; the overloads without a `JsonTypeInfo` use reflection, and a caller deserializing untrusted JSON should use System.Text.Json's own limits (`MaxDepth`) as for any other use of it.
