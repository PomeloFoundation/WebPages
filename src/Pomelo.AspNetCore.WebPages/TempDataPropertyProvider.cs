using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Pomelo.AspNetCore.WebPages
{
    public class TempDataPropertyProvider
    {
        private static readonly string _prefix = "TempDataProperty-";
        private ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _subjectProperties =
            new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        public TempDataPropertyTracker LoadAndTrackChanges(object subject, ITempDataDictionary tempData)
        {
            return new TempDataPropertyTracker(subject, tempData, LoadPropertyState(subject, tempData), SavePropertyValue);
        }

        private IDictionary<PropertyInfo, object> LoadPropertyState(object subject, ITempDataDictionary tempData)
        {
            var properties = GetSubjectProperties(subject);
            var result = new Dictionary<PropertyInfo, object>();

            foreach (var property in properties)
            {
                var value = tempData[_prefix + property.Name];

                result[property] = value;

                // TODO: Clarify what behavior should be for null values here
                if (value != null && property.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    property.SetValue(subject, value);
                }
            }

            return result;
        }

        private void SavePropertyValue(ITempDataDictionary tempData, PropertyInfo property, object value)
        {
            if (value != null)
            {
                tempData[_prefix + property.Name] = value;
            }
        }

        private IEnumerable<PropertyInfo> GetSubjectProperties(object subject)
        {
            return _subjectProperties.GetOrAdd(subject.GetType(), subjectType =>
            {
                var properties = subjectType.GetRuntimeProperties()
                    .Where(pi => pi.GetCustomAttribute<TempDataAttribute>() != null);

                if (properties.Any(pi => !(pi.SetMethod != null && pi.SetMethod.IsPublic && pi.GetMethod != null && pi.GetMethod.IsPublic)))
                {
                    throw new InvalidOperationException("TempData properties must have a public getter and setter.");
                }

                if (properties.Any(pi => !(pi.PropertyType.GetTypeInfo().IsPrimitive || pi.PropertyType == typeof(string))))
                {
                    throw new InvalidOperationException("TempData properties must be declared as primitive types or string only.");
                }

                return properties;
            });
        }
    }
}