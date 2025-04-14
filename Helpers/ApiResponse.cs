using Microsoft.AspNetCore.Mvc;
using BlogAPI.DTO;

namespace BlogAPI.Helper;

public static class ApiResponse
{
    public static ActionResult Success<T>(PaginatedResult<T> paginatedResult)
    {
        return new JsonResult(new
        {
            status = "success",
            message = "Data retrieved successfully",
            data = paginatedResult.Items,
            pagination = new
            {
                page = paginatedResult.Page,
                limit = paginatedResult.Limit,
                totalItems = paginatedResult.TotalItems,
                totalPages = paginatedResult.TotalPages
            },
            timestamp = DateTime.UtcNow.ToString("o")
        });
    }
    public static ActionResult Success(object data = null, string title = null)
    {
        if (title == "insert")
        {
            return new JsonResult(new
            {
                status = "success",
                message = "Data has been successfully created.",
                data,
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }
        if (title == "update")
        {
            return new JsonResult(new
            {
                status = "success",
                message = "Data has been successfully updated.",
                data,
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }
        if (title == "delete")
        {
            return new JsonResult(new
            {
                status = "success",
                message = "Data has been successfully deleted.",
            });
        }
        else
        {
            return new JsonResult(new
            {
                status = "success",
                message = "Data has been retrieved successfully.",
                data,
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }
        
    }

    public static IEnumerable<ErrorDetail> Errors(params (string field, string message)[] items)
    {
        return items.Select(i => new ErrorDetail(i.field, i.message));
    }

    public static ActionResult Error(string message, IEnumerable<object> errors = null, int statusCode = 400)
    {
        return new JsonResult(new
        {
            status = "error",
            message,
            errors,
            timestamp = DateTime.UtcNow.ToString("o")
        })
        { StatusCode = statusCode };
    }

   public static ActionResult NotFound(string message = "Resource not found.", string field = "", string value = "")
    {
        var errors = new List<object>();

        if (!string.IsNullOrWhiteSpace(field))
        {
            errors.Add(new { field = field, message = $"The specified {value} does not exist." });
        }

        return Error(message, errors.ToArray(), 404);
    }

    public static ActionResult BadRequest(string message = "Bad request", List<ValidationError> errors = null)
        {
            return Error(message, errors ?? new List<ValidationError>(), 400);
        }

        public static ActionResult Unauthorized(string message = "Unauthorized")
        {
            return Error(message, null, 401);
        }

        public static ActionResult Forbidden(string message = "Forbidden")
        {
            return Error(message, null, 403);
        }

        public static ActionResult InternalServerError(string message = "Internal server error")
        {
            return Error(message, null, 500);
        }

}