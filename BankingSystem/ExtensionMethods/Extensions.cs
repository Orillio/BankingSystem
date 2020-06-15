using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    public static class Extensions
    {
        public static void Update(this IDataBase db) =>
            db.Adapter.Update(db.Table);
    }
}
