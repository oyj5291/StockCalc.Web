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
}
