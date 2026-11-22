using Permutations.Api;
using Permutations.Api.Middleware;
using Permutations.Application;
using Permutations.Application.Options;
using Permutations.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<PermutationLimits>(builder.Configuration.GetSection(PermutationLimits.Section));
builder.Services.AddApplication().AddInMemorySessionStore(builder.Configuration);
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader().AllowAnyMethod().WithExposedHeaders(SessionIdHeader.Name)));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseCors();
app.MapControllers();
app.Run();

public partial class Program { } // for WebApplicationFactory
