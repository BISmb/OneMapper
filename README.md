# OneMapper
A very simple object mapper

This library is for developers who just need a barebones mapping solution. Very simple, just declare the types and the mapping translation function. Additional objects can optionally be provided to Mapper.To() function, which will be available to use within the mapping function.

## Install Nuget

Package: 

### 1. Create map function

```csharp
protected override SchemaColumn MapFromRecordToDomainType(SchemaColumnRecord record, params object[] additionalObjects)
    {
        return new SchemaColumn
        {
            Id = record.Id,
            CreatedOn = record.CreatedOn,
            ModifiedOn = record.ModifiedOn,
            SchemaModelId = record.SchemaModelId,
            Alias = record.Alias,
            Name = record.Name,
            Type = record.Type,
            Version = record.Version,
        };
    }
```

### 2. Add mapper

```csharp
Mapper.AddMapperDelegate<TDomainType, TRecordType>(MapFromRecordToDomainType);
```

Mapper is a static class.

### 3. Use Mapper

```csharp
var domainObjects = Mapper.To<SchemaColumn, SchemaColumnRecord>(recordObjects);
```

The mapper works by providing it a Func delegate that will be called to do the mapping between objects.


