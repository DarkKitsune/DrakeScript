using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrakeScript
{
    public interface IIndexable
    {
        Value GetValue(object key);
        void SetValue(object key, Value value);
    }
}
