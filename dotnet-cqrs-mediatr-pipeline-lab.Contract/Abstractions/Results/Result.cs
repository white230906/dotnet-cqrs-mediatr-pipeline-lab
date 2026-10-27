namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;

public class Result
{

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyCollection<Error> Errors { get; }
    
    protected Result(bool isSuccess, Error[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (isSuccess && errors.Length > 0)
        {
            throw new InvalidOperationException("A successful result cannot contain errors.");
        }

        if (!isSuccess && errors.Length == 0)
        {
            throw new InvalidOperationException(
                "A failed result must contain at least one error.");
        }
        
        IsSuccess = isSuccess;
        Errors = Array.AsReadOnly(errors.ToArray());
    }

    public static Result Success()
        => new(true, Array.Empty<Error>());

    public static Result Failure(params Error[] errors)
        => new(false, errors);
}
