using Microsoft.Extensions.Logging;

namespace OneMapper;

public class FoundMultipleMappingException : Exception
{
    public FoundMultipleMappingException(Type typeFrom, Type typeTo)
        : base($"Multiple mapping delegates found for {typeFrom.Name} to {typeTo.Name}")
    { }
}

public class CannotAddMultipleMappingException : Exception
{
    public CannotAddMultipleMappingException(Type typeFrom, Type typeTo)
        : base($"A mapper already exists between the types: {typeFrom.Name} and {typeTo.Name}")
    { }
}

public class ConvertedObjectNotExpectedTypeException : Exception
{
    public ConvertedObjectNotExpectedTypeException(Type typeTo)
        : base($"Converted object is not of expected type: {typeTo.Name}")
    { }
}

public static class Mapper
{
    private static ILogger? _logger;
    private static bool _ThrowExceptionOnDuplicateMapping;

    private readonly static Dictionary<(Type, Type), Delegate> _mappersDelegate;

    static Mapper()
    {
        _mappersDelegate = new Dictionary<(Type, Type), Delegate>();
    }

    public static void AddLogger(ILogger logger)
    {
        _logger = logger;
    }

    public static void ThrowExceptionOnDeplicateMapping()
    {
        _ThrowExceptionOnDuplicateMapping = true;
    }

    public static void RemoveMapper((Type, Type) key)
    {
        _mappersDelegate.Remove(key);
    }

    public static void AddMapperDelegate<T, F>(Func<F, object[], T> mappingFunc)
        where T : class
        where F : class
    {
        if (_mappersDelegate.ContainsKey((typeof(T), typeof(F))))
        {
            if (_ThrowExceptionOnDuplicateMapping)
            {
                throw new CannotAddMultipleMappingException(typeof(F), typeof(T));
            }
            else
            {
                return;
            }
        }

        _mappersDelegate.Add((typeof(T), typeof(F)), mappingFunc);
    }

    public static void AddMapperDelegate<T, F>(Func<F, T> mappingFunc)
        where T : class
        where F : class
    {
        Func<F, object[], T> inputMapFunc = new Func<F, object[], T>((obj, additionalArgs) =>
        {
            return mappingFunc(obj);
        });

        AddMapperDelegate(inputMapFunc);
    }

    public static T To<T, F>(F fromObject, params object[] additionalItems)
        where T : class
        where F : class
    {
        // use strongly typed mappers (delegates)
        var foundMapper = _mappersDelegate[(typeof(T), typeof(F))];
        var convertedObject = foundMapper.DynamicInvoke(fromObject, additionalItems);

        if (convertedObject is not T expectedObjectType)
        {
            throw new ConvertedObjectNotExpectedTypeException(typeof(T));
        }

        return expectedObjectType;
    }

    public static IEnumerable<T> To<T, F>(IEnumerable<F> fromObjects)
        where T : class
        where F : class
    {
        foreach (var fromObject in fromObjects)
        {
            yield return To<T, F>(fromObject, new object[0]);
        }
    }
}