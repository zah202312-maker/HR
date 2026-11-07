using System;
using System.Linq;

namespace HR_Mangment2.Swagger;

public static class HrSwaggerNames
{
    /// <summary>Short schema names, so Swagger can be compared with the schemas in openapi.yaml.</summary>
    public static string SchemaId(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var name = type.Name[..type.Name.IndexOf('`')];
        return name + "Of" + string.Join("And", type.GetGenericArguments().Select(SchemaId));
    }
}
