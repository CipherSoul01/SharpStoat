using System.Diagnostics.CodeAnalysis;

namespace Stoat.Api.Model;

public class StoatResponse : StoatResponse<object>
{
    protected StoatResponse(
        bool isSuccess,
        object? data,
        ProviderErrorKind errorKind,
        string? errorMessage,
        Exception? exception,
        IReadOnlyList<string>? diagnostics)
        : base(isSuccess, data, errorKind, errorMessage, exception, diagnostics)
    {
    }

    public new static StoatResponse Success(
        object? data = null,
        IReadOnlyList<string>? diagnostics = null) =>
        new(true, data, ProviderErrorKind.None, null, null, diagnostics);

    public new static StoatResponse NotFound(
        string message = "Recurso não encontrado.",
        IReadOnlyList<string>? diagnostics = null) =>
        new(false, null, ProviderErrorKind.NotFound, message, null, diagnostics);

    public new static StoatResponse NotSupported(
        string message = "Operação não suportada por este provedor.",
        IReadOnlyList<string>? diagnostics = null) =>
        new(false, null, ProviderErrorKind.NotSupported, message, null, diagnostics);

    public new static StoatResponse NetworkError(
        string message,
        Exception? exception = null,
        IReadOnlyList<string>? diagnostics = null) =>
        new(false, null, ProviderErrorKind.NetworkError, message, exception, diagnostics);

    public new static StoatResponse Failure(
        string message,
        Exception? exception = null,
        ProviderErrorKind errorKind = ProviderErrorKind.Other,
        IReadOnlyList<string>? diagnostics = null) =>
        new(false, null, errorKind, message, exception, diagnostics);

    public new static StoatResponse Cancelled(
        string message = "Operação cancelada.") =>
        new(false, null, ProviderErrorKind.Cancelled, message, null, null);

    public static StoatResponse<T> Success<T>(T data, IReadOnlyList<string>? diagnostics = null) =>
        new(true, data, ProviderErrorKind.None, null, null, diagnostics);

    public static StoatResponse<T> NotFound<T>(string message = "Recurso não encontrado.", IReadOnlyList<string>? diagnostics = null) =>
        new(false, default, ProviderErrorKind.NotFound, message, null, diagnostics);

    public static StoatResponse<T> NotSupported<T>(string message = "Operação não suportada por este provedor.", IReadOnlyList<string>? diagnostics = null) =>
        new(false, default, ProviderErrorKind.NotSupported, message, null, diagnostics);

    public static StoatResponse<T> NetworkError<T>(string message, Exception? exception = null, IReadOnlyList<string>? diagnostics = null) =>
        new(false, default, ProviderErrorKind.NetworkError, message, exception, diagnostics);

    public static StoatResponse<T> Failure<T>(string message, Exception? exception = null, ProviderErrorKind errorKind = ProviderErrorKind.Other, IReadOnlyList<string>? diagnostics = null) =>
        new(false, default, errorKind, message, exception, diagnostics);

    public static StoatResponse<T> Cancelled<T>(string message = "Operação cancelada.") =>
        new(false, default, ProviderErrorKind.Cancelled, message, null, null);
}

public class StoatResponse<T>
{
    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsSuccess { get; }
    
    public T? Data { get; set; }
    public ProviderErrorKind ErrorKind { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public IReadOnlyList<string> Diagnostics { get; }

    public StoatResponse( 
        bool isSuccess,
        T? data,
        ProviderErrorKind errorKind,
        string? errorMessage,
        Exception? exception,
        IReadOnlyList<string>? diagnostics)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorKind = errorKind;
        ErrorMessage = errorMessage;
        Exception = exception;
        Diagnostics = diagnostics ?? Array.Empty<string>();
    }

    public static StoatResponse<T> Success(T data, IReadOnlyList<string>? diagnostics = null) =>
        new(true, data, ProviderErrorKind.None, null, null, diagnostics);

    public static StoatResponse<T> NotFound(string message = "Recurso não encontrado.", IReadOnlyList<string>? diagnostics = null) =>
        new(false, default, ProviderErrorKind.NotFound, message, null, diagnostics);

    public static StoatResponse<T> NotSupported(string message = "Operação não suportada por este provedor.", IReadOnlyList<string>? diagnostics = null) =>
        new(false, default, ProviderErrorKind.NotSupported, message, null, diagnostics);

    public static StoatResponse<T> NetworkError(string message, Exception? exception = null, IReadOnlyList<string>? diagnostics = null) =>
        new(false, default, ProviderErrorKind.NetworkError, message, exception, diagnostics);

    public static StoatResponse<T> Failure(string message, Exception? exception = null, ProviderErrorKind errorKind = ProviderErrorKind.Other, IReadOnlyList<string>? diagnostics = null) =>
        new(false, default, errorKind, message, exception, diagnostics);

    public static StoatResponse<T> Cancelled(string message = "Operação cancelada.") =>
        new(false, default, ProviderErrorKind.Cancelled, message, null, null);

    public static implicit operator StoatResponse<T>(T data) => Success(data);
    
    public static implicit operator StoatResponse<T>(StoatResponse response)
    {
        if (!response.IsSuccess)
        {
            return new StoatResponse<T>(
                false,
                default,
                response.ErrorKind,
                response.ErrorMessage,
                response.Exception,
                response.Diagnostics);
        }

        if (response.Data is T value)
        {
            return new StoatResponse<T>(
                true,
                value,
                response.ErrorKind,
                response.ErrorMessage,
                response.Exception,
                response.Diagnostics);
        }

        return Failure(
            "Response data could not be converted to the expected type.",
            errorKind: ProviderErrorKind.InvalidResponse,
            diagnostics: response.Diagnostics
                .Append("ParseError")
                .ToArray());
    }
}

public enum ProviderErrorKind
{
    None = 0,
    NotSupported = 1,
    NotFound = 2,
    NetworkError = 3,
    InvalidResponse = 4,
    AuthenticationRequired = 5,
    Cancelled = 6,
    Other = 99
}