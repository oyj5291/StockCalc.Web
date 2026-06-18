using Microsoft.AspNetCore.Mvc;

namespace StockCalc.Web.Controllers;

public class ArticlesController : Controller
{
    public IActionResult BasicStockReturn()
    {
        return View();
    }

    public IActionResult AveragePriceGuide()
    {
        return View();
    }

    public IActionResult StockBacktestGuide()
    {
        return View();
    }

    public IActionResult SamsungTenPercentProfit() => View();

    public IActionResult SplitBuyCalculatorGuide() => View();

    public IActionResult StopLossCalculationGuide() => View();

    public IActionResult TargetReturnCalculation() => View();

    public IActionResult AfterTaxDividendCalculation() => View();

    public IActionResult StockTaxBasics() => View();

    public IActionResult EtfLongTermReturn() => View();

    public IActionResult AveragingDownCostBasis() => View();
}
