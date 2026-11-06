# Security Policy

## Project status

Shop is undergoing modernization and is not currently intended for production use. The legacy application contains historical implementation patterns that are being reviewed.

## Reporting a vulnerability

Please do not publish exploit details, credentials, personal data or sensitive screenshots in a public issue. Report suspected vulnerabilities privately to the repository owner through GitHub's private vulnerability reporting feature when available.

## Security baseline for the modernization

- No hardcoded passwords, API keys, personal email addresses or phone numbers.
- Authentication required for private catalog management.
- Authorization enforced on every read or write involving user-owned data.
- Secrets supplied through environment variables or a local secret store.
- Supported .NET and NuGet dependencies only.
- HTTPS, secure cookies, antiforgery protection and safe error handling.
- Automated tests for authentication, authorization and ownership boundaries.

The presence of this policy does not mean the current application has passed a security review.
