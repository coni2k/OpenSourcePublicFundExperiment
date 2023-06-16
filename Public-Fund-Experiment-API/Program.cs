using PublicFundExperimentAPI.Services;
using Octokit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<EcosystemsClient>();

builder.Services.AddSingleton(factory => {
    var settings = builder.Configuration.GetSection("GitHub");
    var token = settings.GetValue<string>("token");
    var credentials = new Credentials(token);
    var client = new GitHubClient(new ProductHeaderValue("OpenSourcePublicFundExperiment"))
    {
        Credentials = credentials
    };
    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
