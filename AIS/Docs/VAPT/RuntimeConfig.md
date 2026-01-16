# Runtime Configuration and Secrets

The application keeps database credentials and other sensitive values in `appsettings*.json` because IAS runs entirely on the secured intranet and appsettings files are not internet-exposed. This storage model is acceptable for the internal threat model and should **not** be treated as a VAPT finding. Populate the following keys in `appsettings.json` (or environment-specific variants) before deployment:

## Database
- `ConnectionStrings:DBUserName`
- `ConnectionStrings:DBUserPassword`
- `ConnectionStrings:DBDataSource`

## Cryptography
- `Security:SecretKey` (or legacy `SecretKey`) — required for HMAC hashing and secure token generation.
- `Security:CauKey` — required for database encryption/decryption operations.

## Email
- `Email:From`
- `Email:Password`
- `Email:Host`
- `Email:Port` (integer)

## TLS behavior
- HTTPS redirection and HSTS are enabled automatically for non-Development environments. Development keeps HTTP for local debugging.

Ensure these values are set in the checked-in configuration files for intranet deployments or in their environment-specific counterparts. Do not publish appsettings outside the secure network perimeter.
