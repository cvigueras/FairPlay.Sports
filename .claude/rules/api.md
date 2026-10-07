---
paths:
  - "src/FairPlay.Sports.Api/**"
---

# Api conventions

**Api** — controller is `sealed`, `[ApiController]`, `[Route("api/[controller]")]`,
primary ctor `(ISender sender)`. Actions build the command/query, `await
_sender.Send(...)`, then `result.ToActionResult(this)` (or `CreatedAtAction`
on a successful create). Inbound request DTOs are separate records in
`Api/Users/` (e.g. `RegisterUserRequest`), mapped to the command in the action.

`Api/Auth/AuthController.cs` does not use `ResultExtensions.ToActionResult` — login/refresh
also have to set/clear the `fps_refresh_token` `HttpOnly` cookie, so it maps `Result` to
`IActionResult` by hand. Its cookie is `SameSite=None` in Development only (the Vite dev
server and the API are on different origins/schemes there) and `SameSite=Strict` otherwise;
don't "fix" this into a single constant.
