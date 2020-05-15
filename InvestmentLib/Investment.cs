using PropertiesChangedLib;
using System;
using System.IO.Compression;
using EnumsLib;
using static EnumsLib.Enums;

namespace InvestmentLib
{
    public class Investment : PropertiesChanged
    {
        #region Private fields
        private TimeSpan ts; // разница во времени между  датой вклада и сегодняшней
        private DateTime currentdate; //  дата выбранная в календаре
        private int percent;

        #endregion

        #region Properties

        public int Percentage { get => percent; } // ставка по вкладу
        public Enums.ClientType ClientType { get; set; } 
        public Enums.InvestmentType Type { get; set; }
        public long InvestmentSum { get; set; } // сумма инвестиции
        public DateTime InvestmentDate { get; set; } // дата инвестиции
        public long CurrentSum { get; set; } // сумма накопившаяся за период вклада
        public DateTime CurrentDate
        {
            get => currentdate;
            set
            {
                if (value == default) return;

                currentdate = value; // передаем значение из календаря в currentdate
                ts = currentdate - InvestmentDate; // считаем количество дней прошедших после депозита
                if (ts.Days < 0) throw new Exception($"Вклада еще несуществовало. Дата вклада: {InvestmentDate.ToShortDateString()}"); // если дней меньше нуля, то вклада еще не существовало
                switch (Type)
                {
                    case Enums.InvestmentType.Capitalization: // в случае, если тип инвестиции - с капитализацией
                        var months = ts.Days / 30; // считаем количество месяцев прошедших после депозита
                        CurrentSum = (long)(InvestmentSum + (InvestmentSum / 100.0 * Percentage / 12.0 * months));
                        break;
                    case Enums.InvestmentType.NotCapitalization:// в случае, если тип инвестиции - без капитализации
                        var years = ts.Days / 365; // считаем количество лет прошедших после депозита
                        CurrentSum = (long)(InvestmentSum + (InvestmentSum / 100.0 * Percentage * years));
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        public Investment(InvestmentType type, ClientType clType, long sum, DateTime date)
        {
            percent = clType == ClientType.VIP ? 15 : clType == ClientType.Juridical ? 9 : 11; 
            // определение ставки по типу клиента VIP - 15%; Физическое лицо - 11%; Юридическое - 9%
            this.InvestmentSum = sum;
            this.Type = type;
            InvestmentDate = date;
        }
    }
}
