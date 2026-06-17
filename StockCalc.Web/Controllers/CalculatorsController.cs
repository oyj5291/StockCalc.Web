using Microsoft.AspNetCore.Mvc;
using StockCalc.Web.Models;

namespace StockCalc.Web.Controllers;

public class CalculatorsController : Controller
{
    [HttpGet]
    public IActionResult Profit()
    {
        return View(new ProfitCalculatorViewModel
        {
            BuyPrice = 50000,
            SellPrice = 56000,
            Quantity = 100,
            FeeRate = 0.015m,
            TaxRate = 0.18m
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Profit(ProfitCalculatorViewModel model)
    {
        return View(model);
    }

    [HttpGet]
    public IActionResult AveragePrice()
    {
        return View(new AveragePriceCalculatorViewModel
        {
            ExistingPrice = 60000,
            ExistingQuantity = 50,
            AdditionalPrice = 52000,
            AdditionalQuantity = 30
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AveragePrice(AveragePriceCalculatorViewModel model)
    {
        return View(model);
    }

    [HttpGet]
    public IActionResult WaterAverage()
    {
        return View(new WaterAverageCalculatorViewModel
        {
            CurrentAveragePrice = 65000,
            CurrentQuantity = 40,
            CurrentPrice = 52000,
            AdditionalAmount = 1000000
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult WaterAverage(WaterAverageCalculatorViewModel model)
    {
        return View(model);
    }

    [HttpGet]
    public IActionResult StopLoss()
    {
        return View(new StopLossCalculatorViewModel { BuyPrice = 50000, LossRate = 8 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult StopLoss(StopLossCalculatorViewModel model)
    {
        return View(model);
    }

    [HttpGet]
    public IActionResult TargetPrice()
    {
        return View(new TargetPriceCalculatorViewModel { BuyPrice = 50000, TargetRate = 15 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TargetPrice(TargetPriceCalculatorViewModel model)
    {
        return View(model);
    }

    [HttpGet]
    public IActionResult Dividend()
    {
        return View(new DividendCalculatorViewModel
        {
            Quantity = 100,
            DividendPerShare = 1200,
            TaxRate = 15.4m
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Dividend(DividendCalculatorViewModel model)
    {
        return View(model);
    }

    [HttpGet]
    public IActionResult Compound()
    {
        return View(new CompoundCalculatorViewModel
        {
            InitialInvestment = 10000000,
            MonthlyInvestment = 500000,
            AnnualReturnRate = 7,
            Years = 10
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Compound(CompoundCalculatorViewModel model)
    {
        return View(model);
    }
}
