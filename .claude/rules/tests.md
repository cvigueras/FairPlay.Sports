---
paths:
  - "tests/**"
---

# Tests (NUnit 4 + NSubstitute + Object Mother)

- Test data **always** comes from an `XMother` in `FairPlay.Sports.TestSupport`
  (`Users/UserMother.cs`: consts + `DomainUser(...)`, `Command()`, `Dto(...)`).
  Api-only request factories stay in `Api.Tests` (`UserRequestMother`) so the
  Api reference is not dragged into `Application.Tests`.
- `Application.Tests/Users/<UseCase>/<Name>HandlerTests.cs` — unit; ports mocked
  with `Substitute.For<...>`. Assert `Result` shape, error type, and port calls.
- `Api.Tests/Users/UsersControllerTests.cs` — `ISender` mocked; assert the right
  request is dispatched and the `Result` maps to the right `IActionResult`.
- `Infrastructure.Tests` — integration against real PostgreSQL via
  **Testcontainers.PostgreSql**. `PostgreSqlContainerFixture` (`[SetUpFixture]`)
  starts one container per assembly and runs `MigrateAsync()` once;
  `RepositoryTestBase` gives fresh `DbContext`s and empties the table between
  tests; `[assembly: NonParallelizable]`. Container reuse is on locally, off on
  CI (`CI` env var). Requires a running Docker daemon.
- Frameworks: `[TestFixture]`, `Assert.That` / `Assert.Multiple`. Prefer the
  behaviour-named test style already in the suite.
