using System;
using System.Collections.Generic;

namespace Lazaro.Orm.Attributes
{
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
        public class Column : System.Attribute
        {
                public bool Id { get; set; }
                public ColumnTypes Type { get; set; }
                public string Name { get; set; }
                public int Length { get; set; }
                public int Precision { get; set; }

                public bool Nullable { get; set; }
                public bool Unique { get; set; }
        }
}
