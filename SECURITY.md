# Security Policy — NainNotify

## Supported Versions
Only the latest release targeting .NET 10 on the `main` branch is actively supported with security patches.

| Version | Supported |
| ------- | --------- |
| 1.0.x   | Yes       |
| < 1.0   | No        |

## Reporting a Vulnerability
If you discover a vulnerability or security flaw, please report it privately to `security@naindev.com`.
Security assessments are conducted within 48 hours.

## Privacy & Protection Architecture
1. **PII Masking (`PrivacyMasker`)**: All recipient email addresses and sensitive customer identifiers are masked (e.g. `c***@example.com`) before being logged or broadcasted over public telemetry streams.
2. **Anti-Spam Honeypot**: The contact submission endpoint enforces a honeypot field (`Website`). Automated bot submissions filling hidden fields are silently discarded without queuing messages.
3. **Partitioned Rate Limiting**: All write endpoints are bounded per client IP using fixed-window limiters to safeguard external SMTP providers against abuse.
4. **Recipient Sandboxing**: In demo mode, outgoing messages are trapped or routed exclusively to pre-authorized test inboxes to prevent unintended external delivery.
