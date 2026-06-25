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

    public IActionResult IsaAccountWorstInvestmentMistake() => View();

    public IActionResult StockInvestmentProcessGuide() => View();

    public IActionResult StockBeginnerAvoidTradingHours() => View();

    public IActionResult SpacexSuperIpoMarketImpact() => View();

    public IActionResult AiEraValuablePeopleAndAssetStrategy() => View();

    public IActionResult OddLotTradingMarketManipulationWarning() => View();

    public IActionResult JosephKennedyShoeshineBoyTheory() => View();

    public IActionResult OpenRangeBreakoutTradingGuide() => View();

    public IActionResult WhyStockDoesNotRiseWhenRetailBuying() => View();
}
