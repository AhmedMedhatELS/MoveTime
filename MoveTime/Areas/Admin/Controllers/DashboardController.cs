using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Models.ViewModels.Statistics;
using Shared;
using System.Runtime.Intrinsics.X86;
using Utility;

namespace MoveTime.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Supervisor")]
    public class DashboardController(IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        Statistics statistics) : Controller
    {
        #region Start Up
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly Statistics _statistics = statistics;

        //TempData["SuccessMessage"]
        //TempData["ErrorMessage"]
        //sunday = 0
        private readonly int CurrentDay = (int)(DateTime.Now.DayOfWeek);
        private readonly List<string> DaysName = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
        #endregion

        public IActionResult Index() 
        {
            var children = _unitOfWork.Repository<Child>().Get(
                e => !e.IsDeleted,
                sub => sub.ChildSubscriptions
                ).ToList();

            var checkinouts = _unitOfWork.Repository<CheckInOut>().Get(
                e =>
                    e.Status == CheckStatus.Out && 
                    e.DateLog > DateTime.Now.AddDays(-7).Date 
                    );

            HomeView view = new();

            if (children != null)
            {
                #region 1st section
                view.TotalKids = children.Count;
                view.BlackList = children.Count(e => e.BlackList);
                view.Approved = children.Count(e => e.IsApproved);
                view.NotApproved = children.Count(e => !e.IsApproved);
                #endregion
                #region 3rd section
                #region male/female
                int male = children.Count(e => e.Gender == Gender.Male);
                int female = children.Count(e => e.Gender == Gender.Female);

                // Calculating the percentage for each status
                int malePercent = (int)Math.Round((double)male / view.TotalKids * 100);
                int femalePercent = (int)Math.Round((double)female / view.TotalKids * 100);

                // Correcting the sum to be exactly 100
                int totalPercent = malePercent + femalePercent;
                int difference = 100 - totalPercent;

                if (difference != 0)
                {
                    // Adjust the largest category to make the total 100
                    if (malePercent >= femalePercent)
                        malePercent += difference;                    
                    else
                        femalePercent += difference;
                }

                view.MaleKids = malePercent;
                view.FemaleKids = femalePercent;
                #endregion
                #region male/female Sub
                foreach(var child in children)
                {
                    if(child.ChildSubscriptions.Any(e => e.Status == SubscriptionStatus.Active))
                    {
                        view.KidsSub++;
                        if (child.Gender == Gender.Male)
                            view.MaleSub++;
                        else
                            view.FemaleSub++;
                    }
                }

                // Calculating the percentage for each status
                view.MaleSub = (int)Math.Round((double)view.MaleSub / view.KidsSub * 100);
                view.FemaleSub = (int)Math.Round((double)view.FemaleSub / view.KidsSub * 100);

                // Correcting the sum to be exactly 100
                totalPercent = view.MaleSub + view.FemaleSub;
                difference = 100 - totalPercent;

                if (difference != 0)
                {
                    // Adjust the largest category to make the total 100
                    if (view.MaleSub >= view.FemaleSub)
                        view.MaleSub += difference;
                    else
                        view.FemaleSub += difference;
                }
                #endregion
                #endregion
            }

            if(checkinouts !=null)
            {
                int day = CurrentDay;

                for (int i = 0; i > -7; i--)
                {
                    var daychecks = checkinouts.Where(
                        e => e.DateLog == DateTime.Now.AddDays(i).Date).ToList();

                    if (daychecks.Count > 0)
                        view.WeekProfit.Add(daychecks.Sum(e => e.InTotal) + daychecks.Sum(e => e.OutTotal));
                    else
                        view.WeekProfit.Add(0);

                    view.WeekDaysName.Add(DaysName[day]);

                    day = day == 6 ? 0 : day + 1;
                }

            }
            else
            {
                for(int i = 0; i < 7; i++)
                {
                    view.WeekProfit.Add(0);
                    view.WeekDaysName.Add(DaysName[i]);
                }
            }

            return View(view);
        }
        #region Add New person to the system
        public async Task<IActionResult> NewPerson(AddWhich add)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.AddWhich = add;
            await _userManager.UpdateAsync(user);

            TempData["AddWhich"] = add.ToString();

            return RedirectToPage("/Account/Register", new { area = "Identity" });
        }
        #endregion
        #region Products        
        #region get all the products
        public IActionResult AllProducts() =>  
            View(new AdminProductview
                {Products = _unitOfWork.Repository<Product>().Get(e => !e.IsDeleted).ToList()});
        
        #endregion
        #region add new product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProduct(AdminProductview adminProductview)
        {   
            //check for the validation of the data
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "يرجى تصحيح الأخطاء في النموذج. تأكد من ملء جميع الحقول المطلوبة بشكل صحيح.";
                return RedirectToAction("AllProducts");
            }
            #region get the image name and save it and check for its extantion
            var imagegeneratedname = ImageManger.SaveImage(adminProductview.Imagefile, ImageLocation.Products);

            if (imagegeneratedname == null)
            {
                TempData["ErrorMessage"] = "فشل حفظ الصورة. يرجى التأكد من صحة تنسيق الصورة (JPEG، PNG) ثم حاول مرة أخرى.";
                return RedirectToAction("AllProducts");
            }
            #endregion
            #region add the new product
            var product = new Product
            {
                Name = adminProductview.Name,
                Price = adminProductview.Price,
                Quantity = adminProductview.Quantity,
                ImageName = imagegeneratedname
            };
            
            _unitOfWork.Repository<Product>().Add(product);
            TempData["SuccessMessage"] = "تم إضافة المنتج بنجاح!";
            #endregion
            return RedirectToAction("AllProducts");
        }
        #endregion
        #region edit product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(AdminProductview adminProductview)
        {
            #region get the product and check if it is exists
            var product = _unitOfWork.Repository<Product>().GetOne(
                e => e.ProductId == adminProductview.ProductId && !e.IsDeleted);

            if(product == null)
            {
                TempData["ErrorMessage"] = "لا يوجد منتج بهذه الموصفات للتعديل عليه";
                return RedirectToAction("AllProducts");
            }
            #endregion
            #region check for the valdaition of the data
            ModelState.Remove(nameof(adminProductview.Imagefile));

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "يرجى تصحيح الأخطاء في النموذج. تأكد من ملء جميع الحقول المطلوبة بشكل صحيح.";
                return RedirectToAction("AllProducts");
            }
            #endregion
            #region get the image name and save it and check for its extantion
            var imagegeneratedname = adminProductview.Imagefile != null ? 
                    ImageManger.SaveImage(adminProductview.Imagefile, ImageLocation.Products) :
                            "no";

            if (imagegeneratedname == null)
            {
                TempData["ErrorMessage"] = "فشل حفظ الصورة. يرجى التأكد من صحة تنسيق الصورة (JPEG، PNG) ثم حاول مرة أخرى.";
                return RedirectToAction("AllProducts");
            }
            #endregion
            #region save the changes for the product
            product.Price = adminProductview.Price;
            product.Name = adminProductview.Name;
            product.Quantity = adminProductview.Quantity;
            product.ImageName = imagegeneratedname == "no" ? product.ImageName : imagegeneratedname;

            _unitOfWork.Repository<Product>().Update(product);
            TempData["SuccessMessage"] = "تم تعديل على المنتج بنجاح!";
            #endregion
            return RedirectToAction("AllProducts");
        }
        #endregion
        #region delete product
        public IActionResult DeleteProduct(int id)
        {
            #region get the product and check if it is exists
            var product = _unitOfWork.Repository<Product>().GetOne(e => e.ProductId == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "لا يوجد منتج بهذه الموصفات للتعديل عليه";
                return RedirectToAction("AllProducts");
            }
            #endregion
            #region delete product

            product.IsDeleted = true;
            ImageManger.DeleteImage(product.ImageName,ImageLocation.Products);

            _unitOfWork.Repository<Product>().Update(product);
            TempData["SuccessMessage"] = "تم حذف المنتج بنجاح!";
            #endregion

            return RedirectToAction("AllProducts");
        }
        #endregion
        #endregion
        #region Document
        public IActionResult AddEditDocument() => View();
        [HttpPost]
        public IActionResult AddEditDocument(IFormFile file)
        {
            if (file != null)
            {
                var doc = _unitOfWork.Repository<Document>().Get(null).ToList();

                if (doc.Count > 0) _unitOfWork.Repository<Document>().RemoveRange(doc);

                foreach (var docN in doc)
                    ImageManger.DeleteImage(docN.DocumentName, ImageLocation.DisclaimerFile);

                var docName = ImageManger.SaveFile(file, ImageLocation.DisclaimerFile);

                if (docName != null)
                {
                    Document newDoc = new()
                    {
                        DocumentName = docName
                    };

                    _unitOfWork.Repository<Document>().Add(newDoc);
                    TempData["SuccessMessage"] = "تم أضافة الملف بنجاح";
                }
                else TempData["ErrorMessage"] = "حدث مشكلة فى حفظ الملف";
            }

            return View();
        }
        #endregion
        #region Statistics
        public IActionResult LogHistory() =>
            View(_statistics.LogHistory(DateTime.Now,DateTime.Now));
        #endregion
    }
}
