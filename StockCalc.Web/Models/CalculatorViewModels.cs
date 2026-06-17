using System.ComponentModel.DataAnnotations;

namespace StockCalc.Web.Models;

public abstract class CalculatorViewModel
{
    protected static decimal Percent(decimal value) => value / 100m;
}

public class ProfitCalculatorViewModel : CalculatorViewModel
{
    [Display(Name = "매수가")]
    [Range(0, double.MaxValue)]
    public decimal BuyPrice { get; set; }

    [Display(Name = "매도가")]
    [Range(0, double.MaxValue)]
    public decimal SellPrice { get; set; }

    [Display(Name = "수량")]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Display(Name = "수수료율(%)")]
    [Range(0, double.MaxValue)]
    public decimal FeeRate { get; set; }

    [Display(Name = "세금률(%)")]
    [Range(0, double.MaxValue)]
    public decimal TaxRate { get; set; }

    public decimal BuyTotal => BuyPrice * Quantity;
    public decimal SellTotal => SellPrice * Quantity;
    public decimal Fee => (BuyTotal + SellTotal) * Percent(FeeRate);
    public decimal Tax => SellTotal * Percent(TaxRate);
    public decimal NetProfit => SellTotal - BuyTotal - Fee - Tax;
    public decimal ReturnRate => BuyTotal == 0 ? 0 : NetProfit / BuyTotal * 100m;
}

public class AveragePriceCalculatorViewModel
{
    [Display(Name = "기존 매수가")]
    [Range(0, double.MaxValue)]
    public decimal ExistingPrice { get; set; }

    [Display(Name = "기존 수량")]
    [Range(0, int.MaxValue)]
    public int ExistingQuantity { get; set; }

    [Display(Name = "추가 매수가")]
    [Range(0, double.MaxValue)]
    public decimal AdditionalPrice { get; set; }

    [Display(Name = "추가 수량")]
    [Range(0, int.MaxValue)]
    public int AdditionalQuantity { get; set; }

    public int TotalQuantity => ExistingQuantity + AdditionalQuantity;
    public decimal TotalAmount => ExistingPrice * ExistingQuantity + AdditionalPrice * AdditionalQuantity;
    public decimal NewAveragePrice => TotalQuantity == 0 ? 0 : TotalAmount / TotalQuantity;
}

public class WaterAverageCalculatorViewModel
{
    [Display(Name = "현재 평균단가")]
    [Range(0, double.MaxValue)]
    public decimal CurrentAveragePrice { get; set; }

    [Display(Name = "현재 보유수량")]
    [Range(0, int.MaxValue)]
    public int CurrentQuantity { get; set; }

    [Display(Name = "현재가")]
    [Range(0, double.MaxValue)]
    public decimal CurrentPrice { get; set; }

    [Display(Name = "추가매수금액")]
    [Range(0, double.MaxValue)]
    public decimal AdditionalAmount { get; set; }

    public int AdditionalQuantity => CurrentPrice == 0 ? 0 : (int)Math.Floor(AdditionalAmount / CurrentPrice);
    public int TotalQuantity => CurrentQuantity + AdditionalQuantity;
    public decimal NewAveragePrice => TotalQuantity == 0 ? 0 : (CurrentAveragePrice * CurrentQuantity + CurrentPrice * AdditionalQuantity) / TotalQuantity;
}

public class StopLossCalculatorViewModel : CalculatorViewModel
{
    [Display(Name = "매수가")]
    [Range(0, double.MaxValue)]
    public decimal BuyPrice { get; set; }

    [Display(Name = "손실 허용률(%)")]
    [Range(0, 100)]
    public decimal LossRate { get; set; }

    public decimal StopPrice => BuyPrice * (1 - Percent(LossRate));
    public decimal LossPerShare => BuyPrice - StopPrice;
}

public class TargetPriceCalculatorViewModel : CalculatorViewModel
{
    [Display(Name = "매수가")]
    [Range(0, double.MaxValue)]
    public decimal BuyPrice { get; set; }

    [Display(Name = "목표 수익률(%)")]
    [Range(0, double.MaxValue)]
    public decimal TargetRate { get; set; }

    public decimal TargetPrice => BuyPrice * (1 + Percent(TargetRate));
    public decimal ProfitPerShare => TargetPrice - BuyPrice;
}

public class DividendCalculatorViewModel : CalculatorViewModel
{
    [Display(Name = "보유수량")]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Display(Name = "1주당 배당금")]
    [Range(0, double.MaxValue)]
    public decimal DividendPerShare { get; set; }

    [Display(Name = "배당소득세율(%)")]
    [Range(0, double.MaxValue)]
    public decimal TaxRate { get; set; }

    public decimal GrossDividend => Quantity * DividendPerShare;
    public decimal Tax => GrossDividend * Percent(TaxRate);
    public decimal NetDividend => GrossDividend - Tax;
}

public class CompoundCalculatorViewModel : CalculatorViewModel
{
    [Display(Name = "초기 투자금")]
    [Range(0, double.MaxValue)]
    public decimal InitialInvestment { get; set; }

    [Display(Name = "월 추가 투자금")]
    [Range(0, double.MaxValue)]
    public decimal MonthlyInvestment { get; set; }

    [Display(Name = "연 수익률(%)")]
    public decimal AnnualReturnRate { get; set; }

    [Display(Name = "투자 기간(년)")]
    [Range(0, 100)]
    public int Years { get; set; }

    public decimal FutureValue
    {
        get
        {
            var value = InitialInvestment;
            var monthlyRate = AnnualReturnRate / 100m / 12m;
            for (var month = 0; month < Years * 12; month++)
            {
                value = value * (1 + monthlyRate) + MonthlyInvestment;
            }

            return value;
        }
    }

    public decimal Principal => InitialInvestment + MonthlyInvestment * Years * 12;
    public decimal Profit => FutureValue - Principal;
}
