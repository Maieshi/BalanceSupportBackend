using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Balance_Support.DataClasses.DatabaseEntities;
using Newtonsoft.Json;

public static class DtoConvertableExtensions
{
    // Convert a list of entities to a list of objects
    public static List<object> ConvertToDtoList<T>(this IEnumerable<T> entities) 
        where T : IDtoConvertable
    {
        return entities.Select(entity => entity.Convert()).ToList();
    }

    // Convert a dictionary of lists of entities to a dictionary of lists of objects, preserving keys
    public static Dictionary<TKey, List<object>> ConvertToDtoList<TKey, T>(this Dictionary<TKey, List<T>> entityDictionary)
        where T : IDtoConvertable
    {
        return entityDictionary.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Select(entity => entity.Convert()).ToList()
        );
    }

    // Convert a dictionary of single entities to a dictionary of objects, preserving keys
    public static Dictionary<TKey, object> ConvertToDto<TKey, T>(this Dictionary<TKey, T> entityDictionary)
        where T : IDtoConvertable
    {
        return entityDictionary.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Convert()
        );
    }

    // Convert a dictionary of lists of entities to a list of objects, ignoring keys
    public static List<object> ConvertToDtoIgnoreKeys<TKey, T>(this Dictionary<TKey, List<T>> entityDictionary)
        where T : IDtoConvertable
    {
        return entityDictionary.Values
            .SelectMany(entityList => entityList.Select(entity => entity.Convert()))
            .ToList();
    }

    // Convert a dictionary of single entities to a list of objects, ignoring keys
    public static List<object> ConvertToDtoIgnoreKeys<TKey, T>(this Dictionary<TKey, T> entityDictionary)
        where T : IDtoConvertable
    {
        return entityDictionary.Values
            .Select(entity => entity.Convert())
            .ToList();
    }
    public static List<object> ConvertToDtoList<T>(this IQueryable<T> entities)
        where T : IDtoConvertable
    {
        return entities.Select(entity => entity.Convert()).ToList();
    }
}
