using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TransitPulse.Web.Controllers;
public class ServiceBusController : Controller
{
    // GET: ServiceBusController
    public ActionResult Index()
    {
        return View();
    }

    // GET: ServiceBusController/Details/5
    public ActionResult Details(int id)
    {
        return View();
    }

    // GET: ServiceBusController/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: ServiceBusController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: ServiceBusController/Edit/5
    public ActionResult Edit(int id)
    {
        return View();
    }

    // POST: ServiceBusController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: ServiceBusController/Delete/5
    public ActionResult Delete(int id)
    {
        return View();
    }

    // POST: ServiceBusController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
}
