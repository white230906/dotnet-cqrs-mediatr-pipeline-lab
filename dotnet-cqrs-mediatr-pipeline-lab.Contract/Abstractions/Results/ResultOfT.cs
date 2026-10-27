namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;

public class Result<T>: Result
{
    private readonly T? _value;
    
    protected Result(T?  value, bool isSuccess, Error[] errors) : base(isSuccess, errors)
    {
        _value = value;
    }
    
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException( "The value of a failed result cannot be accessed.");
    
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T>(value, true, Array.Empty<Error>());
    }

    public new static Result<T> Failure(params Error[] errors)
        => new(default, false, errors);
}
