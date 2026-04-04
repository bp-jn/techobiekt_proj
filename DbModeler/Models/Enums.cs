using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModeler.Models
{
    public enum SqlDataType
    {
        Int,
        Varchar,
        Decimal,
        DateTime,
        Bool
    }
    public enum RelationshipType
    {
        OneToOne,
        OneToMany,
        ManyToMany
    }
}
