# Common starter

This repository is the shared library for future .NET API services. Each API references `Common` and is the backend for its frontend. The database is a hosted Supabase project. The API calls Supabase over HTTPS through the Supabase client. It does not install SQL Server, PostgreSQL, or Npgsql.

One API process has one Supabase client. A second Supabase project is a second API, with its own `Supabase:Url` and `Supabase:SecretKey`.

This repository has no sample API and no test project.

Target framework is `net10.0`. Nullable and implicit usings are on. Root namespace is `Common`.

## Host startup

Each API's `Program.cs`:

```csharp
using Common.Context;
using Common.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddCommon(typeof(Program).Assembly);
builder.Services.AddSupabase(builder.Configuration);
builder.Services.AddCors(builder.Configuration);
builder.Services.AddPagination(builder.Configuration);
builder.Services.AddSwagger();

var app = builder.Build();
app.UseExceptionHandler();
app.UseSwagger();
app.UseCors("AllowDomains");
app.UseRequestLocalization();
app.UseMiddleware<RequestContextMiddleware>();
app.MapControllers();
app.Run();
```

Call `AddControllers` once. `AddCommon` already configures MVC options, JSON, cultures, and the exception handler. `UseExceptionHandler` has to be first so it wraps the rest of the pipeline. `app.UseCommon()` is that same call.

`AddSwagger` generates the OpenAPI document. `UseSwagger` maps `/openapi/v1.json` and Swagger UI at `/swagger` in Development only. The UI includes the `Language` and `CorrelationId` headers.

`AddCommon` registers MediatR handlers and FluentValidation validators from the API assembly. The validation pipeline runs before the handler and throws `ValidationException` when rules fail.

There is no authentication scheme and no `[Authorize]` on `ApiBaseController`.

## Configuration

The host owns these settings. This package ships no URL and no key.

```json
{
  "Supabase": {
    "Url": "https://YOUR_PROJECT.supabase.co",
    "SecretKey": "YOUR_SERVICE_ROLE_OR_SECRET_KEY"
  },
  "AllowDomains": "http://localhost:4200",
  "PaginationConfigs": {
    "DefaultPageIndex": 1,
    "DefaultPageSize": 15,
    "MaxPageSize": 100
  },
  "ExceptionHandling": {
    "IncludeDetail": true
  }
}
```

`Supabase:SecretKey` is the service-role key or the newer secret key. Keep it on the server. The frontend never receives it. That key bypasses Supabase row-level security, so the API has to enforce its own rules when auth is added later.

`AddSupabase` throws at startup when `Supabase:Url` or `Supabase:SecretKey` is missing. It builds `Supabase.Client` with token refresh and realtime off, registers that client as a singleton, and registers `IGenericRepository<>`.

`AllowDomains` is a comma-separated origin list for the `AllowDomains` CORS policy. The policy allows those origins, any header, any method, and credentials. An empty list allows no origin.

`AddPagination` binds `PaginationConfigs`. Missing or invalid values fall back to page `1`, size `15`, and max `100`. A max below the default page size is raised to that page size. Call `AddPagination` after `AddSupabase` so the host section overrides the defaults registered with the client.

## Response envelope

Every API response is one camelCase `TResponse<T>`:

- `id` (`Guid`)
- `utcTime` (`DateTime`, UTC round-trip `O`)
- `status` (a token, not the HTTP status number)
- `errorCode`
- `message`
- `isSuccess`
- `data`
- `errors` (`Dictionary<string, string[]>`, omitted when null)
- `debugDetail` (omitted when null)

Nulls are omitted. Dictionary keys are camelCase.

A finished handler returns `OkAsync` or `FailAsync`. It does not throw for that result. `OkAsync` sets `isSuccess` to true. `FailAsync` sets it to false and stores the status token in `status`, not in the HTTP status.

Business failures are HTTP 200 and `isSuccess: false`. Unexpected failures are HTTP 500 with the same JSON shape. The frontend has to read `isSuccess`. A 200 response is not proof of success.

`status` tokens and `errorCode` values:

- `ValidationException` and invalid model state: `InvalidArgument`, `BUSINESS:Validation`. Field messages stay in `errors`. `message` joins them with `"; "`.
- `BadRequestException`: `BadRequest`, `BUSINESS:BadRequest`
- `NotFoundException`: `NotFound`, `BUSINESS:NotFound`
- `UnauthorizedException`: `Unauthorized`, `BUSINESS:Unauthorized`
- `ForbiddenException`: `Forbidden`, `BUSINESS:Forbidden`
- `MethodNotAllowedException`: `MethodNotAllowed`, `BUSINESS:MethodNotAllowed`
- Anything else, including PostgREST and HTTP failures: `Error`, `UNKNOWN:Unhandled`

For a 500, `message` is the correlation id. `debugDetail` is set only when `ExceptionHandling:IncludeDetail` parses as true, and only for that 500. The text is `{ExceptionTypeName}: {Message}` truncated to 1024 characters. The error log also includes the localized `MessageConfirm.ErrorOccurred` string (`Đã có lỗi xảy ra` for `vi-VN`, `An error occurred` for `en-US`).

Invalid model state never enters the action. `[ApiController]`'s automatic 400 is suppressed, and the response is the envelope at HTTP 200.

Controllers inherit `ApiBaseController` and send MediatR requests with `SendAsync`. The request's `T` is the `TResponse<T>` the handler returns.

## Language and correlation

Header `Language`: `vi` and `vn` select `vi-VN`, `en` selects `en-US`, and a missing or any other value selects `vi-VN`. That culture is both the culture and the UI culture. The default request culture is `vi-VN`.

Header `CorrelationId` is kept when it is a GUID. Otherwise the value is `HttpContext.TraceIdentifier`. The same value is stored on `IRequestContextAccessor.Current` and used as the 500 `message`.

## Supabase data access

Entities extend `BaseEntity` (`id` uuid, primary key). Each API model adds `[Table("table_name")]`. Create the table in the hosted Supabase SQL editor. This repository has no SQL script and no EF or Dapper package.

`GetPagedAsync` and `FindPagedAsync` clamp the page to at least `DefaultPageIndex` and the page size to `1..MaxPageSize` (`DefaultPageSize` when the requested size is below 1). They return `PagingResponse<T>`: `totalRecords`, `currentPageNumber`, `pageSize`, `totalPages`, `hasNextPage`, `hasPreviousPage`, and `data`. The total comes from one PostgREST read with an exact count. `totalPages` is `0` when `pageSize` is `0`. `hasNextPage` is true when `currentPageNumber < totalPages`. `hasPreviousPage` is true when `currentPageNumber > 1`.

## Add later, as separate pieces

- Bearer or API-key auth, then `[Authorize]` on `ApiBaseController`. Until that exists, unauthorized and forbidden stay HTTP 200 with `isSuccess: false`.
- Another API project when a second Supabase project is needed. Do not register a second client in this package.
- Domain constants and validators in the API that needs them. Do not pre-create them here.

## Known limit

The service-role key skips row-level security. Until the API grows its own auth, any caller who can reach the API can do what the handlers allow.
