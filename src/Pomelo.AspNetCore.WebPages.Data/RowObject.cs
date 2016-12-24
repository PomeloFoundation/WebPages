using System.Collections.Generic;
using System.Dynamic;

namespace Pomelo.AspNetCore.WebPages.Data
{
    public class DynamicRow : DynamicObject
    {
        protected virtual IDictionary<string, object> _dictionary { get; set; } = new Dictionary<string, object>();

        public virtual void SetMember(string Name, object Value)
        {
            if (!_dictionary.ContainsKey(Name.ToLower()))
                _dictionary.Add(Name.ToLower(), Value);
            else
                _dictionary[Name.ToLower()] = Value;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!_dictionary.ContainsKey(binder.Name.ToLower()))
                _dictionary.Add(binder.Name.ToLower(), value);
            else
                _dictionary[binder.Name.ToLower()] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_dictionary.ContainsKey(binder.Name.ToLower()))
            {
                result = _dictionary[binder.Name.ToLower()];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}
