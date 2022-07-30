using AutoMapper;
using Sample.PaymentsClientApp.Simple.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder);

var app = builder.Build();
ConfigureApp(
    app,
    app.Services.GetRequiredService<IMapper>());

static void ConfigureServices(
    WebApplicationBuilder builder)
{
    var isDevelopment = builder.Environment.IsDevelopment();

    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddAutoMapper(typeof(Program));

    //TODO: add this in the Web API project, to add ability to prepare payments and send redirect UI back to a frontend
    builder.Services.AddPaymentPreparationsFeature(builder.Configuration);

    //TODO: add this in a "worker" project, which will listen to service bus messages and process the payment events (when payment completes or is cancelled)
    builder.Services.AddServiceBusBackgroundProcessor(builder.Configuration, isDevelopment);

    builder.Services.AddScoped<Sample.PaymentsClientApp.Simple.Services.PaymentsService>();
}

static void ConfigureApp(
    WebApplication app,
    IMapper mapper)
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    mapper.ConfigurationProvider.AssertConfigurationIsValid();

    app.MapControllers();
}

app.Run();