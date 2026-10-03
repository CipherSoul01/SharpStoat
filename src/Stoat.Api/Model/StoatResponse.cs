namespace Stoat.Api.Model;

public class StoatResponse<T>
{
    public bool IsSuccessful { get; set; } = false;
    
    public T? Data { get; set; }

    public List<string> Messages { get; set; } = new List<string>();

    public StoatResponse(T data)
    {
        IsSuccessful = true;
        Data = data;
    }

    public StoatResponse(bool isSuccessful, T? data)
    {
        IsSuccessful = isSuccessful;
        Data = data;
    }

    public StoatResponse(List<string> messages)
    {
        IsSuccessful = false;
        Messages = messages;
    }
}