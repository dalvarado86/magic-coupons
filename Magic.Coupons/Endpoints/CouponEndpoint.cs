using AutoMapper;
using FluentValidation;
using Magic.Coupons.Models;
using Magic.Coupons.Models.DTO;
using Magic.Coupons.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Magic.Coupons.Endpoints
{
    public static class CouponEndpoint
    {
        public static void ConfigureCouponEndpoint(this WebApplication app)
        {
            // TODO: Adds Api Response builder
            // TODO: Removes duplicated code
            app.MapGet("/api/coupons", GetAllCouponsAsync)
                .WithName("GetCoupons")
                .Produces<IEnumerable<ApiResponse>>((int)HttpStatusCode.OK);

            app.MapGet("/api/coupons/{id:int}", GetCouponByIdAsync)
                .WithName("GetCoupon")
                .Produces<ApiResponse>((int)HttpStatusCode.OK)
                .Produces((int)HttpStatusCode.NotFound);

            app.MapPost("/api/coupons", CreateCouponAsync)
                .WithName("CreateCoupon")
                .Accepts<CouponCreateRequestDto>("application/json")
                .Produces<ApiResponse>((int)HttpStatusCode.Created)
                .Produces((int)HttpStatusCode.BadRequest);

            app.MapPut("/api/coupons", UpdateCouponAsync)
                .WithName("UpdateCoupon")
                .Accepts<CouponUpdateRequestDto>("application/json")
                .Produces((int)HttpStatusCode.OK)
                .Produces((int)HttpStatusCode.NotFound)
                .Produces((int)HttpStatusCode.BadRequest);

            app.MapDelete("/api/coupons/{id:int}", DeleteCouponAsync)
                .WithName("DeleteCoupon")
                .Produces((int)HttpStatusCode.NoContent)
                .Produces((int)HttpStatusCode.NotFound);
        }

        private async static Task<IResult> GetAllCouponsAsync(ICouponRepository couponRepository, ILogger<Program> logger)
        {
            var response = new ApiResponse();

            logger.Log(LogLevel.Information, "Looking for all coupons.");
            var coupons = await couponRepository.GetAllAsync();

            response.Result = coupons;
            response.IsSucess = true;
            response.StatusCode = HttpStatusCode.OK;

            logger.Log(LogLevel.Information, $"{coupons.Count} coupons were retrieved.");
            return Results.Ok(response);
        }

        private async static Task<IResult> GetCouponByIdAsync(
            ICouponRepository couponRepository, 
            ILogger<Program> logger, 
            int id)
        {
            var response = new ApiResponse() { IsSucess = false, StatusCode = HttpStatusCode.NotFound };

            logger.Log(LogLevel.Information, $"Looking for coupon with identifier '{id}'.");
            var coupon = await couponRepository.GetByIdAsync(id);

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
        }

        private async static Task<IResult> CreateCouponAsync(
            ICouponRepository couponRepository,
            ILogger<Program> logger,
            IMapper mapper,
            IValidator<CouponCreateRequestDto> validator,
            [FromBody] CouponCreateRequestDto request)
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
            if (await couponRepository.GetByNameAsync(request.Name) != null)
            {
                var message = $"The coupon with the name '{request.Name}' already exists";
                logger.Log(LogLevel.Error, $"Validation coupon failed: {message}");
                response.ErrorMessages.Add(message);
                return Results.BadRequest(response);
            }

            logger.Log(LogLevel.Information, "Creating new coupon.");
            var coupon = mapper.Map<Coupon>(request);
            coupon.Created = DateTime.Now;
            await couponRepository.CreateAsync(coupon);
            await couponRepository.SaveChangesAsync();
            logger.Log(LogLevel.Information, "Coupon has been created.");

            response.Result = mapper.Map<CouponResponseDto>(coupon);
            response.IsSucess = true;
            response.StatusCode = HttpStatusCode.Created;
            return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, response);
        }

        private async static Task<IResult> UpdateCouponAsync(
            ICouponRepository couponRepository,
            ILogger<Program> logger,
            IMapper mapper,
            IValidator<CouponUpdateRequestDto> validator,
            [FromBody] CouponUpdateRequestDto request)
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

            var currentCoupon = await couponRepository.GetByIdAsync(request.Id);

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
            currentCoupon.Name = request.Name;
            currentCoupon.Percent = request.Percent;
            currentCoupon.IsActive = request.IsActive;
            currentCoupon.LastUpdated = DateTime.Now;
            couponRepository.Update(currentCoupon);
            await couponRepository.SaveChangesAsync();
            logger.Log(LogLevel.Information, "Coupon has been updated.");

            response.Result = currentCoupon;
            response.IsSucess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }

        private async static Task<IResult> DeleteCouponAsync(
            ICouponRepository couponRepository, 
            ILogger<Program> logger,
            int id)
        {
            var response = new ApiResponse() { IsSucess = false, StatusCode = HttpStatusCode.NotFound };

            logger.Log(LogLevel.Information, $"Loooking for coupon with identifier '{id}'");
            var currentCoupon = await couponRepository.GetByIdAsync(id);

            if (currentCoupon == null)
            {
                var message = $"The coupon with identifier '{id}' does not exist";
                logger.Log(LogLevel.Error, $"Validation coupon failed: {message}");
                response.ErrorMessages.Add(message);
                return Results.NotFound(response);
            }

            logger.Log(LogLevel.Information, $"Deleting coupon with identifier '{id}'");
            couponRepository.Remove(currentCoupon);
            await couponRepository.SaveChangesAsync();
            logger.Log(LogLevel.Information, $"Coupon with identifier '{id}' has been deleted.");

            response.IsSucess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.NoContent();
        }
    }
}
