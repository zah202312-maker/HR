# HR System — Backend

ASP.NET Core + ABP 10.7 + EF Core + PostgreSQL + OpenIddict. The binding API contract is `api-contract/openapi.yaml`.
Read `CLAUDE.md` and `PROGRESS.md` first.

## Run locally

1. Copy `appsettings.secrets.example.json` to `appsettings.secrets.json` in `src/HR_Mangment2.HttpApi.Host` and `src/HR_Mangment2.DbMigrator`, and fill in the real values. The secrets file (and any copy of it such as `.bak`) and `openiddict.pfx` are ignored by git and must never be committed. Keys: `ConnectionStrings:Default`, `StringEncryption:DefaultPassPhrase` (same in both), `AuthServer:CertificatePassPhrase` and `Complaints:MasterKey` in the Host, `Seed:AdminPassword` and `Seed:InitialPassword` in the migrator.
2. Install client libraries for the sign-in pages: `abp install-libs`.
3. Migrate and seed: `dotnet run --project src/HR_Mangment2.DbMigrator`.
4. Run: `dotnet run --project src/HR_Mangment2.HttpApi.Host`, then open `https://localhost:44382/swagger`.

## Tests

`dotnet test HR_Mangment2.slnx`. Tests create their own databases named `HR_Mangment2_Test_*`. The connection comes from the `HR_TEST_CONNECTION` environment variable, or from the Host secrets file with the database name replaced.
