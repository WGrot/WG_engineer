namespace RestaurantApp.Api.Functional;

public readonly struct Either<L, R>
{
    private readonly L _left;
    private readonly R _right;

    public bool IsLeft { get; }
    public bool IsRight => !IsLeft;

    private Either(L left)
    {
        IsLeft = true;
        _left = left;
        _right = default!;
    }

    private Either(R right)
    {
        IsLeft = false;
        _right = right;
        _left = default!;
    }

    public static Either<L, R> Left(L value) => new(value);
    public static Either<L, R> Right(R value) => new(value);

    // Pattern matching
    public T Match<T>(Func<L, T> left, Func<R, T> right) =>
        IsLeft ? left(_left) : right(_right);

    // Monad helpers
    public Either<L, R2> Map<R2>(Func<R, R2> mapper) =>
        IsRight ? Either<L, R2>.Right(mapper(_right)) : Either<L, R2>.Left(_left);

    public Either<L, R2> Bind<R2>(Func<R, Either<L, R2>> binder) =>
        IsRight ? binder(_right) : Either<L, R2>.Left(_left);

    public override string ToString() =>
        IsLeft ? $"Left({_left})" : $"Right({_right})";
}