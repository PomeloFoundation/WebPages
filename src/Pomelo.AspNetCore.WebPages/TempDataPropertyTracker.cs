using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Pomelo.AspNetCore.WebPages
{
    public class TempDataPropertyTracker
    {
        private readonly object _subject;
        private readonly ITempDataDictionary _tempData;
        private readonly IDictionary<PropertyInfo, object> _trackedProperties = new Dictionary<PropertyInfo, object>();
        private readonly Action< ITempDataDictionary, PropertyInfo, object> _saveProperty;

        public TempDataPropertyTracker(object subject, ITempDataDictionary tempData, IDictionary<PropertyInfo, object> trackedProperties, Action<ITempDataDictionary, PropertyInfo, object> saveProperty)
        {
            _subject = subject;
            _tempData = tempData;
            _trackedProperties = trackedProperties;
            _saveProperty = saveProperty;
        }

        public void SaveChanges()
        {
            foreach (var property in _trackedProperties)
            {
                var newValue = property.Key.GetValue(_subject);
                if (newValue != null && newValue != property.Value)
                {
                    _saveProperty(_tempData, property.Key, newValue);
                }
            }
        }
    }
}