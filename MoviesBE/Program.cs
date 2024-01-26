using MoviesBE.Middleware;
using MoviesBE.Repositories;
using MoviesBE.Services;
using MoviesBE.Services.Database;
using MoviesBE.Services.TMDB;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);
var neo4JConfig = builder.Configuration.GetSection("Neo4j");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<HttpService>();
builder.Services.AddScoped<TmdbService>();
builder.Services.AddScoped<TmdbApiService>();
builder.Services.AddScoped<MovieBackdropService>();
builder.Services.AddScoped<VideoService>();
builder.Services.AddScoped<CreditsService>();
builder.Services.AddScoped<PopularityThresholdService>();
builder.Services.AddScoped<RatingThresholdService>();
builder.Services.AddSingleton<Neo4JService>();
builder.Services.AddSingleton(GraphDatabase.Driver(neo4JConfig["Uri"],
    AuthTokens.Basic(neo4JConfig["Username"], neo4JConfig["Password"])));
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<ICreditsRepository, CreditsRepository>();


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

app.UseCors("AllowSpecificOrigin");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();