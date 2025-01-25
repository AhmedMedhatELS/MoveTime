using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Utility
{
    public enum ImageLocation
    {
        Users,
        Products,
        Childrens,
        Disclaimers,
        Bills,
        DisclaimerFile
    }
    public static class ImageManger
    {
        private static readonly Dictionary<ImageLocation, string> location = new()
        {
            {ImageLocation.Users, "wwwroot/images/users"},
            {ImageLocation.Products, "wwwroot/images/Products"},
            {ImageLocation.Childrens, "wwwroot/images/Childrens"},
            {ImageLocation.Disclaimers, "wwwroot/images/Disclaimers"},
            {ImageLocation.Bills, "wwwroot/images/Bills"},
            {ImageLocation.DisclaimerFile, "wwwroot/files/documents"}
        };

        private static readonly List<string> imageExtensions =
                    [
                        ".jpg",
                        ".png"
                    ];

        public static string? SaveImage(IFormFile ImageFile,ImageLocation imageLocation)
        {
            string? ImageName = null;

            if (ImageFile != null)
            {
                //get the image extension
                var imageextension = Path.GetExtension(ImageFile.FileName);

                //cheack for the image extansion if its vailed
                if(string.IsNullOrEmpty(imageextension) || !imageExtensions.Contains(imageextension.ToLower())) return null;

                // Generate a unique file name without changing the extension
                ImageName  = $"{Guid.NewGuid()}{imageextension}";

                // Define the path to save the image
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), location[imageLocation], ImageName);

                //copy the image to the desired location
                using var fileStream = new FileStream(imagePath, FileMode.Create);
                ImageFile.CopyTo(fileStream);
            }

            return ImageName;
        }

        public static string? SaveFile(IFormFile File, ImageLocation fileLocation)
        {
            string? FileName = null;

            if (File != null)
            {
                //get the image extension
                var fileextension = Path.GetExtension(File.FileName);

                // Generate a unique file name without changing the extension
                FileName = $"{Guid.NewGuid()}{fileextension}";

                // Define the path to save the file
                var FilePath = Path.Combine(Directory.GetCurrentDirectory(), location[fileLocation], FileName);

                //copy the file to the desired location
                using var fileStream = new FileStream(FilePath, FileMode.Create);
                File.CopyTo(fileStream);
            }

            return FileName;
        }
        #region add list of images
        //public static List<string> SaveImageList(List<IFormFile> ImageFiles, ImageLocation imageLocation)
        //{
        //    List<string> ImageNames = [];

        //    foreach (IFormFile ImageFile in ImageFiles)
        //    {
        //        if (ImageFile != null)
        //        {
        //            // Generate a unique file name without changing the extension
        //            var currentImageName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";

        //            // Define the path to save the image
        //            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), location[imageLocation], currentImageName);

        //            //copy the image to the desired location
        //            using var fileStream = new FileStream(imagePath, FileMode.Create);
        //            ImageFile.CopyTo(fileStream);

        //            //Add Name To The List
        //            if(currentImageName != null)
        //                ImageNames.Add(currentImageName);
        //        }
        //    }
        //    return ImageNames;
        //}
        #endregion
        public static void DeleteImage(string ImageName, ImageLocation imageLocation)
        {
            if (!string.IsNullOrEmpty(ImageName))
            {
                var pathimg = Path.Combine(Directory.GetCurrentDirectory(), location[imageLocation], ImageName);
                FileInfo file = new(pathimg);
                if (file.Exists)
                    file.Delete();                
            }
        }
        #region delete list of images
        //public static void DeleteImageList(List<string> ImageNames, ImageLocation imageLocation)
        //{
        //    foreach (string ImageName in ImageNames)
        //    {
        //        if (!string.IsNullOrEmpty(ImageName))
        //        {
        //            var pathimg = Path.Combine(Directory.GetCurrentDirectory(), location[imageLocation], ImageName);
        //            FileInfo file = new(pathimg);
        //            if (file.Exists)
        //                file.Delete();
        //        }
        //    }
        //}
        #endregion
    }
}
