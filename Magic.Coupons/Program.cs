using AutoMapper;
using FluentValidation;
using Magic.Coupons.Data;
using Magic.Coupons.Models;
using Magic.Coupons.Models.DTO;
using Magic.Coupons.Profiles;
using Microsoft.AspNetCore.Mvc;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(CouponProfile));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// TODO: Adds Api Response builder
// TODO: Removes duplicated code

app.MapGet("/api/coupons", (ILogger<Program> logger) =>
{
    var response = new ApiResponse();

    logger.Log(LogLevel.Information, "Looking for all coupons.");
    var coupons = CouponStore.Coupons;

    response.Result = coupons;
    response.IsSucess = true;
    response.StatusCode = HttpStatusCode.OK;

    logger.Log(LogLevel.Information, $"{coupons.Count} coupons were retrieved.");
    return Results.Ok(response);
})
    .WithName("GetCoupons")
    .Produces<IEnumerable<ApiResponse>>((int)HttpStatusCode.OK);

app.MapGet("/api/coupons/{id:int}", (
    ILogger<Program> logger, 
    int id) =>
{
    var response = new ApiResponse() { IsSucess = false, StatusCode = HttpStatusCode.NotFound };

    logger.Log(LogLevel.Information, $"Looking for coupon with identifier '{id}'.");
    var coupon = CouponStore.Coupons.FirstOrDefault(x => x.Id == id);

    if (coupon == null)
    {
        var message = $"Coupon with identifier '{id}' not found.";
        logger.Log(LogLevel.Error, message);
        response.ErrorMessages.Add(message);
        return Results.NotFound(response);
    }

    logger.Log(LogLevel.Information, $"Coupon with identifier '{id}' has been retrieved.");

    response.Result = coupon;
    response.IsSucess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
})
    .WithName("GetCoupon")
    .Produces<ApiResponse>((int)HttpStatusCode.OK)
    .Produces((int)HttpStatusCode.NotFound);

app.MapPost("/api/coupons", async (
    ILogger <Program> logger,
    IMapper mapper,
    IValidator<CouponCreateRequestDto> validator,
    [FromBody] CouponCreateRequestDto request) => 
{
    var response = new ApiResponse() { IsSucess = false, StatusCode = HttpStatusCode.BadRequest };

    logger.Log(LogLevel.Information, "Validating coupon data.");
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        // TODO: Retrieves all the errors list.
        logger.Log(LogLevel.Error, "Validation coupon failed.");
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }

    // TODO: Puts in fluent validation
    if (CouponStore.Coupons.FirstOrDefault(x => x.Name.ToLower() == request.Name.ToLower()) != null)
    {
        var message = $"The '{nameof(request.Name)}' coupon already exists";
        logger.Log(LogLevel.Error, $"Validation coupon failed: {message}");
        response.ErrorMessages.Add(message);
        return Results.BadRequest(response);
    }

    logger.Log(LogLevel.Information, "Creating new coupon.");
    var coupon = mapper.Map<Coupon>(request);
    coupon.Id = CouponStore.Coupons.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
    coupon.Created = DateTime.Now;
    CouponStore.Coupons.Add(coupon);
    logger.Log(LogLevel.Information, "Coupon has been created.");

    response.Result = mapper.Map<CouponResponseDto>(coupon);
    response.IsSucess = true;
    response.StatusCode = HttpStatusCode.Created;
    return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, response);
})
    .WithName("CreateCoupon")
    .Accepts<CouponCreateRequestDto>("application/json")
    .Produces<ApiResponse>((int)HttpStatusCode.Created)
    .Produces((int)HttpStatusCode.BadRequest);

app.MapPut("/api/coupons", async (
    ILogger<Program> logger,
    IMapper mapper,
    IValidator<CouponUpdateRequestDto> validator,
    [FromBody] CouponUpdateRequestDto request) =>
{
    var response = new ApiResponse() { IsSucess = false, StatusCode = HttpStatusCode.BadRequest };

    logger.Log(LogLevel.Information, "Validating coupon data.");
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        // TODO: Retrieves all the errors list.
        logger.Log(LogLevel.Error, "Validation coupon failed.");
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }

    var currentCoupon = CouponStore.Coupons.FirstOrDefault(x => x.Id == request.Id);
    
    // TODO: Validates if the coupon name is already taken by anothore item.
    if (currentCoupon == null)
    {
        var message = $"Coupon with identifier '{request.Id}' not found.";
        logger.Log(LogLevel.Error, message);
        response.StatusCode = HttpStatusCode.NotFound;
        response.ErrorMessages.Add(message);
        return Results.NotFound(response);
    }

    logger.Log(LogLevel.Information, "Updating coupon.");
    currentCoupon.IsActive = request.IsActive;
    currentCoupon.Name = request.Name;
    currentCoupon.Percent = request.Percent;
    currentCoupon.LastUpdated = DateTime.Now;
    logger.Log(LogLevel.Information, "Coupon has been updated.");

    response.Result = mapper.Map<Coupon>(currentCoupon);
    response.IsSucess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
})
    .WithName("UpdateCoupon")
    .Accepts<CouponUpdateRequestDto>("application/json")
    .Produces((int)HttpStatusCode.OK)
    .Produces((int)HttpStatusCode.NotFound)
    .Produces((int)HttpStatusCode.BadRequest);

app.MapDelete("/api/coupons/{id:int}", (
    ILogger<Program> logger,
    int id) =>
{
    var response = new ApiResponse() { IsSucess = false, StatusCode = HttpStatusCode.NotFound };

    logger.Log(LogLevel.Information, $"Loooking for coupon with identifier '{id}'");
    var currentCoupon = CouponStore.Coupons.FirstOrDefault(x => x.Id == id);

    if (currentCoupon == null)
    {
        var message = $"The coupon with identifier '{id}' does not exist";
        logger.Log(LogLevel.Error, $"Validation coupon failed: {message}");
        response.ErrorMessages.Add(message);
        return Results.NotFound(response);
    }

    logger.Log(LogLevel.Information, $"Deleting coupon with identifier '{id}'");
    CouponStore.Coupons.Remove(currentCoupon);
    logger.Log(LogLevel.Information, $"Coupon with identifier '{id}' has been deleted.");

    response.IsSucess = true;
    response.StatusCode = HttpStatusCode.NoContent;
    return Results.NoContent();

})
    .WithName("DeleteCoupon")
    .Produces((int)HttpStatusCode.NoContent)
    .Produces((int)HttpStatusCode.NotFound);

app.UseHttpsRedirection();

app.Run();