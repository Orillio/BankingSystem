//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BankingSystem
{
    using PropertiesChangedLib;
    using System;
    using System.Collections.Generic;
    
    public partial class Investment : PropertiesChanged
    {
        public int Id { get; set; }
        public int clientId { get; set; }
        public string investmentType { get; set; }
        public string investmentDate { get; set; }
        public int investmentSum { get; set; }
        public int percentage { get; set; }
    }
}