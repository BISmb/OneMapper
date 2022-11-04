namespace OneMapper;

public static class Mapper
{
    public static bool ThrowExceptionOnDuplicateMapping;

    private readonly static Dictionary<(Type, Type), Delegate> _mappersDelegate;
    private readonly static Dictionary<(Type, Type), Func<object, object[], object>> _mappers;

    static Mapper()
    {
        _mappersDelegate = new Dictionary<(Type, Type), Delegate>();
        _mappers = new Dictionary<(Type, Type), Func<object, object[], object>>();

        ThrowExceptionOnDuplicateMapping = false;
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
            if (ThrowExceptionOnDuplicateMapping)
            {
                throw new ArgumentException($"A mapper already exists between the types: {typeof(F).Name} and {typeof(T).Name}");
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

        AddMapperDelegate<T, F>(inputMapFunc);
    }

    public static T To<T, F>(F fromObject, params object[] additionalItems)
        where T : class
        where F : class
    {
        // use object type mappers
        //var foundMapper = _mappers[(typeof(T), typeof(F))];
        //var convertedObject = foundMapper(fromObject, additionalItems);

        // use strongly typed mappers (delegates)
        var foundMapper = _mappersDelegate[(typeof(T), typeof(F))];
        var convertedObject = foundMapper.DynamicInvoke(fromObject, additionalItems);

        if (convertedObject is not T expectedObjectType)
        {
            throw new Exception($"Converted object is not of expected type: {typeof(T).Name}");
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

    //public static void AddMapper((Type, Type) key, Func<object, object[], object> mappingFunc)
    //{
    //    _mappers.Add(key, mappingFunc);
    //}

    //public static void AddMapper<T, F>(Func<object, object[], object> mappingFunc)
    //{
    //    _mappers.Add((typeof(T), typeof(F)), mappingFunc);
    //}

    //public static void AddMapper<T, F>(Func<object, object> mappingFunc)
    //    where T: class
    //    where F: class
    //{
    //    Func<object, object[], object> inputMapFunc = new Func<object, object[], object>((obj, additionalArgs) =>
    //    {
    //        return mappingFunc(obj);
    //    });

    //    _mappers.Add((typeof(T), typeof(F)), inputMapFunc);
    //}
}