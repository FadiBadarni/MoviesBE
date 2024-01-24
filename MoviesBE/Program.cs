using MoviesBE.Services;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);
var neo4JConfig = builder.Configuration.GetSection("Neo4j");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<HttpService>();
builder.Services.AddScoped<TmdbService>();
builder.Services.AddSingleton<Neo4JService>();
builder.Services.AddSingleton(GraphDatabase.Driver(neo4JConfig["Uri"],
    AuthTokens.Basic(neo4JConfig["Username"], neo4JConfig["Password"])));

// Configure logging
builder.Logging.AddConsole();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        corsBuilder => corsBuilder.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/error");

app.UseCors("AllowSpecificOrigin");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();